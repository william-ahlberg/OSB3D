from mlagents_envs.environment import UnityEnvironment
import numpy as np
import matplotlib.pyplot as plt
import logging as logs
from mlagents_envs.base_env import ActionTuple


class OSB3DEnv:
    def __init__(self, file_name, worker_id, no_graphics, max_timestep, seed,):
        self.file_name = file_name
        self.seed = seed
        self.no_graphics = no_graphics
        self._max_episode_timesteps = max_timestep
        self.unity_env = UnityEnvironment(self.file_name, worker_id=worker_id,seed=self.seed, no_graphics=self.no_graphics)
        self.worker_id = worker_id
        self.behavior_name = "AgentBehavior?team=0"
        self.actions_for_episode = dict()
        self.episode = -1
        self.trajectories_for_episode = dict()
        self.config = None
        self.reward_weights = None

        self.unity_env.reset()
        self.coverage_of_points = []
        self.pos_buffer = dict()
        self.action_size = 5

    def execute(self, actions):
        actions = np.asarray(actions)
        action_tuple = ActionTuple()

        actions = np.reshape(actions, [1, self.action_size])

        action_tuple.add_continuous(actions)

        self.unity_env.set_actions(self.behavior_name, action_tuple)
        self.unity_env.step()
        (decision_steps, terminal_steps) = self.unity_env.get_steps(self.behavior_name)

        self.actions_for_episode[self.episode].append(actions) # Add field

        if (len(terminal_steps.interrupted) > 0):
            done = True
            reward = terminal_steps.reward[0]
        else:
            reward = decision_steps.reward[0]
            done = False

        self.previous_action = actions # Add field

        state = dict(global_in=np.concatenate(decision_steps.obs, axis=1))
        state['global_in'] = np.concatenate([state['global_in'], np.reshape(self.sample_weights,(1,3))],axis=1)
        state['global_in'] = state["global_in"].reshape((len(state["global_in"][0])))
        position = state['global_in'][-6:-3]
        #self.trajectories_for_episode[self.episode].append(np.concatenate([position, state['global_in'][-2:]])) #Add field
        self.trajectories_for_episode[self.episode].append(position)

        return state, done, reward

    def reset(self):
        self.reward_weights = self.config['reward_weights']
        self.win_weight = self.config['win_weight']
        self.sample_weights = self.reward_weights[np.random.randint(len(self.reward_weights))]
        self.sample_win = self.win_weight[np.random.randint(len(self.win_weight))]
        self.sample_weights = [self.sample_win, self.sample_weights, 1 - self.sample_weights]

        unity_config = dict()
        for key in self.config.keys():
            if key != "reward_weights" and key != 'win_weight':
                unity_config[key] = self.config[key]

        self.previous_action = [0, 0]
        logs.getLogger("mlagents.envs").setLevel(logs.WARNING)
        self.coverage_of_points.append(len(self.pos_buffer.keys()))
        self.episode += 1
        self.trajectories_for_episode[self.episode] = []
        self.actions_for_episode[self.episode] = []

        (decision_steps, terminal_steps) = self.unity_env.get_steps(self.behavior_name)

        self.unity_env.reset()

        state = dict(global_in=np.concatenate(decision_steps.obs, axis=1))
        # Append the value of the motivation weight

        state['global_in'] = np.concatenate([state['global_in'], np.reshape(self.sample_weights,(1,3))],axis=1)
        state['global_in'] = state["global_in"].reshape((len(state["global_in"][0])))
        position = state['global_in'][-6:-3]

        print(position)
        self.trajectories_for_episode[self.episode].append(np.concatenate([position, state['global_in'][-2:]]))
        return state

    def close(self):
       self.unity_env.close()

    def set_config(self, config):
        self.config = config

    def clear_buffers(self):
        self.trajectories_for_episode = dict()
        self.actions_for_episode = dict()

    def entropy(self, probs):
        entr = 0
        for p in probs:
            entr += (p * np.log(p))

        return -entr
