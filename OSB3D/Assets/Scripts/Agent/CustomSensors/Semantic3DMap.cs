using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Semantic3DMap : ISensor
{
    private string sensorName;
    public Vector3[,,] gridMatrix;
    [Range(0f, 1f)]
    public float _inUp;
    private List<Vector3> gridCubes = new List<Vector3>();
    public List<string> Tags;
    public float _gridScale = 1f;
    public float _gridHeight = 1f;

    // Let developers specify which mask we want to use
    public LayerMask layersMask = ~0;

    [Range(0f, 3f)]
    public float offset_x = 0;
    [Range(0f, 3f)]
    public float offset_y = 0;
    [Range(0f, 3f)]
    public float offset_z = 0;

    [Range(0f, 2f)]
    public float _inFront = 0;
    public int _gridX = 5;
    public int _gridZ = 5;
    public int _gridY = 5;

    public GameObject agent;

    private Collider[] hitColliders;
    private GameObject[,,] detected;
    private float[,,] distances;

    public List<Color> _materials;
    public Color[,,] _debugMaterials;
    private float[] valueToReturn;

    public bool _debug;
    public bool _debugDist;

    private ObservationSpec observationSpec;


    void Awake()
    {
        
    }

    public Semantic3DMap(string sensorName, float _inUp, List<string> Tags, float _gridScale, float _gridHeight, LayerMask layersMask, float offset_x, float offset_y, float offset_z, float _inFront, int _gridX, int _gridY, int _gridZ, GameObject agent, bool _debug)
    {
        this._inUp = _inUp;
        this.Tags = Tags;
        this._gridScale = _gridScale;
        this._gridHeight = _gridHeight;
        // TODO Set this via config in Python
        this.layersMask = ~0;
        this.offset_x = offset_x;
        this.offset_y = offset_y;
        this.offset_z = offset_z;
        this._inFront = _inFront;
        this._gridX = _gridX;
        this._gridY = _gridY;
        this._gridZ = _gridZ;
        this.agent = agent;
        // This is ok to be always true, for now
        this._debug = true;
        this.sensorName = sensorName;

        createLocalGrid();
        _materials = new List<Color>();
        
        // Create random colors for debug
        // The first color is white with no alpha
        _materials.Add(new Color(1f, 1f, 1f, 0.0f));
        for(int i = 1; i < 10; i++)
        {
            Color randomColor = new Color(
                UnityEngine.Random.Range(0f, 1f), 
                UnityEngine.Random.Range(0f, 1f), 
                UnityEngine.Random.Range(0f, 1f),
                UnityEngine.Random.Range(0.25f, 0.25f)
            );
            _materials.Add(randomColor);
            // _materials.Add(UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.25f, 0.25f));
        }
    }

    public void Update()
    {

    }

    public string GetName()
    {
        return "SemanticMap";
    }

    public void Reset()
    {

    }

    public ObservationSpec GetObservationSpec()
    {
        return observationSpec;
    }

    public virtual byte[] GetCompressedObservation()
    {
        return null;
    }

    public CompressionSpec GetCompressionSpec()
    {
        return CompressionSpec.Default();
    }

    public int Write(ObservationWriter writer)
    {
        
        List<float> mapObservations = getDetectedObject();
        writer.AddList(mapObservations);
        
        return mapObservations.Count;
    }

    // Create local grid
    public void createLocalGrid()
    {
        gridMatrix = new Vector3[_gridX, _gridY, _gridZ];
        _debugMaterials = new Color[_gridX, _gridY, _gridZ];

        hitColliders = new Collider[5];
        detected = new GameObject[_gridX, _gridY, _gridZ];
        distances = new float[_gridX, _gridY, _gridZ];
        valueToReturn = new float[_gridX * _gridY * _gridZ];

        float inFront = _inFront;

        observationSpec = ObservationSpec.Visual(_gridX, _gridY, _gridZ, ObservationType.Default);

        // Create gridPerception
        for (int i = -_gridX / 2; i < _gridX / 2 + 1; i++)
        {
            for (int j = -_gridY / 2; j < _gridY / 2 + 1; j++)
            {
                for (int z = -_gridZ / 2; z < _gridZ / 2 + 1; z++)
                {
                    // Instantiate the cube and add it to the grid
                    var newCube = Vector3.zero;

                    // Spacing the cubes
                    float _localOffset_i = 0;
                    float _localOffset_j = 0;
                    float _localOffset_z = 0;

                    if (i < 0)
                    {
                        _localOffset_i = -offset_x;
                    }
                    else if (i > 0)
                    {
                        _localOffset_i = offset_x;
                    }

                    if (j < 0)
                    {
                        _localOffset_j = -offset_y;
                    }
                    else if (j > 0)
                    {
                        _localOffset_j = offset_y;
                    }

                    if (z < 0)
                    {
                        _localOffset_z = -offset_z;
                    }
                    else if (z > 0)
                    {
                        _localOffset_z = offset_z;
                    }

                    newCube = new Vector3(
                        i * _gridScale + _localOffset_i * Math.Abs(i),
                        j * _gridScale + _localOffset_j * Math.Abs(j) + _inUp,
                        z * _gridScale + _localOffset_z * Math.Abs(z) + inFront * (_gridScale + _gridX / 2 + (offset_x * _gridX / 2)));

                    // Add to list
                    gridCubes.Add(newCube);
                }

            }
        }

        for (int i = 0; i < _gridX; i++)
        {
            for (int z = 0; z < _gridZ; z++)
            {
                for (int j = 0; j < _gridY; j++)
                {
                    gridMatrix[i, j, z] = gridCubes[0];
                    gridCubes.RemoveAt(0);
                }
            }
        }

        for (int i = 0; i < _gridX; i++)
            for (int j = 0; j < _gridY; j++)
                for (int t = 0; t < _gridZ; t++)
                {
                    Vector3 cubePosition = gridMatrix[i, j, t];
                    distances[i, j, t] = Vector3.Distance(agent.transform.position, agent.transform.TransformPoint(cubePosition));
                }
    }

    public List<float> getDetectedObject()
    {
        List<float> values = new List<float>();
        
        int finalMask = ~0;

        for (int i = 0; i < _gridX; i++)
            for (int j = 0; j < _gridY; j++)
                for (int t = 0; t < _gridZ; t++)
                {

                    Vector3 cubePosition = gridMatrix[i, j, t];

                    int numColliders =
                    Physics.OverlapBoxNonAlloc(agent.transform.TransformPoint(cubePosition),
                        new Vector3(_gridScale / 2, _gridScale / 2, _gridScale / 2), hitColliders, agent.transform.rotation, finalMask);

                    if (numColliders > 0)                    
                    {
                        Collider c = parseLayerCollider(hitColliders, numColliders);
                        
                        if(c == null)
                        {
                            if (_debug)
                            {
                                _debugMaterials[i, j, t] = _materials[0];
                            }
                            values.Add(TagToInt(null));
                            continue;
                        }

                        GameObject go = c.gameObject;
                        values.Add(TagToInt(go));
                        float dist = distances[i, j, t];

                        if (_debug)
                        {
                            _debugMaterials[i, j, t] = _materials[(int)TagToInt(go)];
                        }
                    }
                    else
                    {
                        if (_debug)
                        {
                            _debugMaterials[i, j, t] = _materials[0];
                        }
                        values.Add(TagToInt(null));
                    }
                }

        return values;
    }

    private Color convertToRgb(float minval, float maxval, float val)
    {
        List<Color> colors = new List<Color>();
        colors.Add(new Color(150, 0, 0));
        colors.Add(new Color(255, 255, 0));
        colors.Add(new Color(255, 255, 255));

        float i_f = ((float)(val - minval)/(float)(maxval-minval)) * (colors.Count - 1);
        int i = (int) (i_f / 1);
        float f = i_f % 1;

        if (f<1e-15)
        {
            return colors[i];
        }
        else
        {
            Color color1 = colors[i];
            Color color2 = colors[i+1];
            return new Color(color1.r + f*(color2.r - color1.r), color1.g + f*(color2.g - color1.g), color1.b + f*(color2.b - color1.b));
        }

    }

    private Collider parseLayerCollider(Collider[] colliders, int numColliders)
    {
        
        GameObject go = colliders[0].gameObject;
        Collider c = colliders[0];
        int startI = 1;
        int currentLayerValue = go.layer;

        bool otherAgent = true;
        while(otherAgent)
        {
            if(go.CompareTag("Agent") && !GameObject.ReferenceEquals(go, agent))
            {
                startI ++;
                if(startI - 1 >= numColliders)
                {
                    c = null;
                    return c;
                }

                go = colliders[startI - 1].gameObject;
                c = colliders[startI - 1];
                currentLayerValue = go.layer;
            }
            else
            {
                otherAgent = false;
            }
        }

        for(int i = startI; i < numColliders; i++)
        {
            if(colliders[i] == null)
            {
                break;
            }

            if(colliders[i].gameObject.CompareTag("Agent") && !GameObject.ReferenceEquals(colliders[i].gameObject, agent))
            {
                continue;
            }

            int layerValue = colliders[i].gameObject.layer;
            if(layerValue < currentLayerValue)
            {
                currentLayerValue = layerValue;
                go = colliders[i].gameObject;
                c = colliders[i];
            }
        }

        return c;
    }

    public float TagToInt(GameObject go)
    {
        if(go == null)
            return 0f;

        for(int i = 0; i < Tags.Count; i++)
        {
            if(go.CompareTag(Tags[i]))
            {
                return i + 1;
            }
        }
        return 0f;
    }
}
