using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float[] actions = new float[5];
    bool[] extraActions = new bool[5];

    private void Start() 
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public float[] PlayerInput()
    {
        actions[0] = Input.GetAxisRaw("Vertical");
        actions[1] = Input.GetAxisRaw("Horizontal");
        actions[2] = Input.GetAxisRaw("Mouse X");
        actions[3] = Input.GetAxisRaw("Mouse Y");
        actions[4] = System.Convert.ToSingle(Input.GetKey(KeyCode.Space));

        return actions;
    }

    public bool[] ExtraInput()
    {
        extraActions[0] = Input.GetKey(KeyCode.V);
       
        return extraActions; 
    
    
    
    }

}