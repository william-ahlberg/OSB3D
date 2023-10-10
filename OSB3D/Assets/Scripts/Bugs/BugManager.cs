using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


public class BugManager : MonoBehaviour
{
    Dictionary<string,int> BugTypeCount;
    //Dictionary<string,int> IsActiveBugType = {};

    List<BugBase> bugs = new List<BugBase>();
    public Bounds bounds;
    public List<Vector3> bugPositions = new List<Vector3>();



    // Start is called before the first frame update
    void Start()
    {
        CalcBounds();
        CreateBugArea();
        SearchBugObject();
        LogBugPosition();
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
            cube.transform.position = PlaceBugArea(cube,0);
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

    public void SearchBugObject()
    {
        GameObject[] bugObjects; 
        bugObjects = GameObject.FindGameObjectsWithTag("Bug");
        foreach (GameObject bugObject in bugObjects)
        {
            if (Regex.IsMatch(bugObject.name, "Elevator"))
            {
                bugObject.AddComponent<GadgetBugs>();
                //LogBugPosition(bugObject.transform.position);
            }


        }

    }

    public void LogBugPosition()
    {
        GameObject[] bugObjects;

        bugObjects = GameObject.FindGameObjectsWithTag("Bug");
        foreach (GameObject bugObject in bugObjects)
        {
            bugPositions.Add(bugObject.transform.position);
        }
    }

    public Vector3 PlaceBugArea(GameObject cube, int depth)
    {
        RaycastHit hit;
        Vector3 position = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), Random.Range(bounds.min.z, bounds.max.z));
        Collider[] freeColliders = Physics.OverlapBox(cube.transform.position, cube.transform.localScale / 2);

        bool validPosition = true;

        foreach (Collider collider in freeColliders)
        {
            if (!collider.gameObject.CompareTag("Bug"))
            {
                validPosition = false;
                break;
            }
        }

        if (!validPosition && (depth < 100))
        {
            //Debug.Log("Finding new position for Cube " + ID);
            position = PlaceBugArea(cube,++depth);
        }

        Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity);
        position.y = hit.point.y + cube.transform.localScale.y / 2;

        return position;

    }


}
