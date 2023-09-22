 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugBase : MonoBehaviour
{
    public Vector3 position;
    public Vector3 scale;
    public Bounds bounds;
    public int id;
    public string type; 
    public int stateDepth;
    [SerializeField] public bool isActive;

    public string searchTag = "Bug";
    public BugManager bugManager;

    /*public BugBase(int id, string type, Bounds bounds)
    {
        _id = id;
        _type = type;
        _bounds = bounds;
        _position = PlaceBugArea(0);
        Debug.Log(transform.position);
        //_scale = transform.localScale;
        _isActive = true;
        //CalcBounds();
    }*/
    public virtual void Start()
    {
        bugManager = GameObject.Find("GameInstance").GetComponent<BugManager>();
        bounds = bugManager.bounds;
        scale = transform.localScale;

        transform.position = PlaceBugArea(0);

    }

    private void Update()
    {


    
    
    }  

    protected virtual void BugBehavior()
    {
        
    
    }

    protected virtual void ToggleBugVisual()
    {
        GetComponent<Renderer>().enabled = !GetComponent<Renderer>().enabled;
    }

    protected virtual Vector3 PlaceBugArea(int depth)
    {
        RaycastHit hit;
        position = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), Random.Range(bounds.min.z, bounds.max.z));
        Collider[] freeColliders = Physics.OverlapBox(position, scale / 2);

        bool validPosition = true;

        foreach (Collider collider in freeColliders)
        {
            if (!collider.gameObject.CompareTag(searchTag))
            {
                validPosition = false;
                break;
            }
        }

        if (!validPosition && (depth < 100))
        {
            //Debug.Log("Finding new position for Cube " + ID);
            position = PlaceBugArea(++depth);
        }

        Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity);
        position.y = hit.point.y + scale.y / 2;

        return position;

    }

    



}
