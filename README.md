# OSB3D

**O**pen-**S**ource **B**enchmark for Automated Testing and Validation
of Complex **3D** Game Environments (OSB3D) is an open source game-testing environment to test and validate Automated Testing Agents (ATA).

***

## Description
Let people know what your project can do specifically. Provide context and add a link to any reference visitors might be unfamiliar with. A list of Features or a Background subsection can also be added here. If there are alternatives to your project, this is a good place to list differentiating factors.

## Badges
On some READMEs, you may see small images that convey metadata, such as whether or not all the tests are passing for the project. You can use Shields to add some to your README. Many services also have instructions for adding a badge.

## Visuals
Depending on what you are making, it can be a good idea to include screenshots or even a video (you'll frequently see GIFs rather than actual videos). Tools like ttygif can help, but check out Asciinema for a more sophisticated method.

## Installation

Within a particular ecosystem, there may be a common way of installing things, such as using Yarn, NuGet, or Homebrew. However, consider the possibility that whoever is reading your README is a novice and would like more guidance. Listing specific steps helps remove ambiguity and gets people to using your project as quickly as possible. If it only runs in a specific context like a particular programming language version or operating system or has dependencies that have to be installed manually, also add a Requirements subsection.

OSB3D can be used after installing the both the required Unity and Python packages.
- For Python run 
    ```
    pip install -r requirements.txt
    ```
  for the required Python libraries. OSB3D was developed and verified for Python 3.8.13.
- For Unity install the `com.unity.ml-agents`, `com.unity.barracuda` and `com.unity.probuilder`. OSB3D was developed and verified for Unity 2021.3.24f1.
* Barracuda 3.0.0
* Ml Agents 2.01
* ProBuilder 5.0.7

## Usage
Once all the packages are installed, locate the 

Linux: https://docs.unity3d.com/2022.1/Documentation/Manual/Buildsettings-linux.html

Windows: https://docs.unity3d.com/2022.1/Documentation/Manual/WindowsStandaloneBinaries.html

Use examples liberally, and show the expected output if you can. It's helpful to have inline the smallest example of usage that you can demonstrate, while providing links to more sophisticated examples if they are too long to reasonably include in the README.

## Configuration
The OSB3D environment is configurable through a yaml file. In the file it is possible to define the agent's observations and action space, and parameters controlling unity and the procedural generation of the environment. To change configuration parameters, just include the top level setting and the sub level parameters and their value. For example if the user want's to use a camera sensor for the agent observation space, just include `camera_settings` and a parameter such as `grayscale: true`. Those settings and parameters not explicitly defined in the config file will revert to default values.

#### Unity engine settings
```yaml
engine_settings:
    width: 1024
    height: 1024
    quality_level: 1
    time_scale: 1
    target_frame_rate: -1
    capture_frame_rate: 60
```
#### Environment generation settings
```yaml
env_settings:
    seed: 1337
    block_count_x: 2
    block_count_z: 2
    park_ratio: 0.25
```
#### Action space settings
```yaml
action_space_settings:
    continuous_actions: true
    available_actions:
        - move_forward
        - move_backwards
        - move_right
        - move_left
        - mouse_y
        - mouse_x
        - jump
        - identify_bug
```
#### Observation space settings
```yaml
observation_space_settings:
    # Vector observations
    vector_obs_settings:
        position_type: "normalized"
        bug_position: false

    # Camera sensor
    camera_settings:
        grayscale: false
        camera_resolution_width: 256
        camera_resolution_height: 256


    # Lidar sensors
    ray_perception_settings:
        ray_perception_plane: 10
        ray_perception_cone : 10
        ray_perception_slice: 10

    # Semantic map sensor
    semantic_map_settings:
        semantic_map_x: 5
        semantic_map_y: 5
        semantic_map_z: 5
```
#### Bug generation settings
```yaml
bug_settings:
    bug_types:
        gadget: 10
        state: 10
        geometry: 10
        physics: 10
        logic: 10
```

#### Unity engine options
| **Setting**                   | **Description**                                                                                                                                                                |
|:------------------------------|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `width`                       | (int, default = 1024): resolution width of the environment session.                                                                                                            |
| `height`                      | (int, default = 1024): resolution width of the environment session.                                                                                                            |
| `quality_level`               | (int, default = 1): quality level of the environment simulation.                                                                                                               |
| `time_scale`                  | (int, default = 1): Changes the environment simulation time to allow for faster training, but can introduce unwanted physics behaviour. Use with caution. Typical range: 1-100 |
| `target_frame_rate`           | (int, default = -1): Specifies the frame rate at which Unity tries to render your game.                                                                                        |
| `capture_frame_rate`          | I don't know yet.                                                                                                                                                              |


| **Setting**                         | **Description**                                                                                                                                                                                                                                                              |
|:------------------------------------|:-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `seed`                              | (int, default = 1337): random generation seed.                                                                                                                                                                                                                               |
| `block_count_x`                     | (int, default = 2): number of city blocks generated in the x direction in the city environment.                                                                                                                                                                              |
| `block_count_z`                     | (int, default = 2): number of city blocks generated in the z direction in the city environment.                                                                                                                                                                              |
| `park_ratio`                        | (int, default = 0.25): the ratio between blocks with parks and blocks with buildings.                                                                                                                                                                                        |

#### Vector observations

| **Settings**            | **Description**                                                                                                                                     |
|:------------------------|:----------------------------------------------------------------------------------------------------------------------------------------------------|
| `position_type`         | (str, default="normalized"): if to include the agent's xyz position in its observation space. The cooridnates can either be "absolute", "normalized"|
| `bug_position `         | (bool, default=false): if to include the bug positions in the observation space.                                                                    |

#### Camera sensor
| **Settings**                   | **Description**                                                                                                                                                                                                  |
|:-------------------------------|:-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `camera_sensor`                | (bool, default=false): if to include a camera sensor in the observation space.                                                                                                                                   |
| `grayscale`                    | (bool, default=true): whether to use rgb or grayscale image data.                                                                                                                                                |
| `camera_resolution_width`      | (int, default=512): resolution width of the image observation.                                                                                                                                                   |
| `camera_resolution_height`     | (int, default=512): resolution height of image observation.                                                                                                                                                      |

#### Ray perception
| **Settings**           | **Description**                                                                                                                    |
|:-----------------------|:-----------------------------------------------------------------------------------------------------------------------------------|
| `ray_perception`       | (bool, default=false): if to include a ray perception sensor in the observation space.                                             |
| `number_of_rays_plane` | (int, default=10): number of ray perception rays directed parallell to the ground to be used in the agent's observation space.     |
| `number_of_rays_cone`  | (int, default=10): number of ray perception rays directed as a above cone to be used in the agent's observation space.             |
| `number_of_rays_slice` | (int, default=10): number of ray perception rays directed perpendicular to the ground to be used in the agent's observation space. |

#### Semantic map sensor
| **Settings**     | **Description**                                                                      |
|:-----------------|:-------------------------------------------------------------------------------------|
| `semantic_map`   | (bool, default=false): if to include a semantic map sensor in the observation space. |
| `semantic_map_x` | (int, default=5): number of cubes of the semantic map in the x direction.            |
| `semantic_map_y` | (int, default=5): number of cubes of the semantic map in the y direction.            |
| `semantic_map_z` | (int, default=5): number of cubes of the semantic map in the z direction.            |

#### Actions allowed
| **Settings**        | **Description                                      |
|:--------------------|:---------------------------------------------------|
| `available_actions` |                                                    |
| `move_forward`      | Walking forwards (W)                               |
| `move_backwards`    | Walking backwards (S)                              |
| `move_right`        | Walking right (D)                                  |
| `move_left`         | Walking left (A)                                   |
| `mouse_y`           | Turning in the plane (side-to-side mouse movement) |
| `mouse_x `          | Turning head up and down (up down mouse movement)  |
| `jump`              | Jumping (SPACE)                                    |
| `identify_bug`      | Agent identifies bug (E)                           |

#### Bugs
| **Settings** | **Description**                                                              |
|:-------------|:-----------------------------------------------------------------------------|
| `bug_types`  | if to include bugs in the environment                                        |
| `gadget`     | (int, default=5): number of bugs relating to interactions with game objects. |
| `state`      | (int, default=5): number of bugs relating to mismatch of game states.        |
| `geometry`   | (int, default=5): number of bugs relating to the environment mesh.           |
| `physics`    | (int, default=5): number of bugs relating to the physics engine.             |
| `logic`      | (int, default=5): number of bugs relating to game logic.                     |


## Support
Tell people where they can go to for help. It can be any combination of an issue tracker, a chat room, an email address, etc.

## Roadmap
If you have ideas for releases in the future, it is a good idea to list them in the README.

## Contributing
State if you are open to contributions and what your requirements are for accepting them.

For people who want to make changes to your project, it's helpful to have some documentation on how to get started. Perhaps there is a script that they should run or some environment variables that they need to set. Make these steps explicit. These instructions could also be useful to your future self.

You can also document commands to lint the code or run tests. These steps help to ensure high code quality and reduce the likelihood that the changes inadvertently break something. Having instructions for running tests is especially helpful if it requires external setup, such as starting a Selenium server for testing in a browser.

## Authors and acknowledgment
Show your appreciation to those who have contributed to the project.

## License
For open source projects, say how it is licensed.

## Project status
If you have run out of energy or time for your project, put a note at the top of the README saying that development has slowed down or stopped completely. Someone may choose to fork your project or volunteer to step in as a maintainer or owner, allowing your project to keep going. You can also make an explicit request for maintainers.
