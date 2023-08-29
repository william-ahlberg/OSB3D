using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveElevator : MonoBehaviour
{
    [SerializeField] float speed; 

    static bool powerOn;
    static bool goUp;
    static bool goDown; 
    static bool moving;

    Vector3 maxPos; 


    private void Start()
    {
        powerOn = false;
        goUp = false; 
        goDown = false;
        moving = false;

        maxPos = new Vector3(transform.position.x, 10, transform.position.z);
    }

    private void Update()
    {

        if (moving) 
        {
            Debug.Log("y pos: " + transform.position.y);

            if (goUp)
            {
                if (Vector3.Distance(transform.position, maxPos) > 0.001f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, maxPos, speed * Time.deltaTime);
                }

                else
                {
                    goUp = false;
                    moving = false;
                }
            }

            else if (goDown  && transform.position.z > 0)
            {
                if(Vector3.Distance(transform.position, new Vector3(transform.position.x, 0f, transform.position.z)) > 0.001f)
                {
                    transform.position -= new Vector3(0, speed * Time.deltaTime, 0);

                }

                else
                {
                    goDown = false;
                    moving = false;
                }
            }
        }
    }



    public static bool ButtonPressed(int _buttonNr)
    {
        bool toReturn = false; 

        switch(_buttonNr)
        {
            case 0:
                powerOn = !powerOn;
                toReturn = powerOn; 
                break;

            case 1:
                if (powerOn)
                {
                    goUp = true;
                    goDown = false;
                    toReturn = true;
                }

                else toReturn = false; 
                break;

            case 2:
                if (powerOn && (goUp || goDown))
                {
                    moving = true;
                    toReturn = true;
                }

                else toReturn = false; 
                break;

            case 3:
                if (powerOn)
                {
                    goDown = true;
                    goDown = false;
                    toReturn = true;
                }

                else toReturn = false; 

                break;

            case 4:
                powerOn = !powerOn;
                toReturn = powerOn;
                break;
        }

        return toReturn;
    }
}