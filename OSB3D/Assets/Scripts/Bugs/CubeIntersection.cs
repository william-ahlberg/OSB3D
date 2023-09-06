using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeIntersection : MonoBehaviour
{
    AgentController agentController;
    float startJumpForce;
    float startMoveSpeed;
    float bugJumpForce;
    float bugMoveSpeed;
    bool inBug;
    int bugType;


    // Start is called before the first frame update
    private void Start()
    {
        GameObject agent = GameObject.Find("/Characters/Agent");
        agentController = agent.GetComponent<AgentController>();
        startJumpForce = agentController.jumpForce;
        startMoveSpeed = agentController.moveSpeed;
        bugJumpForce = Random.Range(0, 32);
        bugMoveSpeed = Random.Range(1, 100);
        inBug = false;
        bugType = Random.Range(0, 2);

    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Player")
        {
            inBug = true;

            Debug.Log("Cube Intersection");
            Debug.Log(agentController);
            ChangePhysics();
        }   

    }

    private void OnTriggerExit(Collider other)
    {

        if (other.tag == "Player")
        {
            inBug = false;

            Debug.Log("Cube Intersection");
            Debug.Log(agentController);
            ChangePhysics();
            inBug = false;
        }

    }

    private void ChangePhysics()
    {
        if (inBug)
        {
            if (bugType == 0)
            {
                agentController.jumpForce = bugJumpForce;



            }
            else
            {
                agentController.moveSpeed = bugMoveSpeed;


            }

        }
        else
        {
            if (bugType == 0)
            {
                agentController.jumpForce = startJumpForce;



            }
            else
            {
                agentController.moveSpeed = startMoveSpeed;


            }



        }
      
    
    
    
    }

}
