import numpy as np
import argparse
from env.osb3d_env import OSB3DEnv
import os
import yaml
from agent.agent import Agent
from algorithm.ppo import PPO

RUNNING_BUILD = False

def read_config(file_path):
    config = {}
    if not os.path.isfile(file_path):
        raise FileNotFoundError(f"Configuration file '{file_path}' not found.")
    with open(file_path, 'r') as file:
        try:
            config = yaml.safe_load(file)
        except yaml.YAMLError as e:
            raise ValueError(f"Error parsing YAML file: {e}")
    return config

def make_env(args, config):

    game_name = config.get("game_name", None)
    headless = config.get("headless", False)
    no_graphics = config.get("no_graphics", False)

    env = OSB3DEnv(game_name=game_name,
                   worker_id=0 if not headless else 1,
                   no_graphics=no_graphics,
                   seed=1,
                   max_episode_timestep=2000,
                   config=config)

    return env

def make_algorithm(algorithm_name, obs_space, act_space, config):
    if algorithm_name == "PPO":
        return PPO(obs_space, act_space, config)
    else:
        raise ValueError(f"Unsupported algorithm: {algorithm_name}")

def main():

    parser = argparse.ArgumentParser()
    parser.add_argument("-cfg", "--configuration-file", help=None)
    parser.add_argument("-gn", "--game-name", help=None, default=None)
    parser.add_argument("-vrb", "--verbose", help=None, default=False)
    args = parser.parse_args()

    config = read_config(args.configuration_file)
    env = make_env(args, config)
    env.reset()
    algorithm = make_algorithm("PPO", env.observation_space, env.action_space, config)
    agent = Agent(algorithm, env.observation_space, env.action_space, config)




    for _ in range(10000):
        action = env.action_sample()

        observation, reward, terminated, _, info = env.step(action)
        if terminated:
            observation, info = env.reset()
            print(info)
    env.close()

if __name__=="__main__":
    main()
