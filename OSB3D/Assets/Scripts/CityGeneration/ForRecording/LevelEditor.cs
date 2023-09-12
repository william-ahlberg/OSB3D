using UnityEngine;
using UnityEditor;


//Class only used to be able to generate cities from the editor. Used for recording purposes and can be removed for release. 
public class LevelEditor : Editor
{
    [CustomEditor(typeof(LevelController))]
    public class RecordEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LevelController levelController = (LevelController)target;

            if (DrawDefaultInspector())
            {
                Debug.Log("Value changed");
            }

            if (GUILayout.Button("Generate"))
            {
                Debug.Log("Button Clicked");
                levelController.ReGenerate();
            }

        }
    }
}