using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeIntersection : MonoBehaviour
{
    AgentController agentController;
    float startJumpForce;
    float startMoveSpeed;


    // Start is called before the first frame update
    private void Start()
    {
        GameObject agent = GameObject.Find("/Characters/Agent");
        agentController = agent.GetComponent<AgentController>();

        startJumpForce = agentController.jumpForce;
        startMoveSpeed = agentController.moveSpeed;
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Cube Intersection");
            Debug.Log(agentController);
            agentController.jumpForce = 0;
        }   

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Cube Intersection");
            Debug.Log(agentController);
            agentController.jumpForce = startJumpForce;
        }

    }

    private void ChangePhysics()
    { 
        
    
    
    
    }

}
