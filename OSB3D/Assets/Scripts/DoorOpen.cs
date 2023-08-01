using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DoorOpen : MonoBehaviour
{
    [SerializeField] Animator doorAnimator;
    [SerializeField] bool openTrigger;

    void OnTriggerEnter(Collider _collider) 
    {
        

        if(_collider.gameObject.tag == "Player")
        {
            

            bool opened = doorAnimator.GetBool("Opened");


            if (!opened && openTrigger)
            {
                Debug.Log("opening");

                doorAnimator.SetTrigger("TriggerOpen");
                doorAnimator.SetBool("Opened", true);
            }

            else if(opened && !openTrigger)
            {
                Debug.Log("closing");

                doorAnimator.SetTrigger("TriggerClose");
                doorAnimator.SetBool("Opened", false);
            }
        }
    }
}
