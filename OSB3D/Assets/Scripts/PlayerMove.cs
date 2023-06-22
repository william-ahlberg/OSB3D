using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed;

    private KeyCode[] letterKeys;
    private KeyCode[] arrowKeys;
    private Vector3[] moves;

    /* RIGID BODY NOT USED
    private Rigidbody ball;
    private float xAxis;
    private float zAxis;*/

    void Awake()
    {
        letterKeys = new KeyCode[] { KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S };
        arrowKeys = new KeyCode[] { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow };
        moves = new Vector3[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        //ball = GetComponent<Rigidbody>(); 
    }

    void Update()
    {
        for (int i = 0; i < letterKeys.Length; i++)
        {
            if (Input.GetKey(letterKeys[i]) || Input.GetKey(arrowKeys[i]))
            {
                transform.position += moves[i] * Time.deltaTime * speed;
            }
        }

        /*xAxis = Input.GetAxis("Horizontal");
        zAxis = Input.GetAxis("Vertical");*/
    }

    /*void FixedUpdate()
    {
        ball.AddForce(new Vector3(xAxis, 0f, zAxis) * Speed);
    }*/
}
