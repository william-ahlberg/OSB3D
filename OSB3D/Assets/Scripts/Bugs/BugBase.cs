using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class BugBase : MonoBehaviour
{
    public Vector3 position;
    public Vector3 scale;
    public int id;
    
    public string bugClass;
    public string bugType; 
    public bool isActive;

    public virtual void Start()
    {
        scale = transform.localScale;
        bugClass = this.GetType().Name;
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

    /*protected virtual Vector3 PlaceBugArea(int depth)
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

    }*/


}

[System.Serializable]
public class BugList
{
    public List<BugBase> bugs;
}
