import numpy as np
import matplotlib.pyplot as plt
import logging as logs
import json

class OSB3DEval:
    def __init__(self, datapath, schema):
        self.datapath = datapath
        self.data = self.load_json(self.datapath["session"])
        self.bugdata = self.load_json(self.datapath["bug"])
        self.schema = schema
        self.agent_positions = self.parse_agent()
        self.bug_positions = self.parse_bug()

    def parse_agent(self):
        x = []
        y = []
        for value, key in enumerate(self.data):
            for point in self.data[key]:
                x.append(point[0])
                y.append(point[2])


        return np.array(list(zip(x,y)))

    def plot_reward(self):
        pass

    def load_json(self,filepath):
        with open(filepath, "r") as f:
            data = json.load(f)
            return data

    def parse_bug(self):
        x = []
        y = []
        for bug in self.bugdata["BugLog"]:
            position = bug["position"]
            x.append(position["x"])
            y.append(position["z"])
        return np.array(list(zip(x,y)))

    def check_bug(self):
        print("agent: ",self.agent_positions.shape)
        print("bug: ",self.bug_positions.shape)
        bug_eval = 0
        for bug_position in self.bug_positions:
            bug_eval += (np.any(np.linalg.norm(bug_position - self.agent_positions,axis=1) < 1))
        print(bug_eval/100)
