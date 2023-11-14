from mlagents_envs.environment import UnityEnvironment
import numpy as np
import matplotlib.pyplot as plt
import logging as logs

class OSB3DEnv:
    def __init__(self, file_name, worker_id, no_graphics, max_timestep, seed,):
        self.file_name = file_name
        self.seed = seed
        self.no_graphics = no_graphics
        self.max_timestep = max_timestep
        self.unity_env = UnityEnvironment(self.file_name, self.seed, self.no_graphics)
        self.worker_id = worker_id
        self.behavior_name = "AgentBehavior?team=0"
        self.actions_for_episode = dict()
        self.episode = -1
        self.trajectories_for_episode = dict()
        self.reward_weights = None
        self.unity_env.reset()
        self.coverage_of_points = []
        self.pos_buffer = dict()


    def execute(self, actions):

        print(actions)
        action_tuple = ActionTuple()

        actions = np.asarray(actions)
        actions = np.reshape(actions, [1, 1])

        action_tuple.add_discrete(actions)

        self.unity_env.set_actions(self.behavior_name, action_tuple)
        self.unity_env.step()
        (decision_steps, terminal_steps) = self.unity_env.get_steps(self.behavior_name)

        self.actions_for_episode(self.episode).append(actions) # Add field

        # Not done
        done = True
        reward = terminal_steps.reward[0]
        # Done
        reward = decision_steps.reward[0]
        done = False

        self.previous_action = actions # Add field

        state = dict(global_in=decision_steps.obs[0])
        state['global_in'] = np.concatenate([state['global_in'], self.sample_weights]) # Add field
        position = state['global_in'][:5]
        self.trajectories_for_episode[self.episode].append(np.concatenate([position, state['global_in'][-2:]])) #Add field

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

        state = dict(global_in=decision_steps.obs[0])
        # Append the value of the motivation weight
        print(self.sample_weights)
        print(state["global_in"])
        state['global_in'] = np.concatenate([state['global_in'], self.sample_weights])
        position = state['global_in'][:5]
        self.trajectories_for_episode[self.episode].append(np.concatenate([position, state['global_in'][-2:]]))
        return state

    def close(self):
       self.unity_env.close()

    def set_config(self, config):
        self.config = config

    def clear_buffers(self):
        self.trajectories_for_episode = dict()
        self.actions_for_episode = dict()
