using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BaseAgent : Agent
{
    float[] actions = new float[3];
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

    public override void CollectObservations(VectorSensor sensor)
    {


    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        actions[0] = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f); // Forward and backwards
        actions[1] = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f); // Right and left
        actions[2] = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f); // Rotate
        actions[2] = Mathf.Clamp(actionBuffers.ContinuousActions[3], 0f, 1f);  // Jump

        agentController.MoveAgent(actions);
    }

    public override void OnEpisodeBegin()
    {


    }

}
