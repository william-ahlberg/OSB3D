from turtle import position
import numpy as np
import random
import os
import json
class RandomMonkeyAgent:
    
    
    def __init__(self,observation_size=0, 
                 action_size=np.zeros((1,1)).shape,
                 is_continuous = True,):
        
        self.action_size = action_size
        self.observation_size = observation_size
        self.is_continuous = is_continuous
        self._spawn_point = (0,0,0)
        self.discretization_threshold = 5
        self.position_buffer = {"positions":[], 
                                "visitation_count": []}
        self._action = np.zeros((self.action_size)) 
        self.trajectory = []
        self.trajectories = []
    
    @property
    def action(self):
        if self.is_continuous:
            action = 2 * np.random.random_sample(size=self.action_size) - 1
            return action
    

    def save_trajectory(self):
        data = {}
        for i, rollout in enumerate(self.trajectories):
            rollout_list = [position.tolist() for position in rollout]
            data = {i: rollout_list}
            #f.write("%s\n" %rollout)
        #data_serialized = json.dump(data)
        with open(os.path.join(os.getcwd(),"../Assets/Data/rollout.json"), "w") as f:
            json.dump(data, f)




    def increment_position_count(self, index):
       self.position_buffer["visitation_count"][index] += 1   
        
    def update_buffer(self, position):
        position_buffer_array = np.array(self.position_buffer["positions"])
        distances = np.linalg.norm(position-position_buffer_array,axis=1)
        threshold_mask = distances >= self.discretization_threshold
        if threshold_mask.all():
            self.add_position(position)
        else:
            closest_position = distances.argmin()
            self.increment_position_count(closest_position)

    def add_position(self, position):
        self.position_buffer["positions"].append(position)
        self.position_buffer["visitation_count"].append(0)
        self.increment_position_count(-1)

    @property
    def spawn_point(self):
        visitation_count = self.position_buffer["visitation_count"] 
        sum_of_inverse = sum(1/n for n in visitation_count)
        position_probability = [count / sum_of_inverse for count in visitation_count]
        position_index = random.choices(population=range(len(visitation_count)), weights=position_probability)
        return self.position_buffer["positions"][position_index[0]]

    @spawn_point.setter
    def spawn_point(self, value) -> None:
        self._spawn_point = value