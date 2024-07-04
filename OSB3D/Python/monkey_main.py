import numpy as np
import argparse
from env.osb3d_env import OSB3DEnv
from agent.random_monkey import RandomMonkeyAgent
import matplotlib.pyplot as plt
import os
import json
from osb3d_utils import OSB3DUtils
RUNNING_BUILD = True
def main():
    osb3d_utils = OSB3DUtils()
    parser = argparse.ArgumentParser()
    frequency = 3000
    parser.add_argument("-cfg", "--configuration-file", help=None)
    parser.add_argument("-gn", "--game-name", help=None, default=None)

    args = parser.parse_args()
    print(args.configuration_file)

    if RUNNING_BUILD:
        env = OSB3DEnv(game_name=args.game_name,
                       worker_id=1338,
                       no_graphics=True,
                       seed=1337,
                       max_episode_timestep=2000,
                       config_file=args.configuration_file)
    else:
        env = OSB3DEnv(game_name=None,
                       worker_id=0,
                       no_graphics=False,
                       seed=1,
                       max_episode_timestep=2000,
                       config_file=args.configuration_file)

    observation, _ = env.reset()

    agent = RandomMonkeyAgent(action_size=6,
                              observation_size=[1, 8],
                              is_continuous=True)
    bug_cumulative = []
    print("NUMBER OF BUGS: ", len(env.bug_data["BugLog"]))
    for i in range(int(15000*2000)):
        action = agent.action
        observation, reward, terminated, _, info = env.step(action)
        position = observation[0][0][-4:-1]
        
        agent.trajectory.append(np.concatenate((position,action)))
        if i == 0:
            agent.add_position(position)    
        elif observation[0][0][-1]:
            agent.update_buffer(position)
            
        if terminated:
            agent.trajectories.append(agent.trajectory)
            agent.trajectory = [] 
            env.spawn_point = agent.spawn_point
            observation, info = env.reset()
            print(info)
            bug_cumulative.append(info["bugs_found_cumulative"])

    with open(osb3d_utils.persistent_datapath() + r"/info.json", "w") as f:
        json.dump(env.info_log, f, indent=4)
            
    agent.save_trajectory()
    env.close()


if __name__ == "__main__":
    main()
        
