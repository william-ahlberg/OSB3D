class ChannelManager:
    def __init__(self):
        self.channels = {}

    def add_channel(self, name, channel):
        self.channels[name] = channel

    def remove_channel(self, name):
        if name in self.channels:
            del self.channels[name]

    def get_channel(self, name):
        return self.channels.get(name, None)

    def __str__(self):
        return f"ChannelManager with channels: {list(self.channels.keys())}"