using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Globalization;
using Unity.MLAgents;

public class Elevator : MonoBehaviour
{
    [SerializeField] GameObject elevatorFrame;
    [SerializeField] GameObject elevator;


    void Start()
    {
        CultureInfo englishUSCulture = new CultureInfo("en-US");
        System.Threading.Thread.CurrentThread.CurrentCulture = englishUSCulture;

        Mesh buildingMesh = GetComponentsInChildren<MeshFilter>()[0].sharedMesh;

        Vector3 size = buildingMesh.bounds.size;
        float height = size.z * 100;

        string name = this.name;
        int floors = int.Parse(name.Substring(name.IndexOf("_F") + 2, 2));

        GameObject elevatorObject = new("Elevator");
        GameObject elevatorFrameObject = new("ElevatorFrame");

        Vector3 moveUp = Vector3.zero;
        int counter = 0;

        for (int i = 0; i < floors + 1; i++)
        {
            GameObject frame = Instantiate(elevatorFrame);
            frame.transform.position += moveUp;
            frame.transform.parent = elevatorFrameObject.transform;

            moveUp.y += 1;
            counter += 1;
        }

        float floorHeight = height / floors;
        Debug.Log("floorheight: " + floorHeight);
        Debug.Log("counter: " + counter);
        Vector3 scaleVector = new Vector3(1, floorHeight, 1);

        elevatorFrameObject.transform.localScale = scaleVector;

        elevatorFrameObject.transform.parent = elevatorObject.transform;

        GameObject plate = Instantiate(elevator);
        plate.transform.Find("ButtonPower2").position += new Vector3(0, (counter - 1) * floorHeight, 0);
        plate.transform.parent = elevatorObject.transform;

        Vector3 elevatorPosition = new Vector3(transform.position.x - (size.x / 2 * 100) - (elevatorFrame.transform.localScale.x * 2), 0, transform.position.z);
        elevatorObject.transform.Translate(elevatorPosition);

    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
    }
}

