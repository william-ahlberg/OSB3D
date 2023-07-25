using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class LevelController : MonoBehaviour
{
    [Header("City setup")]
    public int seed; 
    public int blockCountX;
    public int blockCountZ;

    [Header("Block .txt-files")]
    public List<TextAsset> blockFiles;

    [Header("Referenced prefabs")]
    public GameObject road;
    public GameObject crossing;
    public GameObject groundPlate;
    public List<GameObject> cars;

    Dictionary<string, int> buildingTypes;
    List<List<GameObject>> buildings;
    List<List<Building>> blocks;

    void Start()
    {
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

        buildings = AllBuildlings(buildingCodes);
        blocks = TemplateBlocks();

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

        Random.InitState(seed);

        for (int i = 0; i < loopX; i++)
        {
            for (int j = 0; j < loopZ; j++)
            {
                GameObject newInstance;

                //if both even, instatiate a block
                if ((i % 2 == 0) && (j % 2 == 0))
                {
                    newInstance = GenerateBlock(_materialSelector);

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
                    newInstance = Instantiate(road, new Vector3(currentPosX, 0, currentPosZ), Quaternion.identity);
                    newInstance.transform.Rotate(0, yRotation, 0, Space.World);

                    int carNr = Random.Range(0, 4);

                    for (int k = 0; k < carNr; k++)
                    {
                        int carDirection = Random.Range(0, 2);

                        float carZ, carRot;

                        if(carDirection == 0)
                        {
                            carZ = -3;
                            carRot = 90;
                        }

                        else 
                        
                        {
                            carZ = 3;
                            carRot = -90;
                        }
                        
                        Vector3 carPos = new Vector3(currentPosX, 0,currentPosZ + carZ);
                        //GameObject newCar = Instantiate(car)
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

    GameObject GenerateBlock(MateralSelector _materialSelector)
    {
        GameObject block;

        //randomizes which block to create
        int blocknr = Random.Range(0, blocks.Count);

        //Creates emtpy parent GameObject
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
    List<List<GameObject>> AllBuildlings(List<string> _buildingCodes)
    {
        List<List<GameObject>> buildingLists = new List<List<GameObject>>();
        int index = 0;

        foreach (string buildingType in _buildingCodes)
        {
            buildingTypes.Add(buildingType, index);
            string folder = "Assets/Prefabs/Buildings/" + buildingType;

            List<GameObject> tempList = new List<GameObject>();

            //Finds guids for all prefabs in folders
            string[] guids = AssetDatabase.FindAssets("t:prefab", new string[] { folder });

             foreach (string guid in guids)
             {
                //loads objects from guid and add to list
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var buildingPrefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
                tempList.Add((GameObject) buildingPrefab);
            }

             buildingLists.Add(tempList);
             index++; 
        }

        return buildingLists;
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
