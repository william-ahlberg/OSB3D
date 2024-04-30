using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class PhysicsBug : BugBase
{
    AgentController agentController;
    public float startJumpForce;
    public float startMoveSpeed;
    public float bugJumpForce;
    public float bugMoveSpeed;
    public bool playerInBug;

    public int physicsParameter;


    public override void Start() 
    {
        base.Start();
        base.bugType = "B8";

        GameObject agent = GameObject.Find("/Characters/Agent");
        agentController = agent.GetComponent<AgentController>();
        startJumpForce = agentController.jumpForce;
        startMoveSpeed = agentController.moveSpeed;
        bugJumpForce = Random.Range(0, 32);
        bugMoveSpeed = Random.Range(1, 100);
        playerInBug = false;
        physicsParameter = Random.Range(0, 2);
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
            playerInBug = true;
            ChangePhysics();
        }   

    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            playerInBug = false;

            ChangePhysics();
            playerInBug = false;
        }

    }

    private void ChangePhysics()
    {
        if (playerInBug)
        {
            if (physicsParameter == 0)
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
            if (physicsParameter == 0)
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
