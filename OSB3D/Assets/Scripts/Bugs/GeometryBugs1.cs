using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryBugs1 : BugBase
{
    // Start is called before the first frame update
    List<Collider> edgeNarrowColliders = new List<Collider>();
    private string searchTag = "Bug";
    private string searchName = "EdgeNarrow";
    [SerializeField] private int _numberOfBugs = 0;
    LevelController levelController;

    public int NumberOfBugs
    {
        get
        {
            return Mathf.Min(_numberOfBugs, 2*((levelController.blockCountX-1) + (levelController.blockCountZ)-1));
        }
        set
        {
            _numberOfBugs = value;
        }
    }

    void Start()
    {
        levelController = GetComponent<LevelController>();

        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Bug");
        foreach (GameObject obj in gameObjects)
        {
            if (obj.GetComponent<Collider>() != null && obj.name == searchName)
            {
                edgeNarrowColliders.Add(obj.GetComponent<Collider>());


            }

        }


        for (int i = 0; i < NumberOfBugs; i++) 
        {
            edgeNarrowColliders[i].enabled = false;
        
        
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
