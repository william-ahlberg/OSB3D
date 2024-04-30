using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;
using Unity.MLAgents.SideChannels;


public class BugManager : MonoBehaviour
{
    public Bounds bounds;
    bool firstFrame = true;
    BugBase[] bugs;
    BugLogger bugLogger = new BugLogger();
    BugSideChannel bugSideChannel;
    
    private int highestBugId = 0;
    
    public int HighestBugId 
    { 

        get 
        { 
            highestBugId++;
            return highestBugId;
        }
    
    
    
    }
    void Awake()
    {
        bugSideChannel = new BugSideChannel();
        SideChannelManager.RegisterSideChannel(bugSideChannel);
    }

    void Update()
    {



        if (firstFrame == true)
        {
            CalcBounds();
            firstFrame = false;

            CreateBugArea<GeometryBug>(bugSideChannel.GetWithDefault<int>("geometry", 0));
            CreateBugArea<PhysicsBug>(bugSideChannel.GetWithDefault<int>("physics", 0));
            SearchBugObject<GadgetBug>(bugSideChannel.GetWithDefault<int>("gadget", 10));
            SearchBugObject<StateBug>(bugSideChannel.GetWithDefault<int>("state", 10));
            SearchBugObject<LogicBug>(bugSideChannel.GetWithDefault<int>("logic", 10));
            
   
            bugs = FindObjectsByType<BugBase>(FindObjectsSortMode.None);
            bugLogger.LogBug(bugs);
            bugLogger.SerializeJson();
            SetBugColor();
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
            cube.AddComponent<T>();

            cube.tag = "Bug";
            cube.GetComponent<Collider>().isTrigger = true;
            cube.transform.parent = parentObject.transform;
            cube.transform.position = PlaceBugArea(cube, 0);
            if (typeof(T) == typeof(PhysicsBug))
            {
                cube.transform.localScale = new Vector3(
                    Random.Range(cubeScaleMin, cubeScaleMax),
                    cubeScaleMax / 2,
                    Random.Range(cubeScaleMin, cubeScaleMax));

                cube.GetComponent<PhysicsBug>().position = cube.transform.position;    
                cube.GetComponent<PhysicsBug>().id = HighestBugId;
                cube.GetComponent<PhysicsBug>().bugClass = "PhysicsBug";
                cube.GetComponent<PhysicsBug>().bugType = "B8";
            }
            else if (typeof(T) == typeof(GeometryBug))
            {
                cube.transform.localScale = new Vector3(
                    Random.Range(cubeScaleMin, cubeScaleMax),
                    cubeScaleMax / 2,
                    Random.Range(cubeScaleMin, cubeScaleMax));
                cube.GetComponent<GeometryBug>().position = cube.transform.position;    
                cube.GetComponent<GeometryBug>().id = HighestBugId;
                cube.GetComponent<GeometryBug>().bugClass = "GeometryBug" ;
                cube.GetComponent<GeometryBug>().bugType = "B5" ;
            }

            
            
             
        }

    }

    private void SearchBugObject<T>(int numberOfBugs) where T : UnityEngine.Component
    {
        GameObject[] bugObjects;
        string bugRegex = "";
        bugObjects = GameObject.FindGameObjectsWithTag("Bug");
        int numberOfPlacedBugs = 0;


        if (typeof(T) == typeof(GadgetBug))
        {
            bugRegex = "^BP\\w*";
        }
        else if (typeof(T) == typeof(LogicBug))
        {
            bugRegex = "Elevator\\(Clone\\)";
        }
        else if (typeof(T) == typeof(StateBug))
        {
            bugRegex = "^BP\\w*";
        }

        foreach (GameObject bugObject in bugObjects)
        {

            if (Regex.IsMatch(bugObject.name, bugRegex))
            {
                if ((typeof(T) == typeof(GadgetBug)) & (numberOfPlacedBugs < numberOfBugs))
                {
                    bugObject.AddComponent<GadgetBug>();
                    ++numberOfPlacedBugs;
                    
                    GadgetBug gadgetBug = bugObject.GetComponent<GadgetBug>();
                    gadgetBug.id = HighestBugId;
                    gadgetBug.position = bugObject.transform.position;
                    gadgetBug.bugClass = "GadgetBug";
                    gadgetBug.bugType = "B2";

                }
                else if ((typeof(T) == typeof(LogicBug)) & (numberOfPlacedBugs < numberOfBugs))
                {
                    bugObject.AddComponent<LogicBug>();
                    ++numberOfPlacedBugs;
                    LogicBug logicBug = bugObject.GetComponent<LogicBug>();
                    logicBug.id = HighestBugId;
                    logicBug.position = bugObject.transform.position;
                    logicBug.bugClass = "LogicBug";
                    logicBug.bugType = "B9";
                }
                else if ((typeof(T) == typeof(StateBug)) & (numberOfPlacedBugs < numberOfBugs))
                {
                    bugObject.AddComponent<StateBug>();
                    ++numberOfPlacedBugs;
                    StateBug stateBug = bugObject.GetComponent<StateBug>();
                    stateBug.id = HighestBugId;
                    stateBug.position = bugObject.transform.position;
                    stateBug.bugClass = "StateBug";
                    stateBug.bugType = "B4";
                }
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
            if (!c.gameObject.CompareTag("Bug"))
            { 
                validPosition = false;
                break;
            }
        }
        

        if (!validPosition && depth < 256)
        {
            position = PlaceBugArea(cube, ++depth);
        }


        Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity);
        position.y = hit.point.y + cube.transform.localScale.y / 2;

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

    public void SetBugColor()
    {
        foreach (BugBase bug in bugs)
        {
            Renderer[] bugRenderers = bug.GetComponentsInChildren<Renderer>();
            foreach (Renderer bugRenderer in bugRenderers)
            {
                foreach (Material bugMaterial in bugRenderer.materials)
                {
                    bugMaterial.SetColor("_Color", Color.red);
                }
            
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
    private string savePath = "C://Users//William//Projects//osb3d//OSB3D//Assets";
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
        string filePath = savePath + "//Data//data.json";
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





