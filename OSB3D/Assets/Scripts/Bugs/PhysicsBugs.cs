using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBugs : MonoBehaviour
{
    private Bounds gameAreaBounds;
    [SerializeField] private int numberOfBugs;
    private Transform parent;
    private GameObject newParent;

    // Start is called before the first frame update
    void Start()
    {
        // CalculateGameAreaBounds();
        // Debug.Log(GetBounds());
        // MakeCubes(numberOfBugs);
    }

    private void CalculateGameAreaBounds()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Debug.Log(renderers.Length);
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
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("Generate");
            CalculateGameAreaBounds();
            Debug.Log(GetBounds());
            MakeCubes(numberOfBugs);
        }     
    
    }

    private void MakeCubes(int numberOfCubes)
    {
        GameObject parentObject = new GameObject("CubeParent");
        parentObject.AddComponent<CubeIntersection>();
        for (int i = 0; i < numberOfCubes; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.tag = "BugCube";
            cube.transform.localScale = new Vector3(Random.Range(1,5), Random.Range(1,5), Random.Range(1,5));
            cube.AddComponent<CubeIntersection>();
            cube.GetComponent<Collider>().isTrigger = true;
            cube.transform.parent = parentObject.transform;
            cube.transform.position = GetRandomPosition(cube.transform.localScale, 0, i);

            
        }
    }

    private Vector3 GetRandomPosition(Vector3 cubeScale, int depth, int ID)
    {
        RaycastHit hit;
     
        Bounds sceneBounds = GetBounds(); 
        Vector3 randomPosition = new Vector3(Random.Range(sceneBounds.min.x, sceneBounds.max.x), Random.Range(0, sceneBounds.max.y), Random.Range(sceneBounds.min.z, sceneBounds.max.z));
        Collider[] freeColliders = Physics.OverlapBox(randomPosition, cubeScale/2);
        
        bool validPosition = true;

        foreach(Collider c in freeColliders)
        {
            if(!c.gameObject.CompareTag("BugCube"))
            {
                validPosition = false;
                break;
            }
        }

        if (!validPosition & (depth < 100)) 
        {
            Debug.Log("Finding new position for Cube " + ID);
            randomPosition = GetRandomPosition(cubeScale, ++depth, ID);
        }

        Physics.Raycast(randomPosition , Vector3.down, out hit, Mathf.Infinity);
        randomPosition.y = hit.point.y + cubeScale.y/2;
        

        
        return randomPosition;


    }
}
