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

    int gridX = 5;
    int gridY = 5;
    int gridZ = 5;

    public void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public void AddSensors(SensorSideChannel sensorSideChannel)
    {
        // We need to do this otherwise we can't receive messages to this point
        envParameters = Academy.Instance.EnvironmentParameters;
        rayPerceptionSettings = sensorSideChannel.GetWithDefault<bool>("ray_perception_settings", false);
        semanticMapSettings = sensorSideChannel.GetWithDefault<bool>("semantic_map_settings", false);
        cameraSettings = sensorSideChannel.GetWithDefault<bool>("camera_settings", false);

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
            raySensor.MaxRayDegrees = sensorSideChannel.GetWithDefault<float>("max_ray_degrees", maxRayDegrees);
            raySensor.DetectableTags = new List<string>() { "Building", "Item", "Road", "Car", "Ground" };
        }

        if (semanticMapSettings)
        {
            semanticMapSensor = sensors.AddComponent<Semantic3DMapComponent>();
            semanticMapSensor.root = body;
            semanticMapSensor._gridX = sensorSideChannel.GetWithDefault<int>("semantic_map_x", gridX);
            semanticMapSensor._gridY = sensorSideChannel.GetWithDefault<int>("semantic_map_y", gridY);
            semanticMapSensor._gridZ = sensorSideChannel.GetWithDefault<int>("semantic_map_z", gridZ);
            semanticMapSensor.offset_x = 1.0f;
            semanticMapSensor.offset_y = 1.0f;
            semanticMapSensor.offset_z = 1.0f;
            semanticMapSensor._gridScale = 2.0f;
            semanticMapSensor._gridHeight = 2.0f;


            semanticMapSensor.Tags = new List<string>() { "Building", "Item", "Road", "Car", "Ground" };
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
