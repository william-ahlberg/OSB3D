using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepHandler : MonoBehaviour
{
    [SerializeField] float maxStepHeight = 0.7f;
    [SerializeField] float minStepDepth = 0.7f;
    Rigidbody rb;
    [SerializeField] Transform forwardFace;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
   
             
    }

    // Change to GetContacts later for better memory allocation
    private void OnCollisionEnter(Collision collision) {
   
        
        
        foreach (ContactPoint contact in collision.contacts)
        {
            RaycastHit hit;
            Debug.Log(transform.InverseTransformPoint(contact.point));

            if ((transform.InverseTransformPoint(contact.point).y <= 1f) && (transform.InverseTransformPoint(contact.point).y >= -0.45f))
            {
                if (rb.SweepTest(forwardFace.up, out hit, maxStepHeight));
                {
                    transform.Translate(new Vector3(0f,1f,0f));


                    if (rb.SweepTest(forwardFace.forward, out hit, minStepDepth))
                    {
                        
                    }
        }




               
        }
           
            
            
            
           
        
        } 
    }
}
