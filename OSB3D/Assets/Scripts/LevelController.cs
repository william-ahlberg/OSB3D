using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.Globalization;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [Header("City Setup")]
    [SerializeField] int seed;
    [SerializeField] int blockCountX;
    [SerializeField] int blockCountZ;

    [Header("Park Ratio - a number between 0.0 and 1.0")]
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
                                                          "BP10",
                                                          "BR20", "BR30", "BR40",
                                                          "BRV20"};
        buildingTypes = new Dictionary<string, int>();

        buildings = GetBuildlings(buildingCodes);
        blocks = TemplateBlocks();

        parks = FromDirectory("Assets/Prefabs/Parks");
        cars = FromDirectory("Assets/Prefabs/Cars");

        MateralSelector materialSelector = new MateralSelector(seed);
        GenerateCity(blockSize, roadWidth, materialSelector);
    }

    //Main function to generate city
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
                    float parkOrBlock = Random.Range(0.00f, 1.00f);

                    if (parkOrBlock < parkRatio)
                    {
                        int choosePark = Random.Range(0, parks.Count); 
                        newInstance = Instantiate(parks[choosePark]);
                    }

                    else
                    {
                        newInstance = GenerateBlock(_materialSelector);
                    }

                    newInstance.transform.Translate(new Vector3(currentPosX, 0, currentPosZ));

                    float rotateBlock = Random.Range(0, 4);
                    rotateBlock *= 90;
                    newInstance.transform.Rotate(0, rotateBlock, 0, Space.World);
                }

                //if both uneven, instantiate a crossing
                else if ((i % 2 != 0) && (j % 2 != 0))
                {

                    newInstance = Instantiate(crossing, new Vector3(currentPosX+_roadWidth/2, 0, currentPosZ+ _roadWidth / 2), Quaternion.identity);
                }

                //otherwise, instantiate a road
                else
                {
                    newInstance = GenerateRoad(roadNr, currentPosX, currentPosZ);
                    newInstance.transform.Translate(new Vector3(currentPosX, 0, currentPosZ));
                    newInstance.transform.Rotate(0, yRotation, 0, Space.World);
                    roadNr++;
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

    GameObject GenerateRoad(int _roadnr,  float _currentPosX, float _currentPosZ)
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

    GameObject GenerateBlock(MateralSelector _materialSelector)
    {
        GameObject block;

        //randomizes which block to create
        int blocknr = Random.Range(0, blocks.Count);

        //Creates empty parent GameObject
        string blockName = "Block" + blocknr.ToString();
        block = new GameObject(blockName);
        block.transform.parent = this.transform;

        //Instantiate ground
        GameObject ground = Instantiate(groundPlate);
        ground.transform.parent = block.transform;

        //loop to instantiate all buildings in the block
        for (int i = 0; i < blocks[blocknr].Count; i++)
        {
            int type = blocks[blocknr][i].TypeIndex;
            int options = buildings[type].Count;
            int chosen = Random.Range(0, options);

            GameObject building = Instantiate(buildings[type][chosen], blocks[blocknr][i].Position, Quaternion.identity);
            _materialSelector.SetMaterials(building);
            building.transform.Rotate(0, blocks[blocknr][i].Rotation, 0, Space.World);
            building.transform.parent = block.transform;
        }

        return block;
    }

    //Function to get all building types in Assets>Prefabs>Buildings (in each folder for each type)
    List<List<GameObject>> GetBuildlings(List<string> _buildingCodes)
    {
        List<List<GameObject>> buildingLists = new List<List<GameObject>>();
        int index = 0;

        foreach (string buildingType in _buildingCodes)
        {
            buildingTypes.Add(buildingType, index);
            string directory = "Assets/Prefabs/Buildings/" + buildingType;

            List<GameObject> objectsDirectory = FromDirectory(directory);

             buildingLists.Add(objectsDirectory);
             index++; 
        }

        return buildingLists;
    }

    //Function to read objects from a directory
    List<GameObject> FromDirectory(string _path)
    {
        List<GameObject> tempList = new();

        //Finds guids for all prefabs in folders
        string[] guids = AssetDatabase.FindAssets("t:prefab", new string[] { _path });

        foreach (string guid in guids)
        {
            //loads objects from guid and add to list
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var buildingPrefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
            tempList.Add((GameObject)buildingPrefab);
        }

        return tempList; 
    }

    /*Function below reads type, position and rotation for each block type from .txt files. 
      Info is read as strings and converted to numbers, maybe other ways to store the block information could be better? */

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
                    templateBlock.Add(new Building(typeIndex, new Vector3(float.Parse(info[1]), float.Parse(info[2]), float.Parse(info[3])), float.Parse(info[4])));
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

    public Building(int _typeIndex, Vector3 _Position, float _yRotation)
    {
        this.typeIndex = _typeIndex;
        this.position = _Position;
        this.yRotation = _yRotation;
    }

    public int TypeIndex { get { return this.typeIndex; } }
    public Vector3 Position { get { return this.position; } }
    public float Rotation { get { return this.yRotation; } }
}
