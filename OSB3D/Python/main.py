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
                   worker_id=1,
                   no_graphics=False,
                   seed=1337,
                   config_file=args.configuration_file)
    observation = env.reset()
    for _ in range(100000):
        action = env.action_sample()
        action = np.zeros(action.shape)
        observation, reward, terminated, _, info = env.step(action)
        
        if terminated:
            observation, info = env.reset()
    env.close()

if __name__=="__main__":
    main()
