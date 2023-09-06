using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class GadgetBugs : MonoBehaviour
{
    private string searchTag = "Bug";
    private string searchName = "^BP";
    private string searchName2 = "^Trigger";
    private List<GameObject> doors = new List<GameObject>();
    [SerializeField] private int numberOfBugs = 1;

    // Start is called before the first frame update
    private void Start()
    {
        GetDoors();
        DisableButtons();
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

    private void DisableButtons()
    {
        for (int i = 0; i < numberOfBugs; i++) 
        {
            var bugDoor = doors[Random.Range(0, doors.Count)];
            Collider[] colliders = bugDoor.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders) 
            { 
                if (Regex.IsMatch(collider.name, searchName2)) 
                { 
                    collider.enabled = false; 
                } 
            }

        }

    }

    private void GetDoorTriggers()
    {
    
    
    
    }
}
