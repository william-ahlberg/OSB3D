import numpy as np
from env.osb3d_env import OSB3DEnv
def main():


    env = OSB3DEnv(game_name=r"C:\Users\William\Projects\osb3d\OSB3D\Build\OSB3D",
                   worker_id=1,
                   no_graphics=True,
                   seed=1337,
                   config_file=r"C:\Users\William\Projects\osb3d\OSB3D\Python\config\basic.yaml")
    observation = env.reset()
    for _ in range(100000):
        action = env.action_sample()
        observation, reward, terminated, _, info = env.step(action)

        if terminated:
            observation, info = env.reset()
    env.close()

if __name__=="__main__":
    main()
