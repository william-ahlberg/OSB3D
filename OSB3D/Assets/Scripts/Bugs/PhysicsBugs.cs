using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBugs : MonoBehaviour
{
    private Bounds gameAreaBounds;
    [SerializeField]
    private int numberOfBugs = 5;

    private Transform parent;
    private GameObject newParent;
    
    // Start is called before the first frame update
    void Start()
    {
        CalculateGameAreaBounds();
        MakeCubes(100);


      

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
    
    void Update()
    {

    }

    void MakeCubes(int numberOfCubes)
    {
        GameObject parentObject = new GameObject("CubeParent");
        parentObject.AddComponent<CubeIntersection>();
        for (int i = 0; i < numberOfCubes; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = GetRandomPosition();
            
            cube.transform.localScale = new Vector3(2f, 2f, 2f);
            
            cube.AddComponent<CubeIntersection>();
            cube.GetComponent<Collider>().isTrigger = true;
            cube.transform.parent = parentObject.transform;
        }
    }

    Vector3 GetRandomPosition()
    {
        Vector3 randomPosition = new Vector3(
            Random.Range(GetBounds().min.x, GetBounds().max.x),
            4f,
            Random.Range(GetBounds().min.z, GetBounds().max.z));

        Collider[] freeColliders = Physics.OverlapSphere(randomPosition, 2f);
        foreach (var collider in freeColliders)
        {
            if (collider.gameObject != this.gameObject)
            {
                randomPosition = GetRandomPosition();
                return randomPosition;


            }
        
        }
        return randomPosition;


    }
}
