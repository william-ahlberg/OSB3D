import numpy as np
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
from mlagents_envs.side_channel.environment_parameters_channel import EnvironmentParametersChannel
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel
def main():
    env_channel = EnvironmentParametersChannel()
    eng_channel = EngineConfigurationChannel()
    env_channel.set_float_parameter("seed", 15)
    eng_channel.set_configuration()

    unity_env = UnityEnvironment("games/OSB3D", no_graphics=False, side_channels=[eng_channel,env_channel])
    env = UnityToGymWrapper(unity_env=unity_env, uint8_visual=False)
    observation = env.reset()
    for _ in range(1000):

        action = env.action_space.sample()
        observation, reward, done, info = env.step(action)

        if done:
            observation, info = env.reset()
    env.close()

if __name__=="__main__":
    main()
