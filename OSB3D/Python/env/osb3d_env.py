from tkinter import W
from mlagents_envs.environment import UnityEnvironment
import numpy as np
import matplotlib.pyplot as plt
import logging as logs
from mlagents_envs.base_env import ActionTuple
import yaml
import gymnasium as gym
import numpy as np
from mlagents_envs.base_env import ActionTuple
from mlagents_envs.environment import UnityEnvironment
import matplotlib.pyplot as plt
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.side_channel.environment_parameters_channel import EnvironmentParametersChannel
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfig
from eval.eval import OSB3DEval
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

class OSB3DEnv(gym.Env):
    def __init__(self, game_name, worker_id, no_graphics, seed, max_episode_timestep, config_file):

#===============================================      
#====██████╗ ███████╗██████╗ ██████╗ ██████╗==== 
#===██╔═══██╗██╔════╝██╔══██╗╚════██╗██╔══██╗===
#===██║   ██║███████╗██████╔╝ █████╔╝██║  ██║===
#===██║   ██║╚════██║██╔══██╗ ╚═══██╗██║  ██║===
#===╚██████╔╝███████║██████╔╝██████╔╝██████╔╝===
#====╚═════╝ ╚══════╝╚═════╝ ╚═════╝ ╚═════╝==== 
#===============================================

        print("Starting Environment...")
        time.sleep(3)
        print("You can know start the Unity scene")
        self.game_name = game_name
        self.seed = seed
        self.no_graphics = no_graphics
        self._max_episode_timestep = max_episode_timestep
        self.timesteps = 0
        self.config_file = config_file
        self.config = {}

        self.set_config()
        self.engine_channel = None
        self.parameter_channel = None
        self.sensor_channel = None
        self.action_channel = None
        self.info_channel = None
        self.bug_channel = None
        self.set_engine_channel()
        self.set_env_channel()
        self.set_sensor_channel()
        self.set_action_channel()
        self.set_info_channel()
        self.set_bug_channel()
        self.side_channels = [self.engine_channel,
                              self.parameter_channel,
                              self.sensor_channel,
                              self.action_channel,
                              self.info_channel,
                              self.bug_channel]

        self.behavior_name = "AgentBehavior?team=0"

        self.unity_env = UnityEnvironment(self.game_name, worker_id=worker_id,seed=self.seed, no_graphics=self.no_graphics,side_channels=self.side_channels)
        self.worker_id = worker_id
        self.actions_for_episode = dict()
        self.episode = -1
        self.trajectories_for_episode = dict()
        self.reward_weights = None

        self.unity_env.reset()

        self.coverage_of_points = []
        self.pos_buffer = dict()
        self.action_size = 6 
        self.trajectory = []
        self.trajectories = dict()
        self.bug_data = self.import_bugdata() 
        
        self.bugs_found = 0
        self.bugs_found_cumulative = 0
        self.env_size = 0 
        
        
        self._bug_positions = np.zeros((len(self.bug_data["BugLog"]),3))
        
        for index, bug in enumerate(self.bug_data["BugLog"]):
            x = bug["position"]["x"]
            y = bug["position"]["y"]
            z = bug["position"]["z"]
            self._bug_positions[index,:] = (x,y,z)

        self._spawn_point = [0,0,0]
        
    def set_engine_channel(self):
        self.engine_channel = EngineConfigurationChannel()
        engine_config = self.config["unity_engine_settings"]
        #print("Engine ",engine_config)

        self.engine_channel.set_configuration_parameters(**engine_config)

    def set_env_channel(self):
        self.parameter_channel = EnvironmentParametersChannel()
        parameter_config = self.config["env_settings"]
        for key, value in parameter_config.items():
            self.parameter_channel.set_float_parameter(key,value)
        
    def set_sensor_channel(self):
        self.sensor_channel = SensorSideChannel(self.config)
        self.sensor_channel.set_sensor_parameter()

    def set_action_channel(self):
        self.action_channel = ActionSideChannel(self.config)
        self.action_channel.set_sensor_parameter()

    def set_info_channel(self):
        self.info_channel = InfoSideChannel()

    def set_bug_channel(self):
        self.bug_channel = BugSideChannel(self.config)
        self.bug_channel.set_bug_parameter()

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
        #self.trajectories[self.episode].append(list(observation[-1][0]))
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
            #self.env_size = self.info_channel.message_log.pop(0)
            print("Environment size: ", self.env_size)    
        self.unity_env.reset()
        self.episode += 1
        
        print("New episode!",self.episode)
        self.trajectories[self.episode] = []
        decision_step, _ = self.unity_env.get_steps(self.behavior_name)
        observation = decision_step.obs
        self.bugs_found = 0
        self.timesteps = 0
        
        return observation, info

    def render(self):
        logger.warning("Could not render")
        return

    def close(self):
        self.unity_env.close()

    @property
    def bug_positions(self):
        return self._bug_positions

    @bug_positions.setter
    def bug_positions(self, value):
        self._bug_position = value

    def _get_info(self):
        agent_positions = np.array(self.info_channel.message_log).reshape((len(self.info_channel.message_log),3)) 
        distances = np.linalg.norm(self._bug_positions[:, np.newaxis, :] - agent_positions, axis=2)
        within_distance_mask = distances <= 5;
        bug_key = "BugLog" 
        self.bugs_found = np.sum(within_distance_mask.any(axis=1))        
        self.bugs_found_cumulative += self.bugs_found
        info = { 
            "bugs_found": f"{(100 * self.bugs_found / len(self.bug_data[bug_key])):.2f}%",
            "bugs_found_cumulative": f"{(100 * self.bugs_found_cumulative / len(self.bug_data[bug_key])):.2f}%",
            "area_covered": 0,
            "area_covered_cumulative": 0,
        }
        self._bug_positions = self._bug_positions[np.invert(within_distance_mask.any(axis=1)),...]
        return info

    def set_seed(self):
        pass
    
    def set_config(self):
        with open(self.config_file, "r") as f:
            self.config = yaml.safe_load(f)
            #print(self.config)

    def action_sample(self):
        if self.unity_env.behavior_specs[self.behavior_name].action_spec.is_continuous():
            action_sample = np.random.rand(self.unity_env.behavior_specs[self.behavior_name].action_spec.continuous_size)
            return 2 * action_sample - 1

    def import_bugdata(self):
        with open(r"C:\Users\William\Projects\osb3d\OSB3D\Assets\Data\data.json", "r") as json_file:
            bug_data = json.load(json_file)
        return bug_data
    
    @property
    def spawn_point(self):
        return self._spawn_point
    

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
        """
        Note: We must implement this method of the SideChannel interface to
        receive messages from Unity
        """
        # We simply read a string from the message and print it.
        print(msg.read_string())

    def set_sensor_parameter(self):
        if "observation_space_settings" not in self.config.keys():
            print("No sensor configuration was defined, using default values!")
        else:
            sensor_config = self.config["observation_space_settings"]
            for key1, value1 in sensor_config.items():
                self.send_typed_message(key1,True)
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

