using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBugs : BugBase
{
    AgentController agentController;
    float startJumpForce;
    float startMoveSpeed;
    float bugJumpForce;
    float bugMoveSpeed;
    bool inBug;
    int bugType;

    public override void Start() 
    {
        base.Start();
        GameObject agent = GameObject.Find("/Characters/Agent");
        agentController = agent.GetComponent<AgentController>();
        startJumpForce = agentController.jumpForce;
        startMoveSpeed = agentController.moveSpeed;
        bugJumpForce = Random.Range(0, 32);
        bugMoveSpeed = Random.Range(1, 100);
        inBug = false;
        bugType = Random.Range(0, 2);
    }

    private void Update()
    {
    
    
    
    
    } 

    protected virtual void ToggleBugVisual()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.enabled = !renderer.enabled;
    }

    protected virtual void BugBehavior()
    {
        
    
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            inBug = true;

            Debug.Log("Cube Intersection");
            Debug.Log(agentController);
            ChangePhysics();
        }   

    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player"))
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
