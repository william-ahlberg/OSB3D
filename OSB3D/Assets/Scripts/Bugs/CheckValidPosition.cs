using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckValidPosition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckPosition();
    }

    public bool CheckPosition()
    {

        RaycastHit hit;

        Collider[] freeColliders = Physics.OverlapBox(transform.position, transform.localScale/2);
        Debug.Log(freeColliders.Length);

        foreach (Collider collider in freeColliders)
        {Debug.Log(collider.tag);}
        if (freeColliders.Length > 1)
        {
            Debug.Log("Position invalid, colliders intersect");
            return false;
        }

        //if (Physics.Raycast(transform.position + Vector3.up * transform.localScale.y * 0.5f, Vector3.up, out hit, Mathf.Infinity))
        //{
        //    Debug.Log("Position invalid, roof over cube");
        //    return false;
        
        //}

        return true;
    }
}
