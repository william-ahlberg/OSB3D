using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    [SerializeField] Renderer[] buttons;
    [SerializeField] Animator[] doorAnimator;
    [SerializeField] Material buttonGreen;
    [SerializeField] Material buttonRed;

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

                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i].material = buttonGreen;
                }
            }

            else if(opened)
            {
                for(int i = 0;i < doorAnimator.Length; i++)
                {
                    doorAnimator[i].SetTrigger("TriggerClose");
                    doorAnimator[i].SetBool("Opened", false);
                }

                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i].material = buttonRed;
                }
            }
        }
    }
}
