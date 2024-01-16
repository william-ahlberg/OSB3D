using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Unity.MLAgents.Sensors;



public class Semantic3DMapComponent : SensorComponent
{
    public string sensorName = "Semantic3DMap";
    [Tooltip("The height of the center of the map to the ground")]
    [Range(0f, 1f)]
    public float _inUp;
    [Tooltip("The width of each cube in the map")]
    public float _gridScale = 1f;
    [Tooltip("The height of each cube in the map")]
    public float _gridHeight = 1f;

    // Let developers specify which mask we want to use
    [Tooltip("Which layers the map should recognize")]
    public LayerMask layersMask;
    [Tooltip("Which tags the map should recognize")]
    public List<string> Tags;

    [Tooltip("The distance of each cube in the x-axis")]
    [Range(0f, 3f)]
    public float offset_x = 0;
    [Tooltip("The distance of each cube in the y-axis")]
    [Range(0f, 3f)]
    public float offset_y = 0;
    [Tooltip("The distance of each cube in the z-axis")]
    [Range(0f, 3f)]
    public float offset_z = 0;

    [Tooltip("The distance of the center of the map to the point of view of the root")]
    [Range(0f, 2f)]
    public float _inFront = 0;
    [Tooltip("The number of cubes in the x-axis")]
    public int _gridX = 5;
    [Tooltip("The number of cubes in the y-axis")]
    public int _gridZ = 5;
    [Tooltip("The number of cubes in the z-axis")]
    public int _gridY = 5;

    [Tooltip("The root game object. NB! The map will rotate if the   rotates")]
    public GameObject root;

    private bool _debug = true;

    private Semantic3DMap _semanticMap;

    public override ISensor[] CreateSensors()
    {
        _semanticMap = new Semantic3DMap(sensorName, _inUp, Tags, _gridScale, _gridHeight, layersMask, offset_x, offset_y, offset_z, _inFront, _gridX, _gridY, _gridZ, root, _debug);

        return new ISensor[] { _semanticMap };
    }

    private void OnDrawGizmosSelected()
    {
        if(_debug && _semanticMap != null && _semanticMap.gridMatrix != null)
        {
            for (int i = 0; i < _gridX; i++)
                for (int j = 0; j < _gridY; j++)
                    for (int t = 0; t < _gridZ; t++)
                    {
                        if(_semanticMap.gridMatrix[i, j, t] != null)
                        {
                            Vector3 cubePosition = _semanticMap.gridMatrix[i, j, t];
                            Gizmos.color =_semanticMap._debugMaterials[i, j, t];
                            Gizmos.DrawCube(root.transform.TransformPoint(cubePosition), new Vector3(_gridScale, _gridScale, _gridScale));
                        }
                    }
        }
    }
}
