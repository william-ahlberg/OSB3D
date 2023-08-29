using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DoorOpen : MonoBehaviour
{
    [SerializeField] Animator[] doorAnimator;
    [SerializeField] Material buttonMaterial;

    private void Start()
    {
        buttonMaterial.color = Color.red;
    }

    void OnTriggerEnter(Collider _collider) 
    {
        if(_collider.gameObject.tag == "Player")
       {
            
            bool opened = doorAnimator[0].GetBool("Opened");


            if (!opened)
            {
                for (int i = 0; i < doorAnimator.Length; i++)
                {
                    doorAnimator[i].SetTrigger("TriggerOpen");
                    doorAnimator[i].SetBool("Opened", true);
                }
                buttonMaterial.color = Color.green; 
            }

            else if(opened)
            {
                for(int i = 0;i < doorAnimator.Length; i++)
                {
                    doorAnimator[i].SetTrigger("TriggerClose");
                    doorAnimator[i].SetBool("Opened", false);
                }
                buttonMaterial.color = Color.red;
        }
        }
    }
}
