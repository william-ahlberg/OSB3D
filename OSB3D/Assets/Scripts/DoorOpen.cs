using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DoorOpen : MonoBehaviour
{
    [SerializeField] Animator doorAnimator;
    [SerializeField] Material buttonMaterial;

    private void Start()
    {
        buttonMaterial.color = Color.red;
    }

    void OnTriggerEnter(Collider _collider) 
    {
        if(_collider.gameObject.tag == "Player")
       {
            
            bool opened = doorAnimator.GetBool("Opened");


            if (!opened)
            {
                doorAnimator.SetTrigger("TriggerOpen");
                doorAnimator.SetBool("Opened", true);
                buttonMaterial.color = Color.green; 
            }

            else if(opened)
            {
                doorAnimator.SetTrigger("TriggerClose");
                doorAnimator.SetBool("Opened", false);
                buttonMaterial.color = Color.red;
        }
        }
    }
}
