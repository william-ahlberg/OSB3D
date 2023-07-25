using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentCamera : MonoBehaviour
{

    [SerializeField] Transform head;


    // Start is called before the first frame update


    // Update is called once per frame
    private void Start() 
    {
    
    }
    
    private void Update()
    {
        transform.rotation = head.rotation;
    }   
}
