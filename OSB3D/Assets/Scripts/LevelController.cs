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

    [HideInInspector]
    public List<List<Building>> blocks;

    [HideInInspector]
    public List<List<GameObject>> buildings;

    void Start()
    {
        //fixed numbers, the size of the 3d-assets
        float blockSize = 105;
        float roadWidth = 20;


        /*NOTE: buildlingtypes is associated with a building code (0,1,2,..) in the templates, which depends on the order in this list. 
        Other association would be better, so it is easier to add buildling types, so this needs to be fixed. 
        For now - do ONLY add new types to the end of the list, undependent on type/spelling. */
        List<string> buildingTypes = new List<string>() {"BCC30", "BCC40", 
                                                          "BCS30", "BCS40", 
                                                          "BD20", "BD30",
                                                          "BP10",
                                                          "BR20", "BR30", "BR40", 
                                                          "BRV20"}; 

        buildings = AllBuildlings(buildingTypes);
        blocks = TemplateBlocks();
        GenerateCity(blockSize, roadWidth);
    }

    //Main function to generate city
    void GenerateCity(float _blockSize, float _roadWidth)
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
                    newInstance = GenerateBlock();

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

    GameObject GenerateBlock()
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
            int type = blocks[blocknr][i].BuildingCode;
            int options = buildings[type].Count;
            int chosen = Random.Range(0, options);

            GameObject building = Instantiate(buildings[type][chosen], blocks[blocknr][i].Position, Quaternion.identity);
            building.transform.Rotate(0, blocks[blocknr][i].Rotation, 0, Space.World);
            building.transform.parent = block.transform;
        }

        return block; 
    }

    //Function to get all building types in Assets>Prefabs>Buildings (in each folder for each type)
    List<List<GameObject>> AllBuildlings(List<string> _buildingTypes)
    {
        List<List<GameObject>> buildingLists = new List<List<GameObject>>();

        foreach (string buildingType in _buildingTypes)
        {
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
                    templateBlock.Add(new Building(int.Parse(info[0]), new Vector3(float.Parse(info[1]), float.Parse(info[2]), float.Parse(info[3])), float.Parse(info[4])));
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
    private int buildingCode;
    private Vector3 position;
    private float yRotation;

    public Building(int _buildingCode, Vector3 _Position, float _yRotation)
    {
        this.buildingCode = _buildingCode;
        this.position = _Position;
        this.yRotation = _yRotation;
    }

    public int BuildingCode { get { return this.buildingCode; } }
    public Vector3 Position { get { return this.position; } }
    public float Rotation { get { return this.yRotation; } }
}
