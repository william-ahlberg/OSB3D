using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public GameObject playerGO;

    private Vector3 cameraPos; 

    void Start()
    {
        cameraPos = playerGO.transform.position - transform.position;
    }

    void LateUpdate()
    {
        transform.position = playerGO.transform.position - cameraPos;
    }
}
