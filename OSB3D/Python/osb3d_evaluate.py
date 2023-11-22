import numpy as np
from eval.eval import OSB3DEval
import matplotlib.pyplot as plt



DATAPATH = {"session": "C:\\Users\\sofie\\Projekt\\ccpt\\arrays\\playtest\\playtest_trajectories_1800.json",
            "bug": "C:\\Users\\sofie\\Projekt\\osb3d\\OSB3D\\Assets\\Data\\data.json"}






if __name__ == "__main__":
    evaluator = OSB3DEval(DATAPATH,{})
    #x = []
    #y = []
    #for value, key in enumerate(evaluator.data):
    #    for point in evaluator.data[key]:
    #        x.append(point[0] * 832/227 + 180)
    #        y.append(point[2] * -831/227 + 640)
    #img = plt.imread("birdseye_osb3d.png")
    #fig, ax = plt.subplots()
    #ax.imshow(img)
    #ax.scatter(x,y, alpha=0.01)

    evaluator.check_bug()


