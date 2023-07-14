using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float[] actions = new float[5];

    public float[] PlayerInput()
    {
        actions[0] = Input.GetAxisRaw("Vertical");
        actions[1] = Input.GetAxisRaw("Horizontal");
        actions[2] = System.Convert.ToSingle(Input.GetKey(KeyCode.Space));
        
        return actions;

    }


   

}