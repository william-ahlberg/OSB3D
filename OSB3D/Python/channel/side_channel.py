from mlagents_envs.side_channel.environment_parameters_channel import EnvironmentParametersChannel
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel
from mlagents_envs.side_channel.side_channel import (
    SideChannel,
    IncomingMessage,
    OutgoingMessage,
)
from typing import TypeVar, Any, List, Iterable, Dict
import uuid

class TypedMessageSender:

    @staticmethod
    def _write_value(msg: OutgoingMessage, key, value) -> None:
        if isinstance(value, str):
            msg.write_string(value)

        elif isinstance(value, bool):
            msg.write_bool(value)

        elif isinstance(value, int):
            msg.write_int32(value)

        elif isinstance(value, float):
            msg.write_float32(value)

        elif isinstance(value, Iterable):
            items = list(value)
            msg.write_int32(len(items))
            if not items:
                return
            if all(isinstance(x, float) for x in items):
                msg.write_float32_list(items)
            elif all(isinstance(x, int) for x in items):
                for v in items:
                    msg.write_int32(int(v))
            elif all(isinstance(x, str) for x in items):
                for v in items:
                    msg.write_string(v)
            else:
                for v in items:
                    msg.write_string(str(v))
        else:
            msg.write_string(str(value))

    def send(self, channel: SideChannel, key: str, value: Any) -> None:
        msg = OutgoingMessage()
        msg.write_string(key)
        self._write_value(msg, key, value)
        channel.queue_message_to_send(msg)

    def __call__(self, channel: SideChannel, key: str, value: Any) -> None:
        self.send(channel, key, value)


class SensorSideChannel(SideChannel):
    T = TypeVar("T")

    def __init__(self, config) -> None:
        super().__init__(uuid.UUID("aa97d987-4c42-4878-b597-3de40edf66a6"))
        self.config = config
        self._sender = TypedMessageSender()

    def on_message_received(self, msg: IncomingMessage) -> None:
        try:
            print(msg.read_string())
        except Exception:
            pass

    def set_sensor_parameter(self):
        sensor_config = self.config.get("observation_space_settings", {})

        if not sensor_config:
            print("No sensor configuration was defined, using default values!")
            return

        for key1, value1 in sensor_config.items():
            self._sender(self,key1, True)
            for key2, value2 in value1.items():
                self._sender(self,key2, value2)

class ActionSideChannel(SideChannel):
    T = TypeVar("T")

    def __init__(self, config) -> None:
        super().__init__(uuid.UUID("4a6982f9-d298-4f7b-b7eb-bb7012603bba"))
        self.config = config
        self._sender = TypedMessageSender()

    def on_message_received(self, msg: IncomingMessage) -> None:
        try:
            print(msg.read_string())
        except Exception:
            pass

    def set_sensor_parameter(self):
        if "observation_space_settings" not in self.config.keys():
            print("No sensor configuration was defined, using default values!")
        else:
            action_config = self.config["action_space_settings"]
            for key, value in action_config.items():
                self._sender(self,key, value)


class BugSideChannel(SideChannel):
    T = TypeVar("T")

    def __init__(self, config) -> None:
        super().__init__(uuid.UUID("b1961881-7cec-498d-9f45-1f7d8a299378"))
        self.config = config
        self._sender = TypedMessageSender()

    def on_message_received(self, msg: IncomingMessage) -> None:
        try:
            print(msg.read_string())
        except Exception:
            pass

    def set_bug_parameter(self):
        if "bug_settings" not in self.config.keys():
            print("No bug configuration was defined, using default values!")
        else:
            bug_config = self.config["bug_settings"]
            for key, value in bug_config.items():
                self._sender(self,key, value)


class InfoSideChannel(SideChannel):

    def __init__(self) -> None:
        super().__init__(uuid.UUID("a0b3abca-2146-4ddb-ac7b-713aebedd67f"))
        self._message_log = []
        self._sender = TypedMessageSender()

    @property
    def message_log(self) -> List[float]:
        return self._message_log

    @message_log.setter
    def message_log(self, value) -> None:
        self._message_log = value

    def on_message_received(self, msg: IncomingMessage) -> None:
        self._message_log.append(msg.read_float32_list())
