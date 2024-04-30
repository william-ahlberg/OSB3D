import numpy as np
import argparse
from env.osb3d_env import OSB3DEnv
from agent.random_monkey import RandomMonkeyAgent
import matplotlib.pyplot as plt
from runner.runner import Runner
from runner.parallel_runner import Runner as ParallelRunner

def main():
    parser = argparse.ArgumentParser()
    work_id = 256
    frequency=3000
    parser.add_argument("-cfg", "--configuration-file", help=None)
    args = parser.parse_args()
    """ 
    env = OSB3DEnv(game_name=None,
                   worker_id=0,
                   no_graphics=False,
                   seed=1,
                   max_episode_timestep=2000,
                   config_file=args.configuration_file)
    
    observation = env.reset()
    
    agent = RandomMonkeyAgent(action_size = 6,
                              observation_size = [1,3],
                              is_continuous = True)
    
    for i in range(2000*2):
        action = agent.action
        observation, reward, terminated, _, info = env.step(action)
        position = observation[0][0][-4:-1]
        
        agent.trajectory.append([action,position])
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
            
    env.close()

    agent.save_trajectory()
    """
    #Create agent
    parallel = False
    agent = RandomMonkeyAgent(action_size = 6,
                              observation_size = [1,3],
                              is_continuous = True)
    if not parallel:
        env = OSB3DEnv(game_name=None,
                       worker_id=0,
                       no_graphics=False,
                       max_episode_timestep=2000,
                       config_file=args.configuration_file,
                       seed=0,)
    else:
        envs = []
        for i in range(3):
            envs.append(OSB3DEnv(game_name=None,
                       worker_id=work_id + i,
                       no_graphics=False,
                       max_episode_timestep=2000,
                       config_file=args.configuration_file,))

    if not parallel:
        runner = Runner(agent=agent, frequency=frequency, env=env)
    else:
        runner = ParallelRunner(agent=agent, frequency=frequency, envs=envs)

    runner.run()


if __name__ == "__main__":
    main()
        
