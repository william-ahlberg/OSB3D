using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


public class BugManager : MonoBehaviour
{
    Dictionary<string,int> BugTypCount;
    List<BugBase> bugs = new List<BugBase>();
    public Bounds bounds;
    // Start is called before the first frame update
    void Start()
    {
        CalcBounds();
        CreateBugArea();
        searchBugObject();
    }

    // Update is called once per frame
    void Update()
    {
    }



    void CreateBugArea() 
    {
        Debug.Log(bounds);
        GameObject parentObject = new GameObject("CubeParent");
        for (int i = 0; i < 100; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(Random.Range(1,5), 3f, Random.Range(1,5));
            cube.tag = "Bug";

            cube.GetComponent<Collider>().isTrigger = true;
            cube.transform.parent = parentObject.transform;
            cube.AddComponent<PhysicsBugs>();
            cube.GetComponent<PhysicsBugs>().id = i;
        }

    }

    public void CalcBounds()
    {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length - 1; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

    }

    public void searchBugObject()
    {
        GameObject[] bugObjects; 
        bugObjects = GameObject.FindGameObjectsWithTag("Bug");

        foreach (GameObject bugObject in bugObjects)
        {
            if (Regex.IsMatch(bugObject.name, "^Trigger"))
            {
                bugObject.AddComponent<GadgetBugs>();
            }


        }

    }

  
}
