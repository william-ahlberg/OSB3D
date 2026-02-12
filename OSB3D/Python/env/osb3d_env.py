import yaml
import gymnasium as gym
import numpy as np
from mlagents_envs.base_env import ActionTuple
from mlagents_envs.environment import UnityEnvironment
from typing import TypeVar
from typing import List
import json

from channel.channel_manager import ChannelManager

from channel.side_channel import EngineConfigurationChannel, EnvironmentParametersChannel, SensorSideChannel, ActionSideChannel, InfoSideChannel, BugSideChannel

from gymnasium.spaces import Dict, Box, Discrete, MultiDiscrete

from osb3d_utils import OSB3DUtils


class UnityToGymAdapter(gym.Env):
    def __init__(self, env: UnityEnvironment):
        self.env = env
        self.env.reset()
        self.behavior_name = list(env.behavior_specs._dict.keys())[0]
        self.spec = env.behavior_specs[self.behavior_name]
        self.step_count = 0
        self.length = 512  # Default episode length, can be modified as needed


    @property
    def unwrapped(self):
        if hasattr(self.env, 'unwrapped'):
            return self.env.unwrapped
        return self.env


    def step(self, action):
        action_tuple = ActionTuple()
        if self.spec.action_spec.is_continuous():
            action_tuple.add_continuous(action.reshape(1, -1))
        if self.spec.action_spec.is_discrete():
            action_tuple.add_discrete(action)

        self.env.set_actions(self.behavior_name, action_tuple)
        self.env.step()
        self.step_count += 1
        (decision_steps, terminal_steps) = self.env.get_steps(self.behavior_name)

        # Decide which observation and reward to return. Prefer terminal_steps if present.
        if len(terminal_steps) > 0:
            observation = terminal_steps.obs
            reward = float(terminal_steps.reward[0])
            terminated = True
            truncated = False
            info = {}
            return observation, reward, terminated, truncated, info

        # If step limit reached, treat as truncation
        if self.step_count >= self.length:
            print(self.step_count)
            observation = decision_steps.obs
            reward = float(decision_steps.reward[0])
            terminated = False
            truncated = True
            info = {}
            return observation, reward, terminated, truncated, info

        # Normal decision step
        observation = decision_steps.obs
        reward = float(decision_steps.reward[0])
        terminated = False
        truncated = False
        info = {}
        return observation, reward, terminated, truncated, info

    def reset(self, seed=0, options=None):
        self.env.reset()
        self.step_count = 0
        decision_step, _ = self.env.get_steps(self.behavior_name)
        observation = decision_step.obs
        return observation, {}

    def _is_terminal(self, interrupted):
        if interrupted is None:
            return False
        try:
            return bool(interrupted[0])
        except Exception:
            return bool(interrupted)



