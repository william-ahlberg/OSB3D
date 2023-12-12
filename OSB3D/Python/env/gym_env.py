import gymnasium as gym
import numpy as np
from mlagents_envs.base_env import ActionTuple
from mlagents_envs.environment import UnityEnvironment
import matplotlib.pyplot as plt
from env.unity_env import OSB3DEnv
from mlagents_envs.environment import UnityEnvironment

class GymEnv(gym.Env):
    def __init__(self, game_name, worker_id, seed, no_graphics):
        #Environment fields
        self.game_name = game_name
        self.worker_id = worker_id
        self.seed = seed
        self.no_graphics = no_graphics
        self.unity_env = UnityEnvironment(self.file_name, worker_id=worker_id,seed=self.seed, no_graphics=self.no_graphics)
        
    def step(self, action):
        action = np.asarray(action)
        action_tuple = ActionTuple()

        if (self.is_continuous):
            action = np.reshape(action, [1, self.action_size])
            action_tuple.add_continuous(action)
        else:
            action = np.reshape(action, [1, 1])
            action_tuple.add_discrete(action)

        self.unity_env.set_actions(self.behavior_name, action_tuple)
        self.unity_env.step()
        #decision_steps: obs, reward, agent_id, action_mask
        (decision_steps, terminal_steps) = self.unity_env.get_steps(self.behavior_name)

        if (terminal_steps.interrupted[0] == True):
            terminated = True
            reward = terminal_steps.reward[0]
        else:
            reward = decision_steps.reward[0]
            terminated = False

        info = self.__get_info()

        return observation, reward, terminated, False, info
    def reset(self):
        super().reset(seed=seed)

        self.unity_env.reset()
        decision_step, _ = self.unity_env.get_steps(self.name)
        info = self.__get_info()

        return observation, info
    def render(self):
        logger.warning("Could not render")
        return
    def close(self):
        self.unity_env.close()

    def __get_info(self):
        info = {
            "bugs_found": 0,
            "bugs_found_cumulative": 0,
            "area_covered": 0,
            "area_covered_cumulative": 0}

        return info
    def set_seed(self):
