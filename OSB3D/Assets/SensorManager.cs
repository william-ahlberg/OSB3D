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
    bool haveCameraSensor = false;
    Camera agentCamera;
    CameraSensorComponent cameraSensor;
    int cameraSensorWidth = 1920;
    int cameraSensorHeight = 1080;
    bool isGrayscale = false;
    
    //Rays
    bool haveRaySensor = true;
    RayPerceptionSensorComponent3D raySensor;
    float maxRayDegrees = 180;

    //Semantic Map
    bool haveSemanticMap = false;
    Semantic3DMapComponent semanticMapSensor;
    EnvironmentParameters envParameters;
    float gridX = 5;
    float gridY = 5;
    float gridZ = 5;

    public void Awake()
    {
        sensorSideChannel = new SensorSideChannel();
        SideChannelManager.RegisterSideChannel(sensorSideChannel);
    }

    // Start is called before the first frame update
    void Start()
    {
        body = GameObject.Find("Body");
        sensors = GameObject.Find("Sensors");
        envParameters = Academy.Instance.EnvironmentParameters;
        if (haveCameraSensor)
        { 
            agentCamera = GetComponentInChildren<Camera>();
            cameraSensor = gameObject.AddComponent<CameraSensorComponent>();
            cameraSensor.Width = (int)envParameters.GetWithDefault("camera_sensor_width", cameraSensorWidth);
            cameraSensor.Height = (int)envParameters.GetWithDefault("camera_sensor_height",cameraSensorHeight);
            cameraSensor.Grayscale = isGrayscale;
            cameraSensor.SensorName = "AgentCameraSensor";
            cameraSensor.Camera = agentCamera;
        }

        if (haveRaySensor)
        {
            raySensor = sensors.AddComponent<RayPerceptionSensorComponent3D>();
            raySensor.SensorName = "RaySensor";
            raySensor.RaysPerDirection = (int)31f;
            raySensor.MaxRayDegrees = envParameters.GetWithDefault("max_ray_degrees", maxRayDegrees);
            raySensor.DetectableTags = new List<string>() { "Building", "Item", "Road", "Car", "Ground" };
        }

        if (haveSemanticMap)
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
