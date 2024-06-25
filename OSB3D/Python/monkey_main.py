import numpy as np
import argparse
from env.osb3d_env import OSB3DEnv
from agent.random_monkey import RandomMonkeyAgent
import matplotlib.pyplot as plt
import os
import json

def main():
    parser = argparse.ArgumentParser()
    work_id = 1
    frequency=3000
    parser.add_argument("-cfg", "--configuration-file", help=None)
    args = parser.parse_args()

    env = OSB3DEnv(game_name=os.path.join(os.getcwd(), "../Build/osb3d_random_monkey"),
                   worker_id=2,
                   no_graphics=True,
                   seed=1,
                   max_episode_timestep=2000,
                   config_file=args.configuration_file)
    
    observation = env.reset()
    
    agent = RandomMonkeyAgent(action_size=6,
                              observation_size=[1, 3],
                              is_continuous=True)
    bug_cumulative = []
    for i in range(int(2000)):
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

    with open(os.path.join(os.getcwd(), "../Assets/Data/bug_eval.json"), "w") as f:
        json.dump(bug_cumulative, f)
            
    env.close()

    agent.save_trajectory()

if __name__ == "__main__":
    main()
        
