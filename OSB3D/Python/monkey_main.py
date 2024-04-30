import numpy as np
import argparse
from env.osb3d_env import OSB3DEnv
from agent.random_monkey import RandomMonkeyAgent

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("-cfg", "--configuration-file", help=None)
    args = parser.parse_args()
    
    env = OSB3DEnv(game_name=None,
                   worker_id=0,
                   no_graphics=False,
                   seed=1,
                   max_episode_timestep=1000,
                   config_file=args.configuration_file)
    observation = env.reset()
    agent = RandomMonkeyAgent(action_size = 6,
                              observation_size = [1,3],
                              is_continuous = True)
    
    
    for i in range(3000*100):
        action = agent.action
        observation, reward, terminated, _, info = env.step(action)
        position = observation[0][0][-4:-1]
        if i == 0:
            agent.add_position(position)
        elif observation[0][0][-1]:
            agent.update_buffer(position)
            

        if terminated:
            observation, info = env.reset()
            env.spawn_point = agent.spawn_point
            print(info)
        if i % 3000 == 0:
            print(agent.position_buffer)
    env.close()

if __name__ == "__main__":
    main()
        
