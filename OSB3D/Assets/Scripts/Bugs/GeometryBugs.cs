using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryBugs : MonoBehaviour
{
    // Start is called before the first frame update
    List<Collider> edgeNarrowColliders = new List<Collider>();
    void Start()
    {

        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Bug");
        foreach (GameObject obj in gameObjects)
        {
            if (obj.GetComponent<Collider>() != null)
            {
                edgeNarrowColliders.Add(obj.GetComponent<Collider>());


            }

        }

        Debug.Log("Number of Bug Colliders:" + edgeNarrowColliders.Count);


        foreach (Collider collider in edgeNarrowColliders) 
        { 
            collider.enabled = false;
        
        
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }



}
