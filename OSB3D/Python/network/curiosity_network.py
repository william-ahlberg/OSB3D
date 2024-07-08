"""
Deep neural network for a cuirosity-driven agent using pytorch.
"""

import torch
import torch.nn as nn
import torch.nn.functional as F
import numpy as np
import uuid

def layer_init(layer, std=np.sqrt(2), bias_const=0.0):
    torch.nn.init.orthogonal_(layer.weight, std)
    torch.nn.init.constant_(layer.bias, bias_const)
    return layer

class AgentModel(nn.Module):

    def __init__(self):
        super().__init__()
        self.model = nn.Sequential(
            layer_init(nn.Linear(64, 64)),
            nn.ReLU(),
            layer_init(nn.Linear(64, 64)),
            layer_init(nn.Linear(64, 6), std=1.0),
            nn.Softmax(dim=1),
        )

    def forward(self, x):
        return self.model(x)