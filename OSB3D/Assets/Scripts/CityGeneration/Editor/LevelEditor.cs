#if (UNITY_EDITOR)

using UnityEngine;
using UnityEditor;


//Class used to be able to generate cities from the editor. Should not be included in game builds. 

[CustomEditor(typeof(LevelController))]

public class LevelEditor : Editor
{
        public override void OnInspectorGUI()
        {
            LevelController levelController = (LevelController)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Generate"))
            {
                Debug.Log("Generate");
                levelController.Setup();
            }
        }
}

#endif