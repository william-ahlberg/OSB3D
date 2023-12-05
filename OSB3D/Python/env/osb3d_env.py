from mlagents_envs.environment import UnityEnvironment
import numpy as np
import matplotlib.pyplot as plt
import logging as logs
from mlagents_envs.base_env import ActionTuple


class OSB3DEnv(UnityEnvironment):
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

        self.behavior_name = behavior_name
        self.game_name = game_name
        self.seed = seed
        self.side_channels = []

    def set_seed(self):


class OSB3DConfigurationChannel(SideChannel):
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