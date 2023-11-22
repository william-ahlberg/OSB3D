using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class BugManager : MonoBehaviour
{
    public Bounds bounds;
    public string savePath = "C:\\Users\\William\\Projects\\osb3d\\OSB3D\\Data";
    public PhysicsBug bug;
    bool firstFrame = true;
    PhysicsBug[] bugs;
    int nBugs = 100;
    BugLogger bugLogger = new BugLogger();

    // Start is called before the first frame update
    void Start()
    {
        CalcBounds();
        firstFrame = false;
        CreateBugArea(nBugs);
        SearchBugObject();
        Debug.Log(Application.dataPath);
        bugs = FindObjectsByType<PhysicsBug>(FindObjectsSortMode.None);
        bugLogger.LogBug(bugs);
        Debug.Log("Test: " + bugLogger.logs[0].bugType);
        bugLogger.SerializeJson();
    }

    // Update is called once per frame
    void Update()
    {
        if (firstFrame == true)
        {

        } 
              

    }



    void CreateBugArea(int nBugs)
    {
        int cubeScaleMin = 1;
        int cubeScaleMax = 5;

        GameObject parentObject = new GameObject("CubeParent");
        for (int i = 0; i < nBugs; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(Random.Range(cubeScaleMin, cubeScaleMax), cubeScaleMax / 2, Random.Range(cubeScaleMin, cubeScaleMax));
            cube.tag = "Bug";
            cube.GetComponent<Collider>().isTrigger = true;
            cube.transform.parent = parentObject.transform;
            cube.transform.position = PlaceBugArea(cube, 0);
            bug = cube.AddComponent<PhysicsBug>();
            bug.Initialize(i);
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
                bugObject.AddComponent<GadgetBug>();
            }


        }
    }

    public Vector3 PlaceBugArea(GameObject cube, int depth)
    {
        RaycastHit hit;
        Vector3 position = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y/2), Random.Range(bounds.min.z, bounds.max.z));
        Collider[] checkColliders = Physics.OverlapBox(cube.transform.position, cube.transform.localScale / 2);

        bool validPosition = true;

        if (checkColliders.Length > 1)
        {
            validPosition = false;
        }

        foreach (Collider collider in checkColliders)
        {
            
            
                if (!collider.gameObject.CompareTag("Bug"))
                {
                    validPosition = false;
                    break;
                }
            }

            if (!validPosition && (depth < 100))
            {
                position = PlaceBugArea(cube, ++depth);
            }

            Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity);
            position.y = hit.point.y + cube.transform.localScale.y / 2;

            return position;
        
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
    public string savePath;
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

            i++;        
            
        }
    
    }

    public void SerializeJson()
    {
        string json = JsonHelper.ToJson(logs, true);
        Debug.Log(json);
        string filePath = "C:\\Users\\William\\Projects\\osb3d\\OSB3D\\Data\\" + "data.json";
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





