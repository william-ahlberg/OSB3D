using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class StateBug : BugBase
{

    private Dictionary<string, string> searchNameGadgetPairs = new Dictionary<string, string>()
        {
        {"Door", "BP[10][05]_Door\\w*"},
       
        };


    private List<GameObject> doors = new List<GameObject>();
    Transform bugDoor;
    Transform bugDoorLeft;
    Transform bugDoorRight;


    // Start is called before the first frame update
    private void Start()
    {
        base.Start();
        
        bugDoor = transform.Find("BugDoor");
        bugDoorLeft = transform.Find("BugDoorRight");
        bugDoorRight = transform.Find("BugDoorLeft");

        Collider[] doorColliders = GetComponentsInChildren<Collider>();

        bugDoor?.gameObject.SetActive(true);
        bugDoorRight?.gameObject.SetActive(true);
        bugDoorLeft?.gameObject.SetActive(true);

        foreach (Collider doorCollider in doorColliders)
        {
            if (Regex.IsMatch(doorCollider.name, searchNameGadgetPairs["Door"]))
            {
                doorCollider.enabled = false;
            }
        }


    }

    private void Update()
    {
    }

    private void CrossState(string stateKey)
    { 
        
        
    
    }









}




