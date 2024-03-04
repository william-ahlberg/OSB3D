using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using System;


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
    BugManager bugManager = new BugManager();
    string vectorObsSettings;


    // Start is called before the first frame update
    private void Start()
    {
        agentController = GetComponent<AgentController>();
        playerController = GetComponent<PlayerController>();

        rb = GetComponent<Rigidbody>();
        agentBody = transform.Find("Body");
        startPosition = transform.position;

        sensorSideChannel = new SensorSideChannel();
        Debug.Log($"From the BaseAgent, Env Bounds {bugManager.bounds}");

        string vectorObsSettings = sensorSideChannel.GetWithDefault<string>("vector_obs_settings", "normalized");
        Debug.Log($"Vector obs {vectorObsSettings}");
    }

    // Update is called once per frame
    private void Update()
    {
        if (playerController.ExtraInput()[1]) { OnEpisodeBegin(); }
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

    public void NormalizePosition()
    {
        


    }

    public override void CollectObservations(VectorSensor sensor)
    {

        if (vectorObsSettings == "normalized")
        {
            sensor.AddObservation(agentBody.localRotation);
            sensor.AddObservation(transform.position);
        }

        else if (vectorObsSettings == "absolute")
        {
            sensor.AddObservation(agentBody.localRotation);
            sensor.AddObservation(transform.position);
        }
    

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
        transform.position = startPosition;
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
