using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed;
    public Transform cameraPosition;

    private KeyCode[] letterKeys;
    private KeyCode[] arrowKeys;
    private Vector3[] moves;

    private Rigidbody player;
    private Vector3 playerMove;

    void Start()
    {
        player = GetComponent<Rigidbody>();
        player.freezeRotation = true; 
    }

    void Update()
    {
        Vector3 cameraForward = cameraPosition.forward;
        cameraForward.y = 0;

        Vector3 cameraRight = cameraPosition.right;
        cameraRight.y = 0;

        playerMove = cameraForward * Input.GetAxisRaw("Vertical") + cameraRight * Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        player.AddForce(playerMove.normalized * speed, ForceMode.Acceleration);
    }

}
