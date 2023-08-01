using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BaseAgent : Agent
{
    float[] actions = new float[5];
    AgentController agentController;
    // Start is called before the first frame update
    private void Start()
    {
        agentController = GetComponent<AgentController>();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
    
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode Begin");

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
       


    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Vector Observations
        sensor.AddObservation(transform.position); //Absolute agent position


    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        actions[0] = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f); // Vertical
        actions[1] = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f); // Horizontal
        actions[2] = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f); // Mouse X
        actions[3] = Mathf.Clamp(actionBuffers.ContinuousActions[3], -1f, 1f); // Mouse Y
        actions[4] = Mathf.Clamp(actionBuffers.ContinuousActions[4], 0f, 1f); // Jump

        agentController.MoveAgent(actions);
    }

}
