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
class OSB3DEnv(gym.Env):
    def __init__(self, game_name, worker_id, no_graphics, seed, config_file):
        self.game_name = game_name
        self.seed = seed
        self.no_graphics = no_graphics
        self._max_episode_timesteps = 1000
        self.config_file = config_file
        self.config = {}

        self.set_config()
        self.engine_channel = None
        self.set_engine_channel()

        self.side_channels = [self.engine_channel]
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
        self.action_size = 5
        self.trajectory = []
        self.trajectories = dict()

    def set_engine_channel(self):
        self.engine_channel = EngineConfigurationChannel()
        engine_config = self.config["unity_engine_config"]
        self.engine_channel.set_configuration_parameters(**engine_config)

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
        print(terminal_steps)
        if (terminal_steps.interrupted == True):
            print("done")
            terminated = True
            reward = terminal_steps.reward[0]
        else:
            print("not done")
            reward = decision_steps.reward[0]
            terminated = False
        observation = decision_steps.obs
        self.trajectories[self.episode].append(list(observation[-1][0]))

        return observation, reward, terminated, False, None

    def reset(self):
        super().reset(seed=self.seed)
        if (self.episode != -1):
            info = self.__get_info()
        else:
            info = None
        self.unity_env.reset()
        self.episode += 1
        print("New episode!")
        self.trajectories[self.episode] = []
        decision_step, _ = self.unity_env.get_steps(self.behavior_name)
        observation = decision_step.obs

        return observation, info

    def render(self):
        logger.warning("Could not render")
        return

    def close(self):
        self.unity_env.close()

    def __get_info(self):
        print(self.trajectory)
        self.eval = OSB3DEval({"bug": "C:/Users/William/Projects/osb3d/OSB3D/Assets/Data/data.json"}, self.trajectory)
        print(self.eval.check_bug())
        info = {
            "bugs_found": 0,
            "bugs_found_cumulative": 0,
            "area_covered": 0,
            "area_covered_cumulative": 0}

        return info

    def set_seed(self):
        pass
    def set_config(self):
        with open(self.config_file, "r") as f:
            self.config = yaml.safe_load(f)

    def action_sample(self):
        if self.unity_env.behavior_specs[self.behavior_name].action_spec.is_continuous():
            action_sample = np.random.rand(self.unity_env.behavior_specs[self.behavior_name].action_spec.continuous_size)
            return action_sample






    """
    EngineConfiguration (width,height,quality_level,time_scale,target_frame_rate,capture_frame_rate)
    EnvironmentParametersChannel (key,value)

    Environment
    seed
    Block Count X
    Block Count Z
    Park Ratio
    Car Min
    Car Max
    Item Min
    Item Max

    Bug

    Agent
    Camera Resolution
    Rayperception config
    CheatingASensor
    VectorObservations



    """