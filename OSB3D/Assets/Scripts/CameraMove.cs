using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public GameObject playerGO;
    public float lookSpeed;
    public float damping; 

    private Vector3 cameraPos;
    private Vector3 cameraRot;


    void Start()
    {
        cameraPos = playerGO.transform.position - transform.position;
    }

    void LateUpdate()
    {
        transform.position = playerGO.transform.position - cameraPos;

        cameraRot.x += Input.GetAxis("Mouse X") * lookSpeed;
        //cameraRot.y -= Input.GetAxis("Mouse Y") * lookSpeed;
        //cameraRot.y = Mathf.Clamp(cameraRot.y, -20, 20);

        //Quaternion addRotation = Quaternion.Euler(cameraRot.y, cameraRot.x, 0);

        Quaternion addRotation = Quaternion.Euler(cameraRot.y, cameraRot.x, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, addRotation, Time.deltaTime * damping); 
    }
}
