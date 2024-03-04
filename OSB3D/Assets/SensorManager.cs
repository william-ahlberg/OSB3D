using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;

public class SensorManager : MonoBehaviour
{
    //Channel
    SensorSideChannel sensorSideChannel;
    
    GameObject body;
    GameObject sensors;

    //Camera
    bool cameraSettings;
    Camera agentCamera;
    CameraSensorComponent cameraSensor;
    int cameraSensorWidth = 1920;
    int cameraSensorHeight = 1080;

    //Rays
    bool rayPerceptionSettings;
    RayPerceptionSensorComponent3D raySensor;
    float maxRayDegrees = 180;

    //Semantic Map
    bool semanticMapSettings;
    Semantic3DMapComponent semanticMapSensor;
    EnvironmentParameters envParameters;

    float gridX = 3;
    float gridY = 3;
    float gridZ = 3;

    public void Awake()
    {
        sensorSideChannel = new SensorSideChannel();
        SideChannelManager.RegisterSideChannel(sensorSideChannel);
        rayPerceptionSettings = sensorSideChannel.GetWithDefault<bool>("ray_perception_settings", false);
        semanticMapSettings = sensorSideChannel.GetWithDefault<bool>("semantic_map_settings", false);
        cameraSettings = sensorSideChannel.GetWithDefault<bool>("camera_settings", false);


    }

    // Start is called before the first frame update
    void Start()
    {
        body = GameObject.Find("Body");
        sensors = GameObject.Find("Sensors");

        if (cameraSettings)
        {
            agentCamera = GetComponentInChildren<Camera>();
            cameraSensor = sensors.AddComponent<CameraSensorComponent>();
            cameraSensor.Width = sensorSideChannel.GetWithDefault<int>("camera_resolution_width", cameraSensorWidth);
            cameraSensor.Height = sensorSideChannel.GetWithDefault<int>("camera_resolution_height", cameraSensorHeight);
            cameraSensor.Grayscale = sensorSideChannel.GetWithDefault<bool>("grayscale", false);
            cameraSensor.SensorName = "AgentCameraSensor";
            cameraSensor.Camera = agentCamera;
        }

        if (rayPerceptionSettings)
        {
            raySensor = sensors.AddComponent<RayPerceptionSensorComponent3D>();
            raySensor.SensorName = "RaySensor";
            raySensor.RaysPerDirection = (int)31f;
            raySensor.MaxRayDegrees = envParameters.GetWithDefault("max_ray_degrees", maxRayDegrees);
            raySensor.DetectableTags = new List<string>() { "Building", "Item", "Road", "Car", "Ground" };
        }

        if (semanticMapSettings)
        {
            semanticMapSensor = sensors.AddComponent<Semantic3DMapComponent>();
            semanticMapSensor.root = body;
            semanticMapSensor._gridX = (int)envParameters.GetWithDefault("semantic_grid_x", gridX);
            semanticMapSensor._gridY = (int)envParameters.GetWithDefault("semantic_grid_y", gridY);
            semanticMapSensor._gridZ = (int)envParameters.GetWithDefault("semantic_grid_z", gridZ);
            semanticMapSensor.Tags = new List<string>() { "Building", "Item", "Road", "Car", "Ground" };
        }


    }

    // Update is called once per frame
    void Update()
    {
    }
}
