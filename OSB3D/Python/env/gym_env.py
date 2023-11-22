import gymnasium as gym
import numpy as np
from mlagents_envs.base_env import ActionTuple
from mlagents_envs.environment import UnityEnvironment
import matplotlib.pyplot as plt
from env.unity_env import OSB3DEnv

class GymEnv(gym.Env):
    def __init__(self):
        self.action_space
        self.observation_space
        self.spec
        self.metadata
        self.unity_env


    def step(self, action):
        



        return observation, reward, terminated, truncated, info
    def reset(self):

        self.unity_env.reset()
        return observation, info
    def render(self):
        pass
    def close(self):
        self.unity_env.close
