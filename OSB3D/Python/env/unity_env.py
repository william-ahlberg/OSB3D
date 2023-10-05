from mlagents_envs.environment import UnityEnvironment


class OSB3DEnv:
    def __init__(self, file_name, seed, no_graphics, max_timestep):
        self.file_name = file_name
        self.seed = seed
        self.no_graphics = no_graphics
        self.max_timestep = max_timestep
        self.unity_env = UnityEnvironment(self.file_name, self.seed)
        self.unity_env.reset()

    def execute(self):
        pass

    def reset(self):
        pass

    def close(self):
       self.unity_env.close()
