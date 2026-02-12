import typing as _typing
import math
import torch
import torch.nn as nn
import torch.optim as optim
import torch.nn.functional as F


class MLP(nn.Module):
    def __init__(self, input_dim: int, hidden_dims: _typing.Sequence[int], output_dim: int):
        super().__init__()
        dims = [input_dim, *hidden_dims, output_dim]
        layers = []
        for i in range(len(dims) - 2):
            layers.append(nn.Linear(dims[i], dims[i+1]))
            layers.append(nn.Tanh())
        layers.append(nn.Linear(dims[-2], dims[-1]))
        self.net = nn.Sequential(*layers)

    def forward(self, x: torch.Tensor) -> torch.Tensor:
        return self.net(x)


class PPO:
    """Simple PPO (clip) actor-critic for continuous action spaces.

    Expected use:
      algo = PPO(obs_dim, action_dim)
      agent = RLAgent(algo)

    The `update` method expects a batch dict with keys (numpy arrays or tensors):
      - obs: shape (N, obs_dim)
      - actions: shape (N, action_dim)
      - returns: shape (N,)
      - advantages: shape (N,)
      - old_logp: shape (N,)

    This is a compact implementation for experimentation and easily replaced
    by a more feature-complete implementation later.
    """

    def __init__(self,
                 obs_dim: int,
                 action_dim: int,
                 hidden_sizes=(64, 64),
                 lr: float = 3e-4,
                 clip_eps: float = 0.2,
                 value_coef: float = 0.5,
                 entropy_coef: float = 0.01,
                 max_grad_norm: float = 0.5,
                 device: _typing.Union[str, torch.device] = "cpu"):


        self.device = torch.device(device)
        self.obs_dim = obs_dim
        self.action_dim = action_dim
        self.clip_eps = clip_eps
        self.value_coef = value_coef
        self.entropy_coef = entropy_coef
        self.max_grad_norm = max_grad_norm

        # actor: outputs mean of Gaussian policy
        self.actor = MLP(obs_dim, hidden_sizes, action_dim).to(self.device)
        # learnable log std per action dim
        self.actor_logstd = nn.Parameter(torch.zeros(action_dim, device=self.device))

        # critic: outputs scalar value
        self.critic = MLP(obs_dim, hidden_sizes, 1).to(self.device)

        self.optimizer = optim.Adam(self._parameters(), lr=lr)

    def policy(self, observations):
        action_mean = self.actor(observations)
        action_logstd = self.actor_logstd.expand_as(action_mean)
        action_std = torch.exp(action_logstd)
        normal_distribution = torch.distributions.Normal(action_mean, action_std)
        action = normal_distribution.sample()
        return action


    def value(self, observations):
        return self.critic(observations).squeeze(-1)


    def train(self):

        self.actor.train()
    


