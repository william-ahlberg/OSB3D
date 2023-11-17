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
    float[] actions = new float[5];
    Vector3 startPosition;
    AgentController agentController;
    Transform agentBody;

    // Start is called before the first frame update
    private void Start()
    {
        agentController = GetComponent<AgentController>();
        agentBody = transform.Find("Body");
        startPosition = transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        Debug.Log("Agent position" + transform.position);
        //Debug.Log("Agent rotation" + agentBody.localRotation);

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

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(agentBody.localRotation);
        sensor.AddObservation(transform.position);





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

    public void ResetAgent()
    {
        rigidBody.velocity = Vector3.zero;
        transform.position = startPosition;
    }


}
