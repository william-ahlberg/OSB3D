using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class StateBug : BugBase
{

    private Dictionary<string, string> searchNameGadgetPairs = new Dictionary<string, string>()
        {
        {"Door", "BP10_Door\\w*"},
       
        };


    private List<GameObject> doors = new List<GameObject>();

    // Start is called before the first frame update
    private void Start()
    {
        base.Start();
        Transform bugDoor = transform.Find("BugDoor");
        Collider[] doorColliders = GetComponentsInChildren<Collider>();
        bugDoor.gameObject.SetActive(true);

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




