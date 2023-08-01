using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    Vector3 offset = new Vector3(0f, 0f, 0f);
    [SerializeField] Transform cameraPosition;
    [SerializeField] Transform head;

    [SerializeField] PlayerController playerController;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.ExtraInput()[0] == true)
        {
            offset = new Vector3(0f, 2.5f, -5f);
        }
        else
        {
            offset = new Vector3(0f,0f,0f);
        }
        
        float cameraAngle = head.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, cameraAngle, 0);
        cameraPosition.position = head.position + rotation * offset;
        transform.position = cameraPosition.position;
    }


}
