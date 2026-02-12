import numpy as np
import random
import os
import json
from osb3d_utils import OSB3DUtils
import uuid
osb3d_utils = OSB3DUtils()
import torch
import torch.nn as nn
from torch.distributions.normal import Normal
from gym.spaces import Box, Discrete, Dict, MultiDiscrete, MultiBinary
from typing import Any, Mapping, Sequence

class RandomMonkeyAgent:
    def __init__(self, observation_size=0,
                 action_size=np.zeros((1, 1)).shape,
                 is_continuous=True, ):

        self.action_size = action_size
        self.observation_size = observation_size
        self.is_continuous = is_continuous
        self._spawn_point = (0, 0, 0)
        self.discretization_threshold = 5
        self.position_buffer = {"positions": [],
                                "visitation_count": []}
        self._action = np.zeros((self.action_size))
        self.trajectory = []
        self.trajectories = []

    @property
    def action(self):
        if self.is_continuous:
            action = 2 * np.random.random_sample(size=self.action_size) - 1
            return action

    def save_trajectory(self):
        data = {}
        # print(self.trajectories)
        for i, rollout in enumerate(self.trajectories):
            rollout_list = [position.tolist() for position in rollout]
            data[i] = rollout_list
        with open(osb3d_utils.persistent_datapath() + "/rollouts.json" + "_" + osb3d_utils.get_unique_id(), "w") as f:
            json.dump({"Rollouts": data}, f, indent=4)

    def increment_position_count(self, index):
        self.position_buffer["visitation_count"][index] += 1

    def update_buffer(self, position):
        position_buffer_array = np.array(self.position_buffer["positions"])
        distances = np.linalg.norm(position - position_buffer_array, axis=1)
        threshold_mask = distances >= self.discretization_threshold
        if threshold_mask.all():
            self.add_position(position)
        else:
            closest_position = distances.argmin()
            self.increment_position_count(closest_position)

    def add_position(self, position):
        self.position_buffer["positions"].append(position)
        self.position_buffer["visitation_count"].append(0)
        self.increment_position_count(-1)

    @property
    def spawn_point(self):
        visitation_count = self.position_buffer["visitation_count"]
        sum_of_inverse = sum(1 / n for n in visitation_count)
        position_probability = [count / sum_of_inverse for count in visitation_count]
        position_index = random.choices(population=range(len(visitation_count)), weights=position_probability)
        return self.position_buffer["positions"][position_index[0]]

    @spawn_point.setter
    def spawn_point(self, value) -> None:
        self._spawn_point = value

class CuriosityAgent(nn.Module):

    def __init__(self, env, observation_size=0, action_size=np.zeros((1, 1)).shape, is_continuous=True,):

        super().__init__()
        self.critic = nn.Sequential(
            self.layer_init(nn.Linear(np.array((8,)).prod(), 64)),
            nn.Tanh(),
            self.layer_init(nn.Linear(64, 64)),
            nn.Tanh(),
            self.layer_init(nn.Linear(64, 1), std=1.0),
        )
        self.actor_mean = nn.Sequential(
            self.layer_init(nn.Linear(np.array((8,1)).prod(), 64)),
            nn.Tanh(),
            self.layer_init(nn.Linear(64, 64)),
            nn.Tanh(),
            self.layer_init(nn.Linear(64, np.prod((6, ))), std=0.01),
        )
        self.actor_logstd = nn.Parameter(torch.zeros(1, np.prod((6,))))

        self.action_size = action_size
        self.observation_size = observation_size
        self.is_continuous = is_continuous
        self._spawn_point = (0, 0, 0)
        self.discretization_threshold = 5
        self.position_buffer = {"positions": [],
                                "visitation_count": []}
        self._action = np.zeros((self.action_size))
        self.trajectory = []
        self.trajectories = []
        self.max_counter = 100
        self.r_max = 1

    def layer_init(self, layer, std=np.sqrt(2), bias_const=0.0):
        torch.nn.init.orthogonal_(layer.weight, std)
        torch.nn.init.constant_(layer.bias, bias_const)
        return layer

    @property
    def action(self):
        if self.is_continuous:
            action = 2 * np.random.random_sample(size=self.action_size) - 1
            return action

    def save_trajectory(self):
        data = {}
        # print(self.trajectories)
        for i, rollout in enumerate(self.trajectories):
            rollout_list = [position.tolist() for position in rollout]
            data[i] = rollout_list
        with open(osb3d_utils.persistent_datapath() + "/rollouts.json" + "_" + osb3d_utils.get_unique_id(), "w") as f:
            json.dump({"Rollouts": data}, f, indent=4)

    def increment_position_count(self, index):
        self.position_buffer["visitation_count"][index] += 1

    def update_buffer(self, position):
        position_buffer_array = np.array(self.position_buffer["positions"])
        distances = np.linalg.norm(position - position_buffer_array, axis=1)
        threshold_mask = distances >= self.discretization_threshold
        if threshold_mask.all():
            self.add_position(position)
        else:
            closest_position = distances.argmin()
            self.increment_position_count(closest_position)

    def add_position(self, position):
        self.position_buffer["positions"].append(position)
        self.position_buffer["visitation_count"].append(0)
        self.increment_position_count(-1)

    @property
    def spawn_point(self):
        visitation_count = self.position_buffer["visitation_count"]
        sum_of_inverse = sum(1 / n for n in visitation_count)
        position_probability = [count / sum_of_inverse for count in visitation_count]
        position_index = random.choices(population=range(len(visitation_count)), weights=position_probability)
        return self.position_buffer["positions"][position_index[0]]

    @spawn_point.setter
    def spawn_point(self, value) -> None:
        self._spawn_point = value

    def get_reward(self, position):
        position_buffer_array = np.array(self.position_buffer["positions"])
        distances = np.linalg.norm(position - position_buffer_array, axis=1)
        closest_position = distances.argmin()
        return np.max((0,self.r_max * (1 - self.position_buffer["visitation_count"][closest_position] / self.max_counter)))

    def get_value(self, x):
        return self.critic(x)

    def get_action_and_value(self, x, action=None):
        action_mean = self.actor_mean(x)
        action_logstd = self.actor_logstd.expand_as(action_mean)
        action_std = torch.exp(action_logstd)
        probs = Normal(action_mean, action_std)
        if action is None:
            action = probs.sample()
        return action, probs.log_prob(action).sum(1), probs.entropy().sum(1), self.critic(x)

class Agent:

    def __init__(self, algorithm, observation_space: Dict, action_space: Dict, config):
        super().__init__()
        self.algorithm = algorithm
        self.observation_space = observation_space
        self.action_space = action_space
        # Try to move algorithm to device if it supports `.to()`
        if hasattr(self.algorithm, "to"):
            try:
                self.algorithm.to(self.device)
            except Exception:
                # best-effort: ignore if algorithm can't be moved now
                pass


    def train(self, data):
        return self.algorithm.train(data)




