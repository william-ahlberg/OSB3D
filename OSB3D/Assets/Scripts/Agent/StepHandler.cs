using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepHandler : MonoBehaviour
{
    [SerializeField] private float maxStepHeight = 0.2f;
    [SerializeField] private float minStepDepth = 0.2f;
    Rigidbody rb;
    float startHeight;
    [SerializeField] Transform forwardFace;
    Collider[] agentColliders;
    RaycastHit hitCollider;
    bool hitCheck;
    Vector3 agentSize;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startHeight = transform.position.y;
        agentColliders = GetComponentsInChildren<Collider>();
        Debug.Log("Centre" + transform.position);
        Debug.Log("Size" + GetHeight());
    }

    // Update is called once per frame
    void Update()
    {
             
    }

    Vector3 GetHeight()
    {
        Vector3 agentHeight = Vector3.zero;
        foreach (Collider collider in agentColliders)
        {
            agentHeight += collider.bounds.size;

        }
        return agentHeight;
    
    }

    // Change to GetContacts later for better memory allocation
    private void OnCollisionEnter(Collision collision) {
        foreach (ContactPoint contact in collision.contacts)
        {
            RaycastHit hit;
            float contactHeight = transform.InverseTransformPoint(contact.point).y + GetHeight().y;
            Debug.Log(contactHeight);

            if ((contact.normal.y < 0.9f ) && contactHeight < maxStepHeight)
            {
                hitCheck = Physics.BoxCast(transform.position, new Vector3(0.6f, GetHeight().y, 0.6f), transform.up, out hitCollider, transform.rotation, maxStepHeight + GetHeight().y);
                if (!hitCheck)
                {
                    hitCheck = Physics.BoxCast(transform.position + new Vector3(0f, maxStepHeight, 0f),
                    new Vector3(0.6f, GetHeight().y, 0.6f),
                    transform.forward,
                    out hitCollider,
                    transform.rotation,
                    minStepDepth);

                    if (!hitCheck)
                    {
                        transform.Translate(new Vector3(0f, contactHeight / 4, 0f));
                        transform.Translate(new Vector3(contact.normal.x,0f,contact.normal.z).normalized*-minStepDepth*0.99f);
                    }

                }


                    

                
               
             }

        
        }
    }



}
