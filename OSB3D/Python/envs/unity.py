from mlagents_envs.environment import UnityEnvironment


class OSB3DEnv(UnityEnvironment):
    def __init__(self):
        super().__init__()
        self.file_name = file_name
        self.seed = seed
        self.env = UnityEnvironment(self.file_name, self.seed)



