#if (UNITY_EDITOR)

using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ParkGenerator))]
public class ParkEditor : Editor
{

    public override void OnInspectorGUI()
    {
        ParkGenerator parkGenerator = (ParkGenerator)target;

        if (DrawDefaultInspector())
        {
            if (parkGenerator.autoUpdate)
            {
                parkGenerator.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            parkGenerator.GenerateMap();
        }
    }
}

#endif