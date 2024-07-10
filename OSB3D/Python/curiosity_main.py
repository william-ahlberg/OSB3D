# docs and experiment results can be found at https://docs.cleanrl.dev/rl-algorithms/ppo/#ppo_continuous_actionpy
import os
import random
import time
from dataclasses import dataclass

import numpy as np
import torch
import torch.nn as nn
import torch.optim as optim
import tyro
from torch.utils.tensorboard import SummaryWriter

from env.osb3d_env import OSB3DEnv
from agent.agent import CuriosityAgent
import wandb


@dataclass
class Args:
    exp_name: str = os.path.basename(__file__)[: -len(".py")]
    """the name of this experiment"""
    seed: int = 1337
    """seed of the experiment"""
    torch_deterministic: bool = True
    """if toggled, `torch.backends.cudnn.deterministic=False`"""
    cuda: bool = True
    """if toggled, cuda will be enabled by default"""
    track: bool = True
    """if toggled, this experiment will be tracked with Weights and Biases"""
    wandb_project_name: str = "CuriosityAgent"
    """the wandb's project name"""
    wandb_entity: str = None
    """the entity (team) of wandb's project"""
    capture_video: bool = False
    """whether to capture videos of the agent performances (check out `videos` folder)"""

    # Algorithm specific arguments
    env_id: str = "OSB3D-v0"
    """the id of the environment"""
    total_timesteps: int = 30_000_000
    """total timesteps of the experiments"""
    learning_rate: float = 3e-4
    """the learning rate of the optimizer"""
    num_envs: int = 1
    """the number of parallel game environments"""
    num_timesteps: int = 2000
    """the number of steps to run in each environment per policy rollout"""
    anneal_lr: bool = True
    """Toggle learning rate annealing for policy and value networks"""
    gamma: float = 0.99
    """the discount factor gamma"""
    gae_lambda: float = 0.95
    """the lambda for the general advantage estimation"""
    num_minibatches: int = 32
    """the number of mini-batches"""
    update_epochs: int = 10
    """the K epochs to update the policy"""
    norm_adv: bool = True
    """Toggles advantages normalization"""
    clip_coef: float = 0.2
    """the surrogate clipping coefficient"""
    clip_vloss: bool = True
    """Toggles whether or not to use a clipped loss for the value function, as per the paper."""
    ent_coef: float = 0.0
    """coefficient of the entropy"""
    vf_coef: float = 0.5
    """coefficient of the value function"""
    max_grad_norm: float = 0.5
    """the maximum norm for the gradient clipping"""
    target_kl: float = None
    """the target KL divergence threshold"""

    # to be filled in runtime
    batch_size: int = 0
    """the batch size (computed in runtime)"""
    minibatch_size: int = 0
    """the mini-batch size (computed in runtime)"""
    num_iterations: int = 0
    """the number of iterations (computed in runtime)"""

    configuration_file: str = "config/curiosity.yaml"
    game_name: str = "../Build/"
    worker_id: int = 1
    num_episodes: int = 15000

