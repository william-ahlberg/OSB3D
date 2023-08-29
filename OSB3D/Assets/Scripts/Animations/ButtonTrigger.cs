using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    [SerializeField] Renderer buttonRenderer;

    private void Start()
    {
        buttonRenderer.material.color = Color.white;
    }

    void OnTriggerEnter(Collider _collider)
    {
        if(_collider.CompareTag("Player"))
        {
            string name = this.name;
            Debug.Log(name);
            int buttonNr = int.Parse(name);
            bool on = MoveElevator.ButtonPressed(buttonNr);

            if (on) buttonRenderer.material.color = Color.green;
            else buttonRenderer.material.color = Color.white; 
        }
    }
}
