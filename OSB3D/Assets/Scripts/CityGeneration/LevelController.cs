//using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] Material testMaterial;

    [Header("City Setup")]
    [SerializeField] int seed;
    [SerializeField] int blockCountX;
    [SerializeField] int blockCountZ;
    [Range(0.0f, 1.0f)]
    [SerializeField] float parkRatio;

    [Header("Street Items per road segment")]
    [SerializeField] int itemMin;
    [SerializeField] int itemMax;

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

    GameObject city; 
    List<GameObject> parks;
    List<GameObject> cars;
    List<GameObject> streetItems;
    List<Material> carMaterials;
    private List<float> lightAndTreePositions; 

    BlockGenerator blockGenerator;

    private float blockSize, roadWidth; 

    void Start()
    {
        /*Fixed numbers, the size of the 3d-assets.
        Should only be changed if another size blocks and roads are used than the once included*/
        blockSize = 105;
        roadWidth = 20;

        parks = Utility.FromDirectory("Prefabs/Parks");
        cars = Utility.FromDirectory("Prefabs/Cars");
        streetItems = Utility.FromDirectory("Prefabs/StreetItems");
        carMaterials = Utility.LoadMaterials("Cars");

        lightAndTreePositions = new List<float>() { -45, -30, -15, 0, 15, 30, 45}; 

        blockGenerator = GetComponent<BlockGenerator>();

        city = new GameObject();
        GenerateCity(blockSize, roadWidth);
    }

    //Update() only used for screenshots. Can be removed before distribution. 
    void Update()
    {
        /* if (Input.GetKeyDown(KeyCode.V))
         {
             ScreenCapture.CaptureScreenshot("unityscreenshot" + System.DateTime.Now.ToString("hhmmss") + ".png", 4);
             Debug.Log("A screenshot was taken!");
         }*/
    }


    //ReGenerate() is for recording purpose only. Can be removed before distribution. 
    public void ReGenerate()
    {
        Destroy(city);
        GenerateCity(105, 20); 
    }

    //Main method to generate city
    void GenerateCity(float _blockSize, float _roadWidth)
    {
        //Grouped in a Game Object so it can be destroyed if regenerated for recordings
        city = new GameObject("City");
        city.transform.parent = this.transform;

        Random.InitState(seed);

        int totalBlocks = blockCountX * blockCountZ;
        int nrParks = (int) (parkRatio * totalBlocks);

        //A list of indices that are going to be parks
        List<int> parkBlocks = RandomParkBlocks(nrParks, totalBlocks);

        //Building block index for where to place the elevator
        int elevatorBlock = RandomElevatorBlock(totalBlocks, parkBlocks);

        int blockCounter = 0; 

        //adds to loop count for roads and crossings
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

                //If both even, instatiate a block or park
                if ((i % 2 == 0) && (j % 2 == 0))
                {
                    bool addElevator = false;
                    if (elevatorBlock == blockCounter) addElevator = true;

                    float rotateBlock = Random.Range(0, 4);
                    rotateBlock *= 90;

                    //A list which sides are at the edge and therefore not facing another block (if any)
                    List<System.Tuple<int, float, int>> edges = CheckEdgeConditions(i, j, loopX, loopZ, rotateBlock);

                    Vector3 blockPos = new(currentPosX, 0, currentPosZ);

                    GameObject edgeType;

                    if (parkBlocks.Contains(blockCounter))
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

                //placed all instances in the a game object
                //newInstance.transform.parent = this.transform;
                newInstance.transform.parent = city.transform;

                currentPosZ += addToPos;
            }

            currentPosX += addToPos;
            currentPosZ = 0;

            //changes rotation every new x loop
            if (yRotation == 0) yRotation = 90;
            else yRotation = 0;
        }
    }

    //Returns a list of indecies for where to place parks
    List<int> RandomParkBlocks(int _nrParks, int _totalBlocks)
    {
        List<int> parkBlocks = new();
        int counter = 0;
        bool added;

        for (int i = 0; i < _nrParks; i++)
        {
            added = false;

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

    //Returns a index for in which building block the elevator should be placed
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

    /* CheckEdgeConditions() checks if this block is at the edge of the city and returns a list with which sides are towards the edge. 
     * Tuple<int,float,int> are for:        
        int = index of side towards edge
        float = rotation used for edgeWall
        int = index of side towards edge after the block have been rotated.*/

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

    //Checks which side will be facing the edge after the block have been rotated
    int EdgeBlocked(int _startEdge, float _rotation)
    {
        int blockEdge;

        if (_rotation == 270) blockEdge = (_startEdge + 1) % 4;
        else if (_rotation == 180) blockEdge = (_startEdge + 2) % 4;
        else if (_rotation == 90) blockEdge = (_startEdge + 3) % 4;
        else blockEdge = _startEdge;

        return blockEdge;
    }

    //Adds a new park block
    GameObject AddPark(Vector3 _blockPos, float _rotateBlock)
    {
        GameObject gameObject;
        int choosePark = Random.Range(0, parks.Count);
        gameObject = Instantiate(parks[choosePark]);
        gameObject.transform.Translate(_blockPos);
        gameObject.transform.Rotate(0, _rotateBlock, 0, Space.World);
        return gameObject;
    }

    //Adds a new building block
    GameObject AddBlock(List<System.Tuple<int, float, int>> _edges, Vector3 _blockPos, float _rotateBlock, bool _addElevator)
    {
        GameObject gameObject;

        gameObject = blockGenerator.GenerateBlock(_edges, _addElevator);
        gameObject.transform.Translate(_blockPos);
        gameObject.transform.Rotate(0, _rotateBlock, 0, Space.World);

        if (_addElevator) GameObject.Find("Elevator(Clone)").GetComponent<MoveElevator>().SetMaxPosition();

        return gameObject;
    }

    //Adds the edge walls
    GameObject AddEdges(GameObject _gameObject, List<System.Tuple<int, float, int>> _edges, GameObject _edgeType, Vector3 _blockPos)
    {
        for (int k = 0; k < _edges.Count; k++)
        {
            GameObject edgeObject = Instantiate(_edgeType);
            edgeObject.transform.Translate(_blockPos);
            edgeObject.transform.Rotate(0, _edges[k].Item2, 0, Space.World);
            //edgeObject.transform.parent = gameObject.transform;
            edgeObject.transform.parent = city.transform;
        }

        return _gameObject;
    }

    //Generates a road object, including randomly placed cars and street items
    GameObject GenerateRoad(int _roadnr)
    {
        string roadName = "Road" + _roadnr.ToString();
        GameObject roadObject = new(roadName);

        GameObject newRoad = Instantiate(road);
        newRoad.transform.parent = roadObject.transform;

        List<Vector3> carPositions = PlacingPositions(true);
        PlaceItems(roadObject, carPositions, true);

        List<Vector3> itemPositions = PlacingPositions(false);
        PlaceItems(roadObject, itemPositions, false);

        return roadObject;
    }

    //Returns a list of car positions
    List<Vector3> PlacingPositions(bool _cars)
    {
        int nrPositions; 

        List<Vector3> direction0 = new();
        List<Vector3> direction1 = new();

        float zPosition;
        float threshold; 

        if (_cars)
        {
            nrPositions = Random.Range(carMin, carMax);

            /*carZ postion is set based on the width of the road segment. 
            Width the road segments included 0.24*roadWidth is next to the curb (parked position) 
            and 0.15*road width is in the middle of a road (driving position) */
            if (parkedPosition) zPosition = roadWidth * 0.24f;
            else zPosition = roadWidth * 0.15f;

            //Threshold of 7.5f ensure no overlap of cars
            threshold = 7.5f;
        }

        else
        {
            nrPositions = Random.Range(itemMin, itemMax);

            for (int i = 0; i < lightAndTreePositions.Count; i++)
            {
                direction0.Add(new Vector3(lightAndTreePositions[i], 0, -7));
                direction1.Add(new Vector3(lightAndTreePositions[i], 0, 7));
            }

            //Z position of 6.8 places street items on the edge of the curb towards the road, between the threes and street lights. 3.0 ensures no overlaps of street items or trees. 
            zPosition = 6.8f;
            threshold = 3.0f;
        }

            for (var i = 0; i < nrPositions; i++)
            {
                int direction = Random.Range(0, 2);

                if (direction == 0)
                {
                    direction0 = FindPosition(direction0, zPosition * -1.0f, threshold);
                }

                else
                {
                    direction1 = FindPosition(direction1, zPosition, threshold);
                }
            }
                
        if(!_cars)
        {
            direction0.RemoveRange(0, lightAndTreePositions.Count);
            direction1.RemoveRange(0, lightAndTreePositions.Count);   
        }

        direction0.AddRange(direction1);
        return direction0;
    }

    //Adds a new car position to the list of carPositions, if it succesfully fins a position within a max number of attempts
    List<Vector3> FindPosition(List<Vector3> _positions, float _zPosition, float _threshold)
    {
        Debug.Log("zPosition: " + _zPosition);
        float minX = -46.0f;
        float maxX = 46.0f;
        int attempts = 0;
        bool placed = false;

        Vector3 newPos;

        while (attempts < maxAttempts && !placed)
        {

            newPos = new Vector3(Random.Range(minX, maxX), 0, _zPosition);

            //Checks if the new postion is already in the list of cars within a threshold 
            if (!_positions.Any(p => Vector3.Distance(newPos, p) < _threshold))
            {
                _positions.Add(newPos);
                placed = true;
            }

            attempts++;
        }

        return _positions;
    }

    void PlaceItems(GameObject _roadObject, List<Vector3> _positions, bool _cars)
    {
        for (int i = 0; i < _positions.Count; i++)
        {
            float rotation;
            GameObject newItem;

            if (_cars)
            {
                //Checks where the car is placed and which direction it should be facing
                if (_positions[i].z > 0) rotation = -90;
                else rotation = 90;

                int carIndex = Random.Range(0, cars.Count);
                newItem = Instantiate(cars[carIndex], new Vector3(_positions[i].x, 0, _positions[i].z), Quaternion.identity);
            }

            else
            {
                if (_positions[i].z > 0) rotation = 180;
                else rotation = 0;

                int itemIndex = Random.Range(0, streetItems.Count);
                newItem = Instantiate(streetItems[itemIndex], new Vector3(_positions[i].x, 0, _positions[i].z), Quaternion.identity);
            }



            newItem.transform.Rotate(0, rotation, 0, Space.World);
            newItem.transform.parent = _roadObject.transform;

            if (_cars) SetCarMaterial(newItem);
        }
    }

    //Randomises a car material for the car body. The other materials on the cars are not changed. 
    void SetCarMaterial(GameObject _newCar)
    {
        GameObject carImport = _newCar.transform.GetChild(0).gameObject;
        GameObject carBody = carImport.transform.Find("CarBody").gameObject;
        MeshRenderer renderer = carBody.GetComponent<MeshRenderer>();
        Material[] prefabMaterials = renderer.materials;
        prefabMaterials[0] = carMaterials[Random.Range(0, carMaterials.Count)];
        renderer.materials = prefabMaterials;
    }

    //Instatiate the edge-object next to a road with the correct rotation
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