class OSB3DEnv(gym.Env):
    def __init__(self, game_name, worker_id, no_graphics, seed, max_episode_timestep, config):
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
        self.total_timesteps = 0
        self.config = config
        self.worker_id = worker_id
        self._channel_manager = ChannelManager()





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

        self._env = UnityToGymAdapter(UnityEnvironment(self.game_name,
                                                       worker_id=self.worker_id,
                                                       seed=self.seed,
                                                       no_graphics=self.no_graphics,
                                                       side_channels=self.side_channels,
                                                       ))
        self._env.reset()

        self.behavior_name = "AgentBehavior?team=0"


        self.actions_for_episode = dict()
        self.episode = -1
        self.trajectories_for_episode = dict()
        self.reward_weights = None
        self.coverage_of_points = []
        self.pos_buffer = dict()
        self.action_size = 6 
        self.trajectory = []
        self.trajectories = dict()
        self.bug_data = self.import_bugdata() 
        self.bugs_found = 0
        self.bugs_found_cumulative = 0
        self.env_size = 0 
        self.info_log = {}
        
        self._bug_positions = np.zeros((len(self.bug_data["BugLog"]),3))
        
        for index, bug in enumerate(self.bug_data["BugLog"]):
            x = bug["position"]["x"]
            y = bug["position"]["y"]
            z = bug["position"]["z"]
            self._bug_positions[index,:] = (x,y,z)

        self._spawn_point = [0,0,0]

        #self.observation_space = gym.spaces.Box(low=-np.inf, high=np.inf, shape=(1, 8), dtype=np.float32)
        #self.action_space = gym.spaces.Box(low=-1, high=1, shape=(self.action_size,), dtype=np.float32)


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
        observation, reward, terminated, truncated, info = self._env.step(action)
        info = self._get_info(info)
        return observation, reward, terminated, truncated, info

    def _observation(self, observation):

        observation_order = self.observation_space
        print("Observation order: ", observation_order)
        for sensor in list(self.observation_space.keys()):
           print("Sensor: ", sensor)
        return {}

    def reset(self, seed=0, options=None):
        observation, info = self._env.reset()
        return observation, info

    def render(self):
        raise NotImplementedError


    def close(self):
        self._env.close()

    @property
    def bug_positions(self):
        return self._bug_positions

    @bug_positions.setter
    def bug_positions(self, value):
        self._bug_position = value

    def _get_info(self,info):
        agent_positions = np.array(self.info_channel.message_log).reshape((len(self.info_channel.message_log),3)) 
        distances = np.linalg.norm(self._bug_positions[:, np.newaxis, :] - agent_positions, axis=2)
        within_distance_mask = distances <= 2
        bug_key = "BugLog" 
        self.bugs_found = np.sum(within_distance_mask.any(axis=1))        
        self.bugs_found_cumulative += self.bugs_found
        self.total_timesteps += self.timesteps
        info = { 
            "bugs_found": f"{self.bugs_found}({(100 * self.bugs_found / len(self.bug_data[bug_key])):.2f}%)",
            "bugs_found_cumulative": f"{self.bugs_found_cumulative}({(100 * self.bugs_found_cumulative / len(self.bug_data[bug_key])):.2f}%)",
            "area_covered": 0,
            "area_covered_cumulative": 0,
            "steps": self.timesteps,
            "total_steps": self.total_timesteps,
        }
        self._bug_positions = self._bug_positions[np.invert(within_distance_mask.any(axis=1)),...]
        self.info_log[self.episode] = info
        return info

    def set_seed(self):
        raise NotImplementedError

    def action_sample(self):
        if self._env.spec.action_spec.is_continuous():
            action_sample = np.random.rand(self._env.spec.action_spec.continuous_size)
            return 2 * action_sample - 1

    def import_bugdata(self): #TODO: Change to relative path
        osb3d_utils = OSB3DUtils()
        data_path = osb3d_utils.persistent_datapath() + "/data.json"

        with open(data_path, "r") as json_file:
            bug_data = json.load(json_file)
        return bug_data
    
    @property
    def spawn_point(self):
        return self._spawn_point
    
    @spawn_point.setter
    def spawn_point(self, value):
        self._spawn_point = list(value)
        self.info_channel.send_typed_message("spawn_point", self._spawn_point)

    @property
    def observation_space(self):
        observation_space = Dict()
        for sensor in self._env.spec.observation_specs:
            if sensor.name == "VectorSensor_size8":
                observation_space[sensor.name] = Box(low=-np.inf, high=np.inf, shape=sensor.shape, dtype=np.float32)
            if sensor.name == "SemanticMapSensor":
                observation_space[sensor.name] = MultiDiscrete(sensor.shape,dtype=np.int8)
            if sensor.name == "RaySensor":
                observation_space[sensor.name] = Box(low=-np.inf, high=np.inf, shape=sensor.shape, dtype=np.float32)
            if sensor.name == "CameraSensor":
                observation_space[sensor.name] = Box(low=0, high=255, shape=sensor.shape, dtype=np.uint8)

        return observation_space

    @property
    def action_space(self):
        if self._env.spec.action_spec.is_continuous():
            continuous_action_shape = self._env.spec.action_spec.continuous_size
            action_space = gym.spaces.Box(low=-1, high=1, shape=(continuous_action_shape,), dtype=np.float32)
            return action_space
        elif self._env.spec.action_spec.is_discrete():
            discrete_action_shape = self._env.spec.action_spec.discrete_size
            action_space = gym.spaces.Discrete(discrete_action_shape)
            return action_space
        else:
            raise NotImplementedError


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

class AgentDataLogger():

    def __init__(self):
        agent_action_space = 6
        agent_observation_space = 8
        data_log = {}

    def log_data(self, data, data_key):
        self.data_log[data_key] = data








