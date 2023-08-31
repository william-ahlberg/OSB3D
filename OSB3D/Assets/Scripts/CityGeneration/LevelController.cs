//using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class LevelController : MonoBehaviour
{
    [Header("City Setup")]
    [SerializeField] int seed;
    [SerializeField] int blockCountX;
    [SerializeField] int blockCountZ;
    [Range(0.0f, 1.0f)]
    [SerializeField] float parkRatio;

    [Header("Vertical connection")]


    [Header("Car Generation")]
    [SerializeField] bool parkedPosition;
    [SerializeField] int carMin;
    [SerializeField] int carMax;
    [SerializeField] int maxAttempts;

    [Header("Referenced Prefabs")]
    [SerializeField] GameObject road;
    [SerializeField] GameObject crossing;

    [SerializeField] GameObject edgeBlock;
    [SerializeField] GameObject edgePark;
    [SerializeField] GameObject edgeRoad;

    List<GameObject> parks;
    List<GameObject> cars;

    BlockGenerator blockGenerator;

    void Start()
    {
        //fixed numbers, the size of the 3d-assets
        float blockSize = 105;
        float roadWidth = 20;

        parks = Utility.FromDirectory("Prefabs/Parks");
        cars = Utility.FromDirectory("Prefabs/Cars");

        blockGenerator = GetComponent<BlockGenerator>();
        GenerateCity(blockSize, roadWidth);
    }

    void Update()
    {
        /* if (Input.GetKeyDown(KeyCode.V))
         {
             ScreenCapture.CaptureScreenshot("unityscreenshot" + System.DateTime.Now.ToString("hhmmss") + ".png", 4);
             Debug.Log("A screenshot was taken!");
         }*/
    }

    //Main method to generate city
    void GenerateCity(float _blockSize, float _roadWidth)
    {
        Random.InitState(seed);

        int totalBlocks = blockCountX * blockCountZ;
        int nrParks = (int) parkRatio * totalBlocks;

        List<int> parkBlocks = RandomParkBlocks(nrParks, totalBlocks);

        int elevatorBlock = RandomElevatorBlock(totalBlocks, parkBlocks);

        int blockCounter = 0; 

        //adds to count for roads and crossings
        int loopX = blockCountX * 2 - 1;
        int loopZ = blockCountZ * 2 - 1;

        //how much position should be moved (in X/Z) for every placement
        float addToPos = _blockSize / 2 + _roadWidth / 2;

        //start placing at x=0, z=0 with the rotation of 0
        float currentPosX = 0;
        float currentPosZ = 0;
        float yRotation = 0;

        int roadNr = 0;

        for (int i = 0; i < loopX; i++)
        {
            for (int j = 0; j < loopZ; j++)
            {
                GameObject newInstance;

                //if both even, instatiate a block or park
                if ((i % 2 == 0) && (j % 2 == 0))
                {
                    bool isPark = false;
                    if (parkBlocks.Contains(blockCounter)) isPark = true;

                    bool addElevator = false;
                    if (elevatorBlock == blockCounter) addElevator = true;

                    float rotateBlock = Random.Range(0, 4);
                    rotateBlock *= 90;

                    List<System.Tuple<int, float, int>> edges = CheckEdgeConditions(i, j, loopX, loopZ, rotateBlock);

                    Vector3 blockPos = new(currentPosX, 0, currentPosZ);

                    GameObject edgeType;

                    if (isPark)
                    {
                        newInstance = AddPark(blockPos, rotateBlock);
                        edgeType = edgePark;
                    }

                    else
                    {
                        newInstance = AddBlock(edges, blockPos, rotateBlock, addElevator);
                        edgeType = edgeBlock;
                    }

                    if(edges.Count > 0) AddEdges(newInstance, edges, edgeType, blockPos);

                    blockCounter++;
                }

                //if both uneven, instantiate a crossing
                else if ((i % 2 != 0) && (j % 2 != 0))
                {
                    newInstance = Instantiate(crossing, new Vector3(currentPosX + _roadWidth / 2, 0, currentPosZ + _roadWidth / 2), Quaternion.identity);
                }

                //otherwise, instantiate a road
                else
                {
                    newInstance = GenerateRoad(roadNr);
                    Vector3 roadPosition = new(currentPosX, 0, currentPosZ);
                    newInstance.transform.Translate(roadPosition);
                    newInstance.transform.Rotate(0, yRotation, 0, Space.World);
                    roadNr++;

                    if (i == 0 || j == 0 || i == loopX - 1 || j == loopZ - 1)
                    {
                        GameObject edgeObject = RoadEdge(i, j, loopX, loopZ, roadPosition);
                        edgeObject.transform.parent = newInstance.transform;
                    }
                }

                //placed all instances in the active game object
                newInstance.transform.parent = this.transform;

                currentPosZ += addToPos;
            }

            currentPosX += addToPos;
            currentPosZ = 0;

            //changes rotation every new x loop
            if (yRotation == 0) yRotation = 90;
            else yRotation = 0;
        }
    }

    List<int> RandomParkBlocks(int _nrParks, int _totalBlocks)
    {
        List<int> parkBlocks = new();
        int counter = 0;
        bool added = false;

        for (int i = 0; i < _nrParks; i++)
        {
            while (counter < 1000 && !added)
            {
                int index = Random.Range(0, _totalBlocks);
                if (!parkBlocks.Contains(index))
                {
                    parkBlocks.Add(index);
                    added = true;
                }
                counter++;
            }
        }
        return parkBlocks; ;
    }

    int RandomElevatorBlock(int _totalBlocks, List<int> _parkBlocks)
    {
        int chosenIndex = 0;
        int counter = 0;
        bool added = false;

        while (counter < 100 && !added)
        {
            int index = Random.Range(0, _totalBlocks);

            if (!_parkBlocks.Contains(index))
            {
                chosenIndex = index;
                added = true;
            }
            counter++;
        }

        return chosenIndex;
    }

    List<System.Tuple<int, float, int>> CheckEdgeConditions(int _i, int _j, int _loopX, int _loopZ, float _rotateBlock)
    {
        List<System.Tuple<int, float, int>> edges = new();
        int blockedEdge;

        if (_j == 0)
        {
            blockedEdge = EdgeBlocked(3, _rotateBlock);
            edges.Add(new System.Tuple<int, float, int>(3, -90, blockedEdge));
        }
        if (_j == _loopZ - 1)
        {
            blockedEdge = EdgeBlocked(1, _rotateBlock);
            edges.Add(new System.Tuple<int, float, int>(1, 90, blockedEdge));
        }
        if (_i == 0)
        {
            blockedEdge = EdgeBlocked(0, _rotateBlock);
            edges.Add(new System.Tuple<int, float, int>(0, 0, blockedEdge));
        }

        if (_i == _loopX - 1)
        {
            blockedEdge = EdgeBlocked(2, _rotateBlock);
            edges.Add(new System.Tuple<int, float, int>(2, 180, blockedEdge));
        }

        return edges; 
    }

    int EdgeBlocked(int _startEdge, float _rotation)
    {
        int blockEdge;

        if (_rotation == 270) blockEdge = (_startEdge + 1) % 4;
        else if (_rotation == 180) blockEdge = (_startEdge + 2) % 4;
        else if (_rotation == 90) blockEdge = (_startEdge + 3) % 4;
        else blockEdge = _startEdge;

        return blockEdge;
    }

    GameObject AddPark(Vector3 _blockPos, float _rotateBlock)
    {
        GameObject gameObject;
        int choosePark = Random.Range(0, parks.Count);
        gameObject = Instantiate(parks[choosePark]);
        gameObject.transform.Translate(_blockPos);
        gameObject.transform.Rotate(0, _rotateBlock, 0, Space.World);
        return gameObject;
    }

    GameObject AddBlock(List<System.Tuple<int, float, int>> _edges, Vector3 _blockPos, float _rotateBlock, bool _addElevator)
    {
        GameObject gameObject;

        gameObject = blockGenerator.GenerateBlock(_edges, _addElevator);
        gameObject.transform.Translate(_blockPos);
        gameObject.transform.Rotate(0, _rotateBlock, 0, Space.World);

        if (_addElevator) GameObject.Find("Elevator(Clone)").GetComponent<MoveElevator>().SetMaxPosition();

        return gameObject;
    }

    GameObject AddEdges(GameObject _gameObject, List<System.Tuple<int, float, int>> _edges, GameObject _edgeType, Vector3 _blockPos)
    {
        for (int k = 0; k < _edges.Count; k++)
        {
            GameObject edgeObject = Instantiate(_edgeType);
            edgeObject.transform.Translate(_blockPos);
            edgeObject.transform.Rotate(0, _edges[k].Item2, 0, Space.World);
            edgeObject.transform.parent = gameObject.transform;
        }

        return _gameObject;
    }

    GameObject GenerateRoad(int _roadnr)
    {
        string roadName = "Road" + _roadnr.ToString();
        GameObject roadObject = new(roadName);

        GameObject newRoad = Instantiate(road);
        newRoad.transform.parent = roadObject.transform;

        int carNr = Random.Range(carMin, carMax);

        List<Vector3> carPositions = CarPositions(carNr);

        for (int k = 0; k < carPositions.Count; k++)
        {
            float carRot;
            if (carPositions[k].z > 0) carRot = -90;
            else carRot = 90;

            int carIndex = Random.Range(0, cars.Count);
            GameObject newCar = Instantiate(cars[carIndex], new Vector3(carPositions[k].x, 0, carPositions[k].z), Quaternion.identity);
            newCar.transform.Rotate(0, carRot, 0, Space.World);
            newCar.transform.parent = roadObject.transform;
        }

        return roadObject;
    }

    List<Vector3> CarPositions(int _carNr)
    {

        List<Vector3> carsDir1 = new();
        List<Vector3> carsDir2 = new();

        float carZ;
        if (parkedPosition) carZ = 4.8f;
        else carZ = 3.0f;

        for (var i = 0; i < _carNr; i++)
        {
            int carDirection = Random.Range(0, 2);

            if (carDirection == 0)
            {
                carsDir1 = FindCarPosition(carsDir1, carZ * -1.0f);
            }

            else
            {
                carsDir1 = FindCarPosition(carsDir1, carZ);
            }
        }

        carsDir1.AddRange(carsDir2);
        return carsDir1;
    }

    List<Vector3> FindCarPosition(List<Vector3> _carPositions, float _carZ)
    {
        float minX = -46.0f;
        float maxX = 46.0f;
        int attempts = 0;
        bool placed = false;

        Vector3 carPos;

        while (attempts < maxAttempts && !placed)
        {

            carPos = new Vector3(Random.Range(minX, maxX), 0, _carZ);

            if (!_carPositions.Any(p => Vector3.Distance(carPos, p) < 7.5f))
            {
                _carPositions.Add(carPos);
                placed = true;
            }

            attempts++;
        }

        return _carPositions;
    }

    GameObject RoadEdge(int _i, int _j, int _loopX, int _loopZ, Vector3 _roadPosition)
    {
        float edgeRotation = 0;

        if (_j == 0) edgeRotation = -90;
        else if (_j == _loopZ - 1) edgeRotation = 90;
        else if (_i == _loopX - 1) edgeRotation = 180;

        GameObject edge = Instantiate(edgeRoad);
        edge.transform.Translate(_roadPosition);
        edge.transform.Rotate(0, edgeRotation, 0, Space.World);

        return edge;
    }
}
