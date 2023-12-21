using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;

public class BugManager : MonoBehaviour
{
    public Bounds bounds;
    //public PhysicsBug bug;
    bool firstFrame = true;
    //PhysicsBug[] bugs;
    int nBugs = 500;
    BugLogger bugLogger = new BugLogger();

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (firstFrame == true)
        {
            CalcBounds();
            firstFrame = false;
            CreateBugArea<GeometryBug>(100);
            CreateBugArea<PhysicsBug>(100);
            SearchBugObject();
            Debug.Log(Application.dataPath);
            /*bugs = FindObjectsByType<PhysicsBug>(FindObjectsSortMode.None);
            bugLogger.LogBug(bugs);
            Debug.Log("Test: " + bugLogger.logs[0].bugType);
            bugLogger.SerializeJson();*/
        }
    }



    private void CreateBugArea<T>(int numberOfBugs) where T : UnityEngine.Component
    {
        int cubeScaleMin = 1;
        int cubeScaleMax = 5;

        GameObject parentObject = new GameObject("GameBugs");
        for (int i = 0; i < numberOfBugs; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            if (typeof(T) == typeof(PhysicsBug))
            {
                cube.transform.localScale = new Vector3(
                    Random.Range(cubeScaleMin, cubeScaleMax),
                    cubeScaleMax / 2,
                    Random.Range(cubeScaleMin, cubeScaleMax));
            }
            else if (typeof(T) == typeof(GeometryBug))
            {
                cube.transform.localScale = new Vector3(
                    Random.Range(cubeScaleMin, cubeScaleMax),
                    cubeScaleMax / 2,
                    Random.Range(cubeScaleMin, cubeScaleMax));


            }


          

            cube.tag = "Bug";
            cube.GetComponent<Collider>().isTrigger = true;
            cube.transform.parent = parentObject.transform;
           
            cube.AddComponent<T>();
            cube.transform.position = PlaceBugArea(cube, 0);
            


            //bug.Initialize(i);
            //bug.id = i;
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
                bugObject.AddComponent<GadgetBug>();
            }


        }
    }

    public Vector3 PlaceBugArea(GameObject cube, int depth)
    {
        RaycastHit hit;
        bool validPosition = true;
        Vector3 position = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y * 1.1f, bounds.max.y / 2),
            Random.Range(bounds.min.z, bounds.max.z));

        Collider[] checkColliders = Physics.OverlapBox(position, cube.transform.localScale / 2);

        foreach (Collider c in checkColliders)
        {
            Debug.Log(c.gameObject.tag);
            if (!c.gameObject.CompareTag("Bug"))
            { 
                validPosition = false;
                break;
            }
        }
        

        if (!validPosition && depth < 256)
        {
            Debug.Log("Depth: " + depth);
            position = PlaceBugArea(cube, ++depth);
        }


        Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity);
        position.y = hit.point.y + cube.transform.localScale.y / 2;

        Debug.Log("Number of colliders: " + checkColliders.Length);
        return position;
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


}




[System.Serializable]
public class BugLogEntry
{
    public int id;
    public Vector3 position;
    public string bugClass;
    public string bugType;
    public bool isActive;

    public override string ToString() => $"Bug Log Entry: {position}";
}
public class BugLogger
{
    private string savePath = "/home/wilah/Projects/osb3d/OSB3D/Assets";
    public List<BugLogEntry> logs = new List<BugLogEntry>();

    public void LogBug(BugBase[] bugs)
    {
        int i = 0;
        foreach (BugBase bug in bugs)
        {
            logs.Add( new BugLogEntry() );
            logs[i].position = bug.position;
            logs[i].id = bug.id;
            logs[i].bugClass = bug.bugClass;
            logs[i].bugType = bug.bugType;
            logs[i].isActive = bug.isActive;

            i = i+1;        
            
        }
    
    }

    public void SerializeJson()
    {
        string json = JsonHelper.ToJson(logs, true);
        string filePath = savePath + "/Data/data.json";
        System.IO.File.WriteAllText(filePath, json);

    }

    public void DeserializeJson()
    { }
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.BugLog;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.BugLog = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.BugLog = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    public static string ToJson<T>(List<T> list)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.BugLog = list.ToArray();
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(List<T> list, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.BugLog = list.ToArray();
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }
    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] BugLog;
    }

}





