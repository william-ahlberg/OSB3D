using System.Collections.Generic;
using UnityEngine;

//struct with the information for each building
public struct Building
{
    private int typeIndex;
    private Vector3 position;
    private float yRotation;
    private bool passage;
    private List<int> edges;

    public Building(int _typeIndex, Vector3 _Position, float _yRotation, bool _passage, List<int> _onEdges)
    {
        this.typeIndex = _typeIndex;
        this.position = _Position;
        this.yRotation = _yRotation;
        this.passage = _passage;
        this.edges = _onEdges;
    }

    public int TypeIndex { get { return this.typeIndex; } }
    public Vector3 Position { get { return this.position; } }
    public float Rotation { get { return this.yRotation; } }
    public bool IsPassage { get { return this.passage; } }

    public List<int> Edges { get { return this.edges; } }
}
