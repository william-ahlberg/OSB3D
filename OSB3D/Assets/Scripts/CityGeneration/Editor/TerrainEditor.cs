#if (UNITY_EDITOR)

using UnityEngine;
using UnityEditor;

//Class used to be able to generate terrain from the editor. Should not be included in game builds. 

[CustomEditor(typeof(BlockTerrain))]
public class TerrainEditor : Editor
{

    public override void OnInspectorGUI()
    {
        BlockTerrain blockEditor = (BlockTerrain)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
        {
            blockEditor.Generate(105);
        }
    }
}

#endif