using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.Globalization;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using static UnityEditor.Progress;
using Unity.VisualScripting.Antlr3.Runtime;

public class LevelController : MonoBehaviour
{
    [Header("City Setup")]
    [SerializeField] int seed;
    [SerializeField] int blockCountX;
    [SerializeField] int blockCountZ;
    [Range(0.0f, 1.0f)]
    [SerializeField] float parkRatio;

    [Header("Car Generation")]
    [SerializeField] bool parkedPosition;
    [SerializeField] int carMin;
    [SerializeField] int carMax;
    [SerializeField] int maxAttempts;

    [Header("Block .txt-files")]
    [SerializeField] List<TextAsset> blockFiles;

    [Header("Referenced Prefabs")]
    [SerializeField] GameObject road;
    [SerializeField] GameObject crossing;
    [SerializeField] GameObject groundPlate;

    [SerializeField] GameObject edgeBlock;
    [SerializeField] GameObject edgePark;
    [SerializeField] GameObject edgeRoad;

    Dictionary<string, int> buildingTypes;
    List<List<GameObject>> buildings;
    List<List<Building>> blocks;

    List<GameObject> parks;
    List<GameObject> cars;

    void Start()
    {
        //For all language computers to accept dot decimal seperation
        CultureInfo englishUSCulture = new CultureInfo("en-US"); 

        System.Threading.Thread.CurrentThread.CurrentCulture = englishUSCulture;

        //fixed numbers, the size of the 3d-assets
        float blockSize = 105;
        float roadWidth = 20;

        List<string> buildingCodes = new List<string>() {"BCC30", "BCC40",
                                                          "BCS30", "BCS40",
                                                          "BD20", "BD30",
                                                          "BP05","BP10", "BP15", "BP20",
                                                          "BR20", "BR30", "BR40",
                                                          "BRV20"};
        buildingTypes = new Dictionary<string, int>();

        buildings = GetBuildlings(buildingCodes);
        blocks = TemplateBlocks();

        parks = FromDirectory("Prefabs/Parks");
        cars = FromDirectory("Prefabs/Cars");

        MateralSelector materialSelector = new MateralSelector(seed);
        GenerateCity(blockSize, roadWidth, materialSelector);
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
    void GenerateCity(float _blockSize, float _roadWidth, MateralSelector _materialSelector)
    {
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


        Random.InitState(seed);

        for (int i = 0; i < loopX; i++)
        {
            for (int j = 0; j < loopZ; j++)
            {
                GameObject newInstance;

                //if both even, instatiate a block or park
                if ((i % 2 == 0) && (j % 2 == 0))
                {

                    float rotateBlock = Random.Range(0, 4);
                    rotateBlock *= 90;
                    //float rotateBlock = 0;

                    List<float> edgeRotations = new List<float>();
                    List<int> blockEdges = new List<int>();

                    if (j == 0)
                    {
                        edgeRotations.Add(-90);

                        //blockEdges.Add(3);
                        blockEdges.Add(EdgeBlocked(3, rotateBlock));
                    }
                    if (j == loopZ - 1)
                    {
                        edgeRotations.Add(90);

                        //blockEdges.Add(1);
                        blockEdges.Add(EdgeBlocked(1, rotateBlock));
                    }
                    if (i == 0)
                    {
                        edgeRotations.Add(0);

                        //blockEdges.Add(0);
                        blockEdges.Add(EdgeBlocked(0, rotateBlock));
                    }
                    if (i == loopX - 1)
                    {
                        edgeRotations.Add(180);

                        //blockEdges.Add(2);
                        blockEdges.Add(EdgeBlocked(2, rotateBlock));
                    }

                    float parkOrBlock = Random.Range(0.00f, 1.00f);

                    if (parkOrBlock < parkRatio)
                    {
                        Debug.Log("New park");
                        int choosePark = Random.Range(0, parks.Count); 
                        newInstance = Instantiate(parks[choosePark]);
                    }

                    else
                    {
                        Debug.Log("blockedlist: " + blockEdges.Count);
                        newInstance = GenerateBlock(_materialSelector, blockEdges);
                    }

                    Vector3 blockPos = new Vector3(currentPosX, 0, currentPosZ); 
                    newInstance.transform.Translate(blockPos);

                   newInstance.transform.Rotate(0, rotateBlock, 0, Space.World);

                    for (int k = 0; k < edgeRotations.Count; k++)
                    {
                        GameObject edgeObject;
                        if(parkOrBlock < parkRatio) edgeObject = Instantiate(edgePark);
                        else edgeObject = Instantiate(edgeBlock);
                        edgeObject.transform.Translate(blockPos);
                        edgeObject.transform.Rotate(0, edgeRotations[k], 0, Space.World);
                        edgeObject.transform.parent = newInstance.transform;
                    }
                }

                //if both uneven, instantiate a crossing
                else if ((i % 2 != 0) && (j % 2 != 0))
                {

                    newInstance = Instantiate(crossing, new Vector3(currentPosX+_roadWidth/2, 0, currentPosZ+ _roadWidth / 2), Quaternion.identity);
                }

                //otherwise, instantiate a road
                else
                {

                    newInstance = GenerateRoad(roadNr);
                    Vector3 roadPosition = new Vector3(currentPosX, 0, currentPosZ);
                    newInstance.transform.Translate(roadPosition);
                    newInstance.transform.Rotate(0, yRotation, 0, Space.World);
                    roadNr++;

                    if (i == 0 || j == 0 || i == loopX - 1 || j == loopZ - 1)
                    {
                        float edgeRotation = 0; 

                        if (j == 0) edgeRotation  = - 90;
                        else if (j == loopZ - 1) edgeRotation= 90;
                        else if (i == loopX - 1) edgeRotation = 180;

                        GameObject edgeObject = Instantiate(edgeRoad);
                        edgeObject.transform.Translate(roadPosition);
                        edgeObject.transform.Rotate(0, edgeRotation, 0, Space.World);
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

    int EdgeBlocked(int _startEdge, float _rotation)
    {
        int blockEdge;

        if (_rotation == 270) blockEdge = (_startEdge + 1) % 4;
        else if (_rotation == 180) blockEdge = (_startEdge + 2) % 4;
        else if (_rotation == 90) blockEdge = (_startEdge + 3) % 4;
        else blockEdge = _startEdge;

        return blockEdge;
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

    List<Vector3> FindCarPosition(List<Vector3> _previousPositions, float _carZ)
    {
        float minX = -46.0f;
        float maxX = 46.0f;
        int attempts = 0;
        bool placed = false;

        Vector3 carPos;

        while (attempts < maxAttempts && !placed)
        {

            carPos = new Vector3(Random.Range(minX, maxX), 0, _carZ);

            if (!_previousPositions.Any(p => Vector3.Distance(carPos, p) < 7.5f))
            {
                _previousPositions.Add(carPos);
                placed = true;
            }

            attempts++;
        }

        return _previousPositions;
    }

    GameObject GenerateBlock(MateralSelector _materialSelector, List<int> _blockedEdges)
    {
        GameObject block;

        //randomizes which block to create
        int blocknr = Random.Range(0, blocks.Count);
        //int blocknr = 1;
        Debug.Log("New block nr: " + blocknr); 

        //Creates empty parent GameObject
        string blockName = "Block" + blocknr.ToString();
        block = new GameObject(blockName);
        block.transform.parent = this.transform;

        //Instantiate ground
        GameObject ground = Instantiate(groundPlate);
        ground.transform.parent = block.transform;

        List<GameObject> doorObjects = new List<GameObject>();
        List<Building> doors = new List<Building>();

        for (int j = 0; j < _blockedEdges.Count; j++)
        {
            Debug.Log("blockedEdges: " + _blockedEdges[j]);

        }

        //loop to instantiate all buildings in the block
        for (int i = 0; i < blocks[blocknr].Count; i++)
        {
            int type = blocks[blocknr][i].TypeIndex;
            int options = buildings[type].Count;
            int chosen = Random.Range(0, options);

            

            if(blocks[blocknr][i].IsPassage)
            {
                bool connectedEdge = true;

                for (int k = 0; k < blocks[blocknr][i].Edges.Count; k++)
                {
                    int onEdge = blocks[blocknr][i].Edges[k];
                    Debug.Log("onEdge: " + onEdge);
                    

                    for (int m = 0; m < _blockedEdges.Count; m++)
                    {
                        Debug.Log("blockedEdge: " + _blockedEdges[m]);

                        if (_blockedEdges[m] == onEdge)
                        {
                            connectedEdge = false;
                            Debug.Log("SKIP THIS!");
                        }
                    }
                }



                if(connectedEdge)
                {
                    doorObjects.Add(buildings[type][chosen]);
                    doors.Add(blocks[blocknr][i]);
                }
            }

            else
            {
                GameObject building = Instantiate(buildings[type][chosen], blocks[blocknr][i].Position, Quaternion.identity);
                _materialSelector.SetMaterials(building);
                building.transform.Rotate(0, blocks[blocknr][i].Rotation, 0, Space.World);
                building.transform.parent = block.transform;
            }
        }

        int chosenDoor = Random.Range(0, doors.Count);
        Debug.Log("doorObjects: " + doorObjects.Count);

        GameObject door = Instantiate(doorObjects[chosenDoor], doors[chosenDoor].Position, Quaternion.identity);
        _materialSelector.SetMaterials(door);
        door.transform.Rotate(0, doors[chosenDoor].Rotation, 0, Space.World);
        door.transform.parent = block.transform;

        return block;
    }

    //Method to get all building types in Assets>Prefabs>Buildings (in each folder for each type)
    List<List<GameObject>> GetBuildlings(List<string> _buildingCodes)
    {
        List<List<GameObject>> buildingLists = new List<List<GameObject>>();
        int index = 0;

        foreach (string buildingType in _buildingCodes)
        {
            buildingTypes.Add(buildingType, index);
            string directory = "Prefabs/Buildings/" + buildingType;

            List<GameObject> objectsDirectory = FromDirectory(directory);

             buildingLists.Add(objectsDirectory);
             index++; 
        }

        return buildingLists;
    }

    //Method to read objects from a directory in resources
    List<GameObject> FromDirectory(string _path)
    {
        var tempList = Resources.LoadAll(_path, typeof(GameObject)).OfType<GameObject>().ToList();
        return tempList; 
    }

    //Method below reads type, position and rotation for each block type from .txt files

    List<List<Building>> TemplateBlocks()
    {
        List<List<Building>> templates = new List<List<Building>>();

        foreach (TextAsset blockFile in blockFiles)
        {
            List<Building> templateBlock = new List<Building>();
            string txtFile = blockFile.ToString();
            List<string> lines = new List<string>(txtFile.Split('\n'));

            foreach (string line in lines.Skip(1))
            {
                List<string> info = new List<string>(line.Split(','));

                if (info.Count == 5)
                {
                    int typeIndex = buildingTypes[info[0]];

                    bool isPassage = false; 
                    if(info[0].Substring(0,2) == "BP") isPassage = true;

                    Vector3 position = new Vector3(float.Parse(info[1]), float.Parse(info[2]), float.Parse(info[3]));

                    List<int> edges = new List<int>();

                    string substring = info[0].Substring(0, 2); 
                    if(substring != "BD")
                    {
                        if (position.x < -30) edges.Add(0);
                        if (position.z > 30) edges.Add(1);
                        if (position.x > 30) edges.Add(2);
                        if (position.z < -30) edges.Add(3);
                    }

                    templateBlock.Add(new Building(typeIndex, position, float.Parse(info[4]), isPassage, edges));
                }
            }

            templates.Add(templateBlock);
        }
        
        return templates;
    }


}

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
