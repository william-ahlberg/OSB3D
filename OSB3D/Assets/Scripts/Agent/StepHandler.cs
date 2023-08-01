using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepHandler : MonoBehaviour
{
    [SerializeField] float maxStepHeight = 1.6f;
    [SerializeField] float minStepDepth = 0.7f;
    Rigidbody rb;
    float startHeight;
    [SerializeField] Transform forwardFace;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startHeight = transform.position.y;
        
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

            if ((contact.normal.y < 0.9f ))
            {
                Debug.Log("Contact normal" + contact.normal.y);
               
                float contactHeight = Mathf.Abs(transform.InverseTransformPoint(contact.point).y - startHeight);

                Debug.Log("Contact height" + contactHeight);
                if (contactHeight < maxStepHeight)
                {
                   
                    transform.Translate(new Vector3(0f, 0.09f, 0f));
                    Debug.Log("Moved");
                }

               



               
        }
           
            
            
            
           
        
        } 
    }
}
