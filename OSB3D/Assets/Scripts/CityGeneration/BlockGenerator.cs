using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BlockGenerator : MonoBehaviour
{
    [SerializeField] GameObject groundPlate;

    List<List<Building>> blocks;
    List<List<GameObject>> buildings;
    Dictionary<string, int> buildingTypes;
    List<List<Material>> materials;
    List<TextAsset> blockFiles;

    BlockTerrain blockTerrain;
    PlaceElevator placeElevator;
    PlaceDoor placeDoor;

    private float lowerTerrainBuildings = 0.5f;

    void Awake()
    {
        Setup();
    }

    //Setup used to also generate the city from the editor
    public void Setup()
    {
        //To ensure .txt information is read correct on all langugae computers
        CultureInfo englishUSCulture = new("en-US");
        System.Threading.Thread.CurrentThread.CurrentCulture = englishUSCulture;

        /* buildingCodes, if a new typ of builing is added it should be listed here with it's own code.
         Order of codes do not matter, now in alphabetic order*/
        List<string> buildingCodes = new() {"BCL30", "BCL40",
                                            "BCR30", "BCR40",
                                             "BD20", "BD30", "BD60", "BD85",
                                             "BP05","BP10", "BP15", "BP20",
                                             "BR20", "BR30", "BR40",
                                             "BRV20",
                                             "BT20"};

        buildingTypes = new Dictionary<string, int>();
        buildings = GetBuildlings(buildingCodes);
        blockFiles = Utility.LoadBlockTemplates("BlockTemplates");
        materials = Utility.GetBuildingMaterials();
        blocks = TemplateBlocks();
        blockTerrain = GetComponent<BlockTerrain>();
        placeElevator = GetComponent<PlaceElevator>();
        placeDoor = GetComponent<PlaceDoor>();
    }

    //Method to get all building types in Resources>Prefabs>Buildings (in each folder for each type)
    List<List<GameObject>> GetBuildlings(List<string> _buildingCodes)
    {
        List<List<GameObject>> buildingLists = new List<List<GameObject>>();
        int index = 0;
        foreach (string buildingType in _buildingCodes)
        {
            buildingTypes.Add(buildingType, index);
            string directory = "Prefabs/Buildings/" + buildingType;
            List<GameObject> objects = new List<GameObject>();
            objects.AddRange(Utility.FromDirectory(directory));

            buildingLists.Add(objects);
            index++;
        }

        return buildingLists;
    }

    //Method below reads type, position and rotation for each block type from .txt files as a string
    List<List<Building>> TemplateBlocks()
    {
        List<List<Building>> templates = new();

        foreach (TextAsset blockFile in blockFiles)
        {
            List<Building> templateBlock = new();
            string txtFile = blockFile.ToString();
            List<string> lines = new(txtFile.Split('\n'));

            foreach (string line in lines.Skip(1))
            {
                List<string> info = new(line.Split(','));

                if (info.Count == 5)
                {
                    int typeIndex = buildingTypes[info[0]];

                    bool isPassage = false; 
                    if(info[0].Substring(0, 2) == "BP") isPassage = true;

                    bool isTerrain = false;
                    if(info[2] == "t") isTerrain = true;

                    Vector3 position = BuildingPosition(info, isTerrain);

                    List<int> edges = new();

                    //If it is not a detached buliding and the x and z value is 
                    if (info[0].Substring(0, 2) != "BD")
                    {
                        if (position.x < -30) edges.Add(0);
                        if (position.z > 30) edges.Add(1);
                        if (position.x > 30) edges.Add(2);
                        if (position.z < -30) edges.Add(3);
                    }

                    templateBlock.Add(new Building(typeIndex, position, float.Parse(info[4]), isPassage, edges, isTerrain));
                }
            }
            templates.Add(templateBlock);
        }

        return templates;
    }

    //Gets the buliding positon from a string 
    Vector3 BuildingPosition(List<string> _info, bool _isTerrain)
    {
        float x = float.Parse(_info[1]);
        float y;
        float z = float.Parse(_info[3]);

        if (_isTerrain) y = 0;
        else y = float.Parse(_info[2]);

        return new Vector3(x, y, z);
    }

    public List<int> RandomTemplateIndices(int _count)
    {
        List<int> indices = new List<int>();

        for (int i = 0; i < _count; i++)
        {
            indices.Add(UnityEngine.Random.Range(0, blocks.Count));
        }
        return indices;
    }

    //Looks for a blocknr which do not include terrain
    public int RandomRangeExcept()
    {
        int index;
        do
        {
            index = UnityEngine.Random.Range(0, blocks.Count);
        } while (blocks[index][0].Terrain);

        return index;
    }

    //Main function to generate the terrain
    public Tuple<bool, GameObject, GameObject> GenerateBlock(List<System.Tuple<int, float, int>> _edges, bool _addElevator, int _blockSize, int _blocknr)
    {
        //Creates empty parent GameObject
        string blockName = "Block" + _blocknr.ToString();
        GameObject block = new(blockName);

        bool isTerrain = false;
        if (blocks[_blocknr][0].Terrain) isTerrain = true;

        GameObject ground = GroundObject(isTerrain, _blockSize, block);

        List<System.Tuple<GameObject, Building>> allBuildings = new();
        List<int> doors = new();
        List<int> forElevator = new();
        List<Vector3> directions = new();

        //loop to instantiate all buildings in the block
        for (int i = 0; i < blocks[_blocknr].Count; i++)
        {
            int type = blocks[_blocknr][i].TypeIndex;
            int options = buildings[type].Count;
            int chosen = UnityEngine.Random.Range(0, options);

            GameObject chosenObject = buildings[type][chosen];

            //Keep track of all buildings for the elevator placement
            allBuildings.Add(new System.Tuple<GameObject, Building>(chosenObject, blocks[_blocknr][i]));

            //Check if it is a passage towards a courtyard, if so a door and elevator can be placed
            if (blocks[_blocknr][i].IsPassage)
            {
                //checks if the edge is towards another block, if so a door object can be placed
                bool connectedEdge = CheckEdges(blocks[_blocknr][i].Edges, _edges);

                if (connectedEdge) doors.Add(i);

                //if a elevator should be placed, the index of the gap and the direction towards the previous building is saved
                if (_addElevator)
                {
                    if (i != 0)
                    {
                        forElevator.Add(i - 1);
                        Vector3 direction = PlacementDirection(allBuildings[i].Item2.Position, allBuildings[i - 1].Item2.Position);
                        directions.Add(direction);
                    }
                }
            }

            //If not a passage, instantiate a building
            else
            {
                Vector3 position = blocks[_blocknr][i].Position;
                float buildingRotation = blocks[_blocknr][i].Rotation;

                if (isTerrain) position.y = TerrainHeight(allBuildings[i], ground, block); 

                GameObject building = Instantiate(buildings[type][chosen], position, Quaternion.identity);
                SetBuildingMaterials(building);
                building.transform.Rotate(0, buildingRotation, 0, Space.World);
                building.transform.parent = block.transform;

            }
        }

        //If positions to place the door have been found, randomly choose one 
        if (doors.Count > 0)
        {
            GameObject door = placeDoor.Place(allBuildings, doors, materials[0]);
            door.transform.parent = block.transform;
        }

        //If positions to place the elevator have been found, randomly choose one
        if (forElevator.Count > 0)
        {
            int chosenPostition = UnityEngine.Random.Range(0, forElevator.Count);
            GameObject elevator = placeElevator.Place(allBuildings[forElevator[chosenPostition]], directions[chosenPostition]);
            elevator.transform.parent = block.transform;
        }

        return new Tuple<bool, GameObject, GameObject>(isTerrain, block, ground);
    }

    //Instantiate ground or terrain
    GameObject GroundObject(bool _isTerrain, int _blockSize, GameObject _block)
    {
        GameObject ground; 

        if (_isTerrain)
        {
            ground = blockTerrain.Generate(_blockSize);
        }

        else
        {
            ground = Instantiate(groundPlate);
            ground.transform.parent = _block.transform;
        }

        return ground;
    }

    //Calculates in which direction the building is, in realation to the elevator
    Vector3 PlacementDirection(Vector3 _elevatorPosition, Vector3 _buildingPositon)
    {
        Vector3 direction = _elevatorPosition - _buildingPositon;
        
        if (System.Math.Abs(direction.x) > System.Math.Abs(direction.z)) direction = new Vector3(direction.x, 0, 0);
        else direction = new Vector3(0, 0, direction.z);
        
        direction.Normalize();
        return direction;
    }

    float TerrainHeight(System.Tuple<GameObject, Building> _buildingTuple, GameObject _ground, GameObject _block)
    {
        float yValue; 

        Mesh buildingMesh = _buildingTuple.Item1.GetComponentsInChildren<MeshFilter>()[0].sharedMesh;
        Vector3 size = BuildingSize(buildingMesh, _buildingTuple.Item2.Rotation);
        Terrain terrainComp = _ground.GetComponentInChildren<Terrain>();

        //Samples the corners of the building and chooses the smallest y-value
        yValue = SampleHeights(size, _buildingTuple.Item2.Position, terrainComp, _block) - lowerTerrainBuildings;

        return yValue;
    }

    Vector3 BuildingSize(Mesh _buildingMesh, float _rotation)
    {
        Vector3 meshSize = _buildingMesh.bounds.size;

        float buildingWidthX = meshSize.x;
        float buildingDepthZ = meshSize.z;

        if (_rotation == 90 || _rotation == -90)
        {
            buildingWidthX = meshSize.z;
            buildingDepthZ = meshSize.x;
        }

        return new Vector3(buildingWidthX, 0, buildingDepthZ);
    }

    //Sample the height in the buildings four courner, for terrain blocks
    float SampleHeights(Vector3 _buildingSize, Vector3 _position, Terrain _terrainComp, GameObject _block)
    {
        List<float> sampleHeights = new List<float>();

        float addX = (_buildingSize.x -1) / 2;
        float addZ = (_buildingSize.z -1) / 2;

        float heigh1 = _terrainComp.SampleHeight(new Vector3(_position.x - addX, 0, _position.z - addZ));
        sampleHeights.Add(heigh1);

        float heigh2 = _terrainComp.SampleHeight(new Vector3(_position.x - addX, 0, _position.z + addZ));
        sampleHeights.Add(heigh2);

        float heigh3 = _terrainComp.SampleHeight(new Vector3(_position.x + addX, 0, _position.z + addZ));
        sampleHeights.Add(heigh3);

        float height4 = _terrainComp.SampleHeight(new Vector3(_position.x + addX, 0, _position.z - addZ));
        sampleHeights.Add(height4);

        return sampleHeights.Min();
    }

    //Checks which sides of the block is towards an edge, if any
    bool CheckEdges(List<int> _blockEdges, List<System.Tuple<int, float, int>> _outerEdgh)
    {
        bool connectedEdge = true;

        for (int k = 0; k < _blockEdges.Count; k++)
        {
            int onEdge = _blockEdges[k];

            for (int m = 0; m < _outerEdgh.Count; m++)
            {
                if (_outerEdgh[m].Item3 == onEdge)
                {
                    connectedEdge = false;
                }
            }
        }

        return connectedEdge;
    }

    //Randomises the building materials. Works with the prefabs included, where the material order are in accordance with the seperate documentation
    public void SetBuildingMaterials(GameObject _prefab)
    {
                MeshRenderer renderer = _prefab.GetComponentInChildren<MeshRenderer>();
                Material[] prefabMaterials = renderer.sharedMaterials;

                prefabMaterials[0] = materials[0][UnityEngine.Random.Range(0, materials[0].Count)];  //Facade
                prefabMaterials[1] = materials[1][UnityEngine.Random.Range(0, materials[1].Count)]; //Plinth
                
                int windowDoor = UnityEngine.Random.Range(0, materials[2].Count);
                prefabMaterials[2] = materials[2][windowDoor]; //Door
                prefabMaterials[3] = materials[2][windowDoor]; //Window

                prefabMaterials[4] = materials[3][UnityEngine.Random.Range(0, materials[3].Count)]; //Roof
                renderer.sharedMaterials = prefabMaterials;
    }
}