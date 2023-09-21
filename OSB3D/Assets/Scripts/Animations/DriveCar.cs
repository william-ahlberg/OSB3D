using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using static UnityEditor.PlayerSettings;

public class DriveCar : MonoBehaviour
{
    bool driving; 
    float speed;
    float min; 
    float maxX;
    float maxZ; 
    Vector3 direction;

    bool setDirection;
    bool stopOnCollision;
    private float waitTime = 1;

    /*function called from the RoadGenerator-class after instantiating a new car*/
    public void NewCar(bool _driving, float _min, float _maxX, float _maxZ, float _speed, bool _stopOnCollison)
    {
        driving = _driving;
        min = _min;
        maxX = _maxX;
        maxZ = _maxZ;
        speed = _speed;
        setDirection = true; 
        stopOnCollision = _stopOnCollison;
    }

    void Update()
    {

        //direction is set on the first update, depends on the cars rotation
        if (setDirection)
        {
            float rotationEuler = transform.rotation.eulerAngles.y;

            if (rotationEuler == 0) direction = Vector3.forward;
            else if (rotationEuler == 90) direction = Vector3.right;
            else if (rotationEuler == 180) direction = Vector3.back;
            else if (rotationEuler == 270) direction = Vector3.left;
            else direction = Vector3.zero;

            setDirection = false;
        }

        
        if(driving && !setDirection)
        {
            transform.Translate(direction * Time.deltaTime * speed, Space.World);

            //checks if the car goes outside the city map and if so, moves it to the other side of the map
            if (transform.position.x < min)
            {
                transform.position = new Vector3(maxX, transform.position.y, transform.position.z);
            }

            else if (transform.position.x > maxX)
            {
                transform.position = new Vector3(min, transform.position.y, transform.position.z);
            }


            else if (transform.position.z < min)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, maxZ);
            }


            else if (transform.position.z > maxZ)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, min);
            }
        }
    }

    public Vector3 GetDirection()
    {
        return direction;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (stopOnCollision)
        {
            //check for collisions with other cars
            if (collision.collider.CompareTag("Car"))
            {
                //calculate in which direction the collision happened for this car, compared to the cars direction
                Vector3 collisionPoint = collision.GetContact(0).point;
                Vector3 collisionDirection = Vector3.Normalize(collisionPoint - transform.position);
                float angle = Vector3.Angle(direction, collisionDirection);

                //calculate in which direction the collision happened for the other care, compared to the cars direction
                Vector3 otherCollisionDirection = Vector3.Normalize(collisionPoint - collision.transform.position);
                Vector3 otherDirection = collision.collider.gameObject.GetComponent<DriveCar>().GetDirection();
                float otherAngle = Vector3.Angle(otherDirection, otherCollisionDirection);

                //the car with the smallest angle between collision and direction waits 
                if (angle < otherAngle) StartCoroutine(Wait());
            }
        }
    }

    //wait after a collision (used if stopOnCollision is true, set in RoadGenerator)
    private IEnumerator Wait()
    {
        driving = false;

        yield return new WaitForSeconds(waitTime);

        driving = true;
    }
}
