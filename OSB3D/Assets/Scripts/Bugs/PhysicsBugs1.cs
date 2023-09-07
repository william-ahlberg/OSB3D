using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBugs1 : BugBase
{
    private Bounds gameAreaBounds;
    [SerializeField] private int numberOfBugs;
    private Transform parent;
    private GameObject newParent;
    private string searchTag = "Bug";
    private bool firstFrame = true; //Used to generate bugs after PCG
    private Renderer bugRenderer;
    // Start is called before the first frame update
    void Awake()
    {
       bugRenderer = GetComponent<Renderer>();
       bugRenderer.enabled = !bugRenderer.enabled;
    }

    private void CalculateGameAreaBounds()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            gameAreaBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length - 1; i++)
            {
                gameAreaBounds.Encapsulate(renderers[i].bounds);
            }

        }

    }

    // Update is called once per frame

    public Bounds GetBounds()
    {
        return gameAreaBounds;
    }

    private void Update()
    {
        if(firstFrame)
        {
            CalculateGameAreaBounds();
            MakeCubes(numberOfBugs);
            firstFrame = false;
        }
      

    }

    private void MakeCubes(int numberOfCubes)
    {
        GameObject parentObject = new GameObject("CubeParent");
        parentObject.AddComponent<CubeIntersection>();
        for (int i = 0; i < numberOfCubes; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.tag = searchTag;
            cube.transform.localScale = new Vector3(Random.Range(1,5), 3f, Random.Range(1,5));
            cube.AddComponent<CubeIntersection>();
            cube.GetComponent<Collider>().isTrigger = true;
            cube.transform.parent = parentObject.transform;
            cube.transform.position = GetRandomPosition(cube.transform.localScale, 0, i);

            
        }
    }
    
    private override BugTrigger()
    {
        
    
    
    
    
    
    }
    
    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Player")
        {
            inBug = true;

            Debug.Log("Cube Intersection");
            Debug.Log(agentController);
            ChangePhysics();
        }   

    }

    private void OnTriggerExit(Collider other)
    {

        if (other.tag == "Player")
        {
            inBug = false;

            Debug.Log("Cube Intersection");
            Debug.Log(agentController);
            ChangePhysics();
            inBug = false;
        }

    }
    
    private override void BugVisual()
    {
        bugRenderer.enabled = !bugRenderer.enabled;
    }
    
    private override void BugPlace()
    {
        RaycastHit hit;
     
        Bounds sceneBounds = GetBounds(); 
        Vector3 randomPosition = new Vector3(Random.Range(sceneBounds.min.x, sceneBounds.max.x),    Random.Range(0, 10), Random.Range(sceneBounds.min.z, sceneBounds.max.z));
        Collider[] freeColliders = Physics.OverlapBox(randomPosition, cubeScale/2);
        
        bool validPosition = true;

        foreach(Collider collider in freeColliders)
        {
            if(!collider.gameObject.CompareTag(searchTag))
            {
                validPosition = false;
                break;
            }
        }

        if (!validPosition && (depth < 100)) 
        {
            //Debug.Log("Finding new position for Cube " + ID);
            randomPosition = GetRandomPosition(cubeScale, ++depth, ID);
        }

        Physics.Raycast(randomPosition , Vector3.down, out hit, Mathf.Infinity);
        randomPosition.y = hit.point.y + cubeScale.y/2;
        
        transform.position = randomPosition;
    
    
    
    }

    
}
