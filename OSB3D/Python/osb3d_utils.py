from sys import platform
import os
class OSB3DUtils:

    def __init__(self):
        pass
    def persistent_datapath(self):
        if platform == "win32":
            return rf"C:\Users\{os.getlogin()}\AppData\LocalLow\DefaultCompany\OSB3D"
        elif platform == "linux" or platform == "linux2":
            return os.path.expanduser("~/.config/unity3d/DefaultCompany/OSB3D")
        elif platform == "darwin":
            return r"$HOME/Library/Application Support/DefaultCompany/OSB3D"
        else:
            raise OSError("Unsupported platform")