class ActionSideChannel(SideChannel):

    T = TypeVar("T")

    def __init__(self, config) -> None:
        super().__init__(uuid.UUID("4a6982f9-d298-4f7b-b7eb-bb7012603bba"))
        self.config = config

    def on_message_received(self, msg: IncomingMessage) -> None:
        """
        Note: We must implement this method of the SideChannel interface to
        receive messages from Unity
        """
        # We simply read a string from the message and print it.
        print(msg.read_string())

    def set_sensor_parameter(self):
        if "observation_space_settings" not in self.config.keys():
            print("No sensor configuration was defined, using default values!")
        else:
            action_config = self.config["action_space_settings"]
            for key, value in action_config.items():
                self.send_typed_message(key, value)

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

class BugSideChannel(SideChannel):

    T = TypeVar("T")

    def __init__(self, config) -> None:
        super().__init__(uuid.UUID("b1961881-7cec-498d-9f45-1f7d8a299378"))
        self.config = config

    def on_message_received(self, msg: IncomingMessage) -> None:
        """
        Note: We must implement this method of the SideChannel interface to
        receive messages from Unity
        """
        # We simply read a string from the message and print it.
        print(msg.read_string())

    def set_bug_parameter(self):
        if "bug_settings" not in self.config.keys():
            print("No bug configuration was defined, using default values!")
        else:
            bug_config = self.config["bug_settings"]
            for key, value in bug_config.items():
                self.send_typed_message(key, value)

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

class InfoSideChannel(SideChannel):
    
    def __init__(self) -> None:
        super().__init__(uuid.UUID("a0b3abca-2146-4ddb-ac7b-713aebedd67f"))
        self._message_log = []

    @property
    def message_log(self) -> List[float]:
        return self._message_log
    
    @message_log.setter
    def message_log(self, value) -> None:
        self._message_log = value
        
    def on_message_received(self, msg: IncomingMessage) -> None:
        """
        Note: We must implement this method of the SideChannel interface to
        receive messages from Unity
        """
        # We simply read a string from the message and print it.
        
        self._message_log.append(msg.read_float32_list())