def main():

    args = tyro.cli(Args)
    args.batch_size = int(args.num_envs * args.num_timesteps)
    args.minibatch_size = int(args.batch_size // args.num_minibatches)
    args.num_iterations = args.total_timesteps // args.batch_size
    run_name = f"{args.env_id}__{args.exp_name}__{args.seed}__{int(time.time())}"

    bug_cumulative = []

    if args.track:
        import wandb

        wandb.init(
            project=args.wandb_project_name,
            entity=args.wandb_entity,
            sync_tensorboard=True,
            config=vars(args),
            name=run_name,
            monitor_gym=False,
            save_code=True,
        )

    writer = SummaryWriter(f"runs/{run_name}")
    writer.add_text(
        "hyperparameters",
        "|param|value|\n|-|-|\n%s" % ("\n".join([f"|{key}|{value}|" for key, value in vars(args).items()])),
    )

    # TRY NOT TO MODIFY: seeding
    random.seed(args.seed)
    np.random.seed(args.seed)
    torch.manual_seed(args.seed)
    torch.backends.cudnn.deterministic = args.torch_deterministic

    device = torch.device("cuda" if torch.cuda.is_available() and args.cuda else "cpu")

    # env setup
    env = OSB3DEnv(game_name=args.game_name,
                   worker_id=args.worker_id,
                   no_graphics=True,
                   seed=args.seed,
                   max_episode_timestep=2000,
                   config_file=args.configuration_file)

    agent = CuriosityAgent(env).to(device)
    optimizer = optim.Adam(agent.parameters(), lr=args.learning_rate, eps=1e-5)

    # ALGO Logic: Storage setup
    obs = torch.zeros((args.num_timesteps, args.num_envs) + (8,)).to(device)
    actions = torch.zeros((args.num_timesteps, args.num_envs) + (6,)).to(device)
    logprobs = torch.zeros((args.num_timesteps, args.num_envs)).to(device)
    rewards = torch.zeros((args.num_timesteps, args.num_envs)).to(device)
    dones = torch.zeros((args.num_timesteps, args.num_envs)).to(device)
    values = torch.zeros((args.num_timesteps, args.num_envs)).to(device)

    # TRY NOT TO MODIFY: start the game
    total_timesteps = 0
    start_time = time.time()
    next_obs, _ = env.reset()
    next_obs = torch.Tensor(next_obs).to(device)
    next_done = torch.zeros(args.num_envs).to(device)

    for episode in range(1, args.num_episodes + 1):
        obs = torch.zeros((args.num_timesteps, args.num_envs) + (8,)).to(device)
        actions = torch.zeros((args.num_timesteps, args.num_envs) + (6,)).to(device)
        logprobs = torch.zeros((args.num_timesteps, args.num_envs)).to(device)
        rewards = torch.zeros((args.num_timesteps, args.num_envs)).to(device)
        dones = torch.zeros((args.num_timesteps, args.num_envs)).to(device)
        values = torch.zeros((args.num_timesteps, args.num_envs)).to(device)
        # Annealing the rate if instructed to do so.
        if args.anneal_lr:
            frac = 1.0 - (episode - 1.0) / args.num_iterations
            lrnow = frac * args.learning_rate
            optimizer.param_groups[0]["lr"] = lrnow

        for timestep in range(0, args.num_timesteps):
            total_timesteps += 1
            next_obs = next_obs.reshape((-1,) + (8,))
            obs[timestep] = next_obs
            dones[timestep] = next_done
            position = next_obs[0][-4:-1].cpu().numpy()

            if timestep == 0:
                agent.add_position(position)
            elif next_obs[0][-1]:
                agent.update_buffer(position)

            # ALGO LOGIC: action logic
            with torch.no_grad():
                action, logprob, _, value = agent.get_action_and_value(next_obs)
                values[timestep] = value.flatten()
            actions[timestep] = action
            logprobs[timestep] = logprob

            # TRY NOT TO MODIFY: execute the game and log data.
            next_obs, _, termination, truncation, infos = env.step(action.cpu().numpy())
            reward = agent.get_reward(position)
            next_done = np.array(np.logical_or(termination, truncation), dtype=np.float32)
            rewards[timestep] = torch.tensor(reward).to(device).view(-1)
            next_obs, next_done = torch.Tensor(next_obs).to(device), torch.Tensor(next_done).to(device)

            if next_done:
                print("Episode done!")
                agent.trajectories.append(agent.trajectory)
                agent.trajectory = []
                env.spawn_point = agent.spawn_point
                observation, info = env.reset()
                bug_cumulative.append(info["bugs_found_cumulative"])
                break

        # bootstrap value if not done

        with torch.no_grad():
            next_value = agent.get_value(next_obs).reshape(1, -1)
            advantages = torch.zeros_like(rewards).to(device)
            lastgaelam = 0
            for t in reversed(range(args.num_timesteps)):
                if t == args.num_timesteps - 1:
                    nextnonterminal = 1.0 - next_done
                    nextvalues = next_value
                else:
                    nextnonterminal = 1.0 - dones[t + 1]
                    nextvalues = values[t + 1]
                delta = rewards[t] + args.gamma * nextvalues * nextnonterminal - values[t]
                advantages[t] = lastgaelam = delta + args.gamma * args.gae_lambda * nextnonterminal * lastgaelam
            returns = advantages + values

        # flatten the batch
        b_obs = obs.reshape((-1,) + (8,))
        b_logprobs = logprobs.reshape(-1)
        b_actions = actions.reshape((-1,) + (6,))
        b_advantages = advantages.reshape(-1)
        b_returns = returns.reshape(-1)
        b_values = values.reshape(-1)

        # Optimizing the policy and value network
        b_inds = np.arange(args.batch_size)
        clipfracs = []
        for epoch in range(args.update_epochs):
            np.random.shuffle(b_inds)
            for start in range(0, args.batch_size, args.minibatch_size):
                end = start + args.minibatch_size
                mb_inds = b_inds[start:end]

                _, newlogprob, entropy, newvalue = agent.get_action_and_value(b_obs[mb_inds], b_actions[mb_inds])
                logratio = newlogprob - b_logprobs[mb_inds]
                ratio = logratio.exp()

                with torch.no_grad():
                    # calculate approx_kl http://joschu.net/blog/kl-approx.html
                    old_approx_kl = (-logratio).mean()
                    approx_kl = ((ratio - 1) - logratio).mean()
                    clipfracs += [((ratio - 1.0).abs() > args.clip_coef).float().mean().item()]

                mb_advantages = b_advantages[mb_inds]
                if args.norm_adv:
                    mb_advantages = (mb_advantages - mb_advantages.mean()) / (mb_advantages.std() + 1e-8)

                # Policy loss
                pg_loss1 = -mb_advantages * ratio
                pg_loss2 = -mb_advantages * torch.clamp(ratio, 1 - args.clip_coef, 1 + args.clip_coef)
                pg_loss = torch.max(pg_loss1, pg_loss2).mean()

                # Value loss
                newvalue = newvalue.view(-1)
                if args.clip_vloss:
                    v_loss_unclipped = (newvalue - b_returns[mb_inds]) ** 2
                    v_clipped = b_values[mb_inds] + torch.clamp(
                        newvalue - b_values[mb_inds],
                        -args.clip_coef,
                        args.clip_coef,
                    )
                    v_loss_clipped = (v_clipped - b_returns[mb_inds]) ** 2
                    v_loss_max = torch.max(v_loss_unclipped, v_loss_clipped)
                    v_loss = 0.5 * v_loss_max.mean()
                else:
                    v_loss = 0.5 * ((newvalue - b_returns[mb_inds]) ** 2).mean()

                entropy_loss = entropy.mean()
                loss = pg_loss - args.ent_coef * entropy_loss + v_loss * args.vf_coef

                optimizer.zero_grad()
                loss.backward()
                nn.utils.clip_grad_norm_(agent.parameters(), args.max_grad_norm)
                optimizer.step()

            if args.target_kl is not None and approx_kl > args.target_kl:
                break

        y_pred, y_true = b_values.cpu().numpy(), b_returns.cpu().numpy()
        var_y = np.var(y_true)
        explained_var = np.nan if var_y == 0 else 1 - np.var(y_true - y_pred) / var_y

        # TRY NOT TO MODIFY: record rewards for plotting purposes
        writer.add_scalar("charts/learning_rate", optimizer.param_groups[0]["lr"], total_timesteps)
        writer.add_scalar("losses/value_loss", v_loss.item(), total_timesteps)
        writer.add_scalar("losses/policy_loss", pg_loss.item(), total_timesteps)
        writer.add_scalar("losses/entropy", entropy_loss.item(), total_timesteps)
        writer.add_scalar("losses/old_approx_kl", old_approx_kl.item(), total_timesteps)
        writer.add_scalar("losses/approx_kl", approx_kl.item(), total_timesteps)
        writer.add_scalar("losses/clipfrac", np.mean(clipfracs), total_timesteps)
        writer.add_scalar("losses/explained_variance", explained_var, total_timesteps)
        print("SPS:", int(total_timesteps / (time.time() - start_time)))
        writer.add_scalar("charts/SPS", int(total_timesteps / (time.time() - start_time)), total_timesteps)
        writer.add_scalar("Metrics/Episodic_Return", rewards.sum().item(), episode)
        writer.add_scalar("Metrics/Episodic_Length", timestep, episode)
        print(f"Episode: {episode}, total_timesteps: {total_timesteps}, episode_return: {rewards.sum().item()}")
    env.close()
    writer.close()


if __name__ == "__main__":

    main()