using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class GadgetBug : BugBase
{	
    private Dictionary<string, string> searchNameGadgetPairs = new Dictionary<string, string>()
        {
		{"Door", "^Trigger"},
		{"Elevator", "[0-4]"}
        };
	
	
    private List<GameObject> doors = new List<GameObject>();
    
    // Start is called before the first frame update
    private void Start()
    {
        base.Start();
        DisableGadget("Door");
    }

    private void Update()
    {
    }

    private void DisableGadget(string gadgetKey)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders) 
        { 
            if (Regex.IsMatch(collider.name, searchNameGadgetPairs[gadgetKey])) 
            { 
                collider.enabled = false; 
            } 
        }
            
        
        
    }

    private void CrossGadget()
    {
        


    }

    private void AlterGadget()
    {



    }

		
	
	
	
	
	
	}




