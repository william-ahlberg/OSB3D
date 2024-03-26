import numpy as np
import argparse
from env.osb3d_env import OSB3DEnv
def main():

    parser = argparse.ArgumentParser()
    parser.add_argument("-cfg", "--configuration-file", help=None)
    parser.add_argument("-vrb", "--verbose", help=None, default=False)
    args = parser.parse_args()

    env = OSB3DEnv(
                   game_name=None,
                   worker_id=0,
                   no_graphics=False,
                   seed=1337,
                   max_episode_timestep=1000,
                   config_file=args.configuration_file)
    observation = env.reset()
    for _ in range(10000):
        action = env.action_sample()
        #print(action)
        #action = np.zeros(action.shape)
        observation, reward, terminated, _, info = env.step(action)
       # print(observation)
        if terminated:
            observation, info = env.reset()
            print(info)
    env.close()

if __name__=="__main__":
    main()
