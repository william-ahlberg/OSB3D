using System.Collections.Generic;
using UnityEngine;

//struct with the information for each building
public struct Building
{
    private readonly bool terrain; 
    private readonly int typeIndex;
    private readonly Vector3 position;
    private readonly float yRotation;
    private readonly bool passage;
    private readonly List<int> edges;

    public Building(int _typeIndex, Vector3 _Position, float _yRotation, bool _passage, List<int> _onEdges, bool _terrain)
    {
        this.typeIndex = _typeIndex;
        this.position = _Position;
        this.yRotation = _yRotation;
        this.passage = _passage;
        this.edges = _onEdges;
        this.terrain = _terrain;
    }

    public readonly int TypeIndex { get { return this.typeIndex; } }
    public readonly Vector3 Position { get { return this.position; } }
    public readonly float Rotation { get { return this.yRotation; } }
    public readonly bool IsPassage { get { return this.passage; } }
    public readonly List<int> Edges { get { return this.edges; } }
    public readonly bool Terrain { get { return this.terrain; } }
}
