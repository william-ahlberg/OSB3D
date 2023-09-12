using UnityEngine;

//Class used to trigger the elevator buttons
public class ElevatorButtonTrigger : MonoBehaviour
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
            int buttonNr = int.Parse(name);
            bool on = MoveElevator.ButtonPressed(buttonNr);

            if (on) buttonRenderer.material.color = Color.green;
            else buttonRenderer.material.color = Color.white; 
        }
    }
}
