using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental;
using UnityEngine;

public class MoveElevator : MonoBehaviour
{
    [SerializeField] List<Renderer> buttonRenderers;

    static bool powerOn;
    static bool goUp;
    static bool goDown;
    static bool moving;
    bool runsCorutine; 

    Vector3 maxPos;
    Vector3 downVector;
    public float buildingHeight;

    private float travelTime = 12;

    private void Start()
    {
        powerOn = false;
        goUp = false;
        goDown = false;
        moving = false;
        runsCorutine = false;
    }

    //Called from LevelController, after block with elevator have been move into the correct place
    public void SetTargetPosition()
    {
        maxPos = new Vector3(transform.position.x, buildingHeight, transform.position.z);
        downVector = new Vector3(transform.position.x, 0, transform.position.z);
    }

    //Called from PlaceElevator when instanciating the elevator
    public void SetBuildingHeight(float _buildingHeight)
    {
        buildingHeight  = _buildingHeight;  
    }

    private void FixedUpdate()
    {
        if (moving && !runsCorutine)
        {
            //if moving and goUp is true
            if (goUp)
            {
                if (transform.position.y < maxPos.y)
                {
                    StartCoroutine(MovingElevator(maxPos));
                }

                else
                {
                    goUp = false;
                    buttonRenderers[1].material.color = Color.white;
                }
            }

            //if moving and goDown is true
            else if (goDown)
            {
                if (transform.position.y > 0)
                {
                    StartCoroutine(MovingElevator(downVector));
                }

                else
                {
                    goDown = false;
                    buttonRenderers[3].material.color = Color.white;
                }

            }

            //Resets all but powerOn
            else
            {
                goDown = false;
                buttonRenderers[3].material.color = Color.white;

                goUp = false;
                buttonRenderers[1].material.color = Color.white;

                moving = false;
                buttonRenderers[2].material.color = Color.white;
            }
        }
    }
    
    //Moves the elevator over a period of time
    private IEnumerator MovingElevator(Vector3 _moveTowards)
    {
        runsCorutine = true;
        Vector3 startPos = transform.position;

        float timeCounter = 0;

        while (timeCounter < travelTime)
        {
            timeCounter += Time.deltaTime;
            float stepSize = timeCounter / travelTime;
            transform.position = Vector3.Lerp(startPos, _moveTowards, stepSize);
            yield return new WaitForFixedUpdate();
        }

        transform.position = _moveTowards;
        powerOn = false;
        ResetBooleans();
        ResetButtonColours();
        runsCorutine = false;
    }

    //Reset booleans to move elevator
    void ResetBooleans()
    {
        goUp = false;
        goDown = false;
        moving = false;
    }

    //Resets all buttons to white when the elevator reached it's location
    void ResetButtonColours()
    {
        for (int i = 0; i < buttonRenderers.Count; i++)
        {
            buttonRenderers[i].material.color = Color.white;
        }
    }

    //Called from ElevatorButtonTrigger, checks which button has been pressed and sets the correct bools
    public static bool ButtonPressed(int _buttonNr)
    {
        bool toReturn = false;

        if (!moving)
        {
            switch (_buttonNr)
            {
                case 0:
                    powerOn = !powerOn;

                    if (powerOn)
                    {
                        moving = true;
                        goDown = true;
                        goUp = false;
                    }

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
                        goUp = false;
                        toReturn = true;
                    }

                    else toReturn = false;

                    break;

                case 4:
                    powerOn = !powerOn;

                    if (powerOn)
                    {
                        moving = true;
                        goUp = true;
                        goDown = false;
                    }

                    toReturn = powerOn;
                    break;
            }
        }

        return toReturn;
    }
}
