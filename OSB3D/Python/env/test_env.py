from tkinter import W
import numpy as np
import logging as logs
from mlagents_envs.base_env import ActionTuple
import yaml
import gymnasium as gym
import numpy as np
from mlagents_envs.base_env import ActionTuple
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.side_channel.environment_parameters_channel import EnvironmentParametersChannel
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfig
import uuid
from typing import TypeVar
from typing import List
import json
import time
from mlagents_envs.side_channel.side_channel import (
    SideChannel,
    IncomingMessage,
    OutgoingMessage,
)
from pathlib import Path
import os


class OSB3DEnv(gym.Env):
    def __init__(self, game_name, worker_id, no_graphics, seed, max_episode_timestep, config_file):
        print("""
        ===============================================      
        ====██████╗ ███████╗██████╗ ██████╗ ██████╗==== 
        ===██╔═══██╗██╔════╝██╔══██╗╚════██╗██╔══██╗===
        ===██║   ██║███████╗██████╔╝ █████╔╝██║  ██║===
        ===██║   ██║╚════██║██╔══██╗ ╚═══██╗██║  ██║===
        ===╚██████╔╝███████║██████╔╝██████╔╝██████╔╝===
        ====╚═════╝ ╚══════╝╚═════╝ ╚═════╝ ╚═════╝==== 
        ===============================================
        """)

        print("Starting Environment...")
        print("You can know start the Unity scene")
        self.game_name = game_name
        self.seed = seed
        self.no_graphics = no_graphics
        self._max_episode_timestep = max_episode_timestep
        self.timesteps = 0
        self.config_file = config_file
        self.config = {}

        self.side_channels = []

        self.behavior_name = "AgentBehavior?team=0"
        print(self.side_channels)
        self.unity_env = UnityEnvironment(self.game_name,
                                          worker_id=worker_id,
                                          seed=self.seed,
                                          no_graphics=self.no_graphics,
                                          side_channels=self.side_channels, )

        self.unity_env.reset()

    def step(self, action):
        action = np.asarray(action)
        action_tuple = ActionTuple()
        if (self.unity_env.behavior_specs[self.behavior_name].action_spec.is_continuous()):
            action = np.reshape(action, [1, self.action_size])
            action_tuple.add_continuous(action)
        else:
            action = np.reshape(action, [1, 1])
            action_tuple.add_discrete(action)

        self.unity_env.set_actions(self.behavior_name, action_tuple)
        self.unity_env.step()
        # decision_steps: obs, reward, agent_id, action_mask
        (decision_steps, terminal_steps) = self.unity_env.get_steps(self.behavior_name)

        if (terminal_steps.interrupted == True):
            print("done")
            terminated = True
            reward = terminal_steps.reward[0]
        else:
            reward = decision_steps.reward[0]
            terminated = False
        observation = decision_steps.obs
        # self.trajectories[self.episode].append(list(observation[-1][0]))
        self.timesteps += 1
        if (self.timesteps > self._max_episode_timestep):
            terminated = True

        return observation, reward, terminated, False, dict()

    def reset(self):
        super().reset(seed=self.seed)
        self.import_bugdata()
        if (self.episode != -1):
            info = self._get_info()
        else:
            info = None
            # self.env_size = self.info_channel.message_log.pop(0)
            print("Environment size: ", self.env_size)
        self.unity_env.reset()
        self.episode += 1

        print("New episode!", self.episode)
        self.trajectories[self.episode] = []
        decision_step, _ = self.unity_env.get_steps(self.behavior_name)
        observation = decision_step.obs
        self.bugs_found = 0
        self.timesteps = 0

        return observation, info

    def render(self):
        raise NotImplementedError

        logger.warning("Could not render")
        return

    def close(self):
        self.unity_env.close()



class DiscreteEnvironment():

    def __init__(self, min_x, max_x, min_y, max_y, min_z, max_z) -> None:
        self.max_x = max_x
        self.max_x = max_y
        self.max_x = max_z
        self.min_x = min_x
        self.min_x = min_y
        self.min_x = min_z
        self.x_disc = x_disc
        self.y_disc = y_disc
        self.z_disc = z_disc


class SensorSideChannel(SideChannel):
    T = TypeVar("T")

    def __init__(self, config) -> None:
        super().__init__(uuid.UUID("aa97d987-4c42-4878-b597-3de40edf66a6"))
        self.config = config

    def on_message_received(self, msg: IncomingMessage) -> None:
        print(msg.read_string())

    def set_sensor_parameter(self):
        if "observation_space_settings" not in self.config.keys():
            print("No sensor configuration was defined, using default values!")
        else:
            sensor_config = self.config["observation_space_settings"]
            for key1, value1 in sensor_config.items():
                self.send_typed_message(key1, True)
                for key2, value2 in value1.items():
                    self.send_typed_message(key2, value2)

    def send_typed_message(self, key, value):
        msg = OutgoingMessage()
        msg.write_string(key)

        if isinstance(value, str):
            msg.write_string(value)

        elif isinstance(value, bool):
            msg.write_bool(value)

        elif isinstance(value, int):
            msg.write_int32(value)

        elif isinstance(value, float):
            msg.write_float32(value)

        elif isinstance(value, list):
            msg.write_int32(len(value))
            for i in value:
                msg.write_string(i)

        super().queue_message_to_send(msg)





