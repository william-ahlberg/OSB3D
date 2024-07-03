using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using System;
using Unity.MLAgents.SideChannels;

public class BaseAgent : Agent
{
    float[] actions = new float[6];
    Vector3 startPosition;
    AgentController agentController;
    Transform agentBody;
    Rigidbody rb;
    PlayerController playerController;
    Collider[] agentColliders;

    SensorSideChannel sensorSideChannel;
    string vectorObsSettings;
    public BugManager bugManager; 
    SensorManager sensorManager;
    InfoSideChannel infoSideChannel;
    private void Awake()
    {
        sensorSideChannel = new SensorSideChannel();
        infoSideChannel = new InfoSideChannel();
        SideChannelManager.RegisterSideChannel(infoSideChannel); 
        SideChannelManager.RegisterSideChannel(sensorSideChannel);
    }

    private void OnEnable()
    {
        sensorManager = GetComponent<SensorManager>();
        sensorManager.AddSensors(sensorSideChannel);
        base.OnEnable();
    }

    // Start is called before the first frame update
    private void Start()
    {
        bugManager = FindObjectOfType<BugManager>();
        if (bugManager == null)
        {
            Debug.LogError("No BugManager found in scene for BaseAgent.");
        }
        agentController = GetComponent<AgentController>();
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();
        agentBody = transform.Find("Body");
        startPosition = transform.position;
                
        vectorObsSettings = sensorSideChannel.GetWithDefault<string>("position_type", "none");
    }

    // Update is called once per frame
    private void Update()
    {
        if (playerController.ExtraInput()[1]) { OnEpisodeBegin(); }
        infoSideChannel.SendAgentInfo(transform.position);
    }

    private void FixedUpdate()
    {
            
    }

    public override void OnEpisodeBegin()
    {
        ResetAgent();
        Debug.Log("Episode Begin");

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
       


    }

    public Vector3 DivideComponentWise(Vector3 a, Vector3 b)
    {
        if(b.x == 0 || b.y == 0 || b.z == 0)
        {
            return new Vector3(0,0,0);
        }
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z); 
    }


    public Vector3 NormalizedPosition()
    {
        Vector3 positionNorm = 2 * DivideComponentWise(transform.position - bugManager.bounds.min, bugManager.bounds.max - bugManager.bounds.min) - new Vector3(1f,1f,1f);
        return positionNorm;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (vectorObsSettings == "normalized")
        {
            sensor.AddObservation(agentBody.localRotation);
            try
            {
                sensor.AddObservation(NormalizedPosition());
            }
            catch
            {
                sensor.AddObservation(new Vector3(0,0,0));
            }
        }

        else if (vectorObsSettings == "absolute")
        {
            sensor.AddObservation(agentBody.localRotation);
            sensor.AddObservation(transform.position);
        }
        sensor.AddObservation(agentController.CheckGround());

    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        actions[0] = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f); // Vertical
        actions[1] = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f); // Horizontal
        actions[2] = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f); // Mouse X
        actions[3] = Mathf.Clamp(actionBuffers.ContinuousActions[3], -1f, 1f); // Mouse Y
        actions[4] = Mathf.Clamp(actionBuffers.ContinuousActions[4], 0f, 1f); // Jump
        actions[5] = Mathf.Clamp(actionBuffers.ContinuousActions[5], 0f, 1f); // Identify

        agentController.MoveAgent(actions);
    }

    public void ResetAgent()
    {
        rb.velocity = Vector3.zero;
        Vector3 spawnPoint = infoSideChannel.GetWithDefault<Vector3>("spawn_point", startPosition);
        transform.position = spawnPoint;
        Debug.Log("Start position: " + transform.position);
 
        agentColliders = GetComponentsInChildren<Collider>();
        foreach (Collider agentCollider in agentColliders)
        {
            if ( agentCollider.GetType() != typeof(MeshCollider) )
            {
                agentCollider.enabled = true;
            }



        }
    }


}
