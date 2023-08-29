using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class GadgetBugs : MonoBehaviour
{
    private string searchTag = "Bug";
    private string searchName = "^Trigger";
    private List<GameObject> doors = new List<GameObject>();

    // Start is called before the first frame update
    private void Start()
    {
        GetDoors();
    }

    private void Update()
    {

    }

    private void GetDoors()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(searchTag);
        foreach (GameObject obj in gameObjects)
        {
            if (Regex.IsMatch(obj.name, searchName))
            {
                doors.Add(obj);
            }


        }
    }

    private void DisableDoor()
    {
        
        doors[Random.Range(0, doors.Length - 1)].GetComponentInChildren<Collider>


    }

    private void GetDoorTriggers(List<GameObject> doors)
    { 
        
    
    
    
    }
}
