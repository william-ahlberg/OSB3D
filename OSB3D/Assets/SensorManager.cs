using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;


public class SensorManager : MonoBehaviour
{


    Camera agentCamera;
    CameraSensorComponent cameraSensor;
    int cameraSensorWidth = 1920;
    int cameraSensorHeight = 1080;
    bool isGrayscale = false;
    EnvironmentParameters envParameters

    // Start is called before the first frame update
    void Start()
    {
        envParameters = Academy.Instance.EnvironmentParameters;

        agentCamera = GetComponentInChildren<Camera>();
        cameraSensor = gameObject.AddComponent<CameraSensorComponent>();

        cameraSensor.Width = (int)envParameters.GetWithDefault("camera_sensor_width", cameraSensorWidth);
        cameraSensor.Height = (int)envParameters.GetWithDefault("camera_sensor_height",cameraSensorHeight);
        cameraSensor.Grayscale = isGrayscale;
        cameraSensor.SensorName = "AgentCameraSensor";
        cameraSensor.Camera = agentCamera;


    }

    // Update is called once per frame
    void Update()
    {
    }
}
