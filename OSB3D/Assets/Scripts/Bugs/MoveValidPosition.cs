using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveValidPosition : MonoBehaviour
{
    CheckValidPosition checkValidPosition;
    // Start is called before the first frame update
    void Start()
    {
        checkValidPosition = GetComponent<CheckValidPosition>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!checkValidPosition.CheckPosition())
        {
            Vector3 randomPosition = new Vector3(
               Random.Range(-50, 50),
               Random.Range(0, 50),
               Random.Range(-50, 50));
            transform.position = randomPosition;
        }
        else
        { 
        Debug.Log(transform.position);
        
        }

    }
}
