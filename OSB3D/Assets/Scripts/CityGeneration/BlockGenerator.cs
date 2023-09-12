using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class BlockGenerator : MonoBehaviour
{
    [SerializeField] GameObject groundPlate;

    List<List<Building>> blocks;
    List<List<GameObject>> buildings;
    Dictionary<string, int> buildingTypes;
    List<List<Material>> materials;
    List<TextAsset> blockFiles;

    PlaceElevator placeElevator;
    PlaceDoor placeDoor;

    void Awake()
    {
        //To ensure .txt information is read correct on all langugae computers
        CultureInfo englishUSCulture = new("en-US");
        System.Threading.Thread.CurrentThread.CurrentCulture = englishUSCulture;

        /* buildingCodes, if a new typ of builing is added it should be listed here with it's own code.
         Order of codes do not matter, now in alphabetic order*/
        List<string> buildingCodes = new() {"BCC30", "BCC40",
                                            "BCS30", "BCS40",
                                             "BD20", "BD30",
                                             "BP05","BP10", "BP15", "BP20",
                                             "BR20", "BR30", "BR40",
                                             "BRV20"};

        buildingTypes = new Dictionary<string, int>();
        buildings = GetBuildlings(buildingCodes);
        blockFiles = Utility.LoadBlockTemplates("BlockTemplates");
        materials = Utility.GetBuildingMaterials();
        blocks = TemplateBlocks();
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

            List<GameObject> objectsDirectory = Utility.FromDirectory(directory);

            buildingLists.Add(objectsDirectory);
            index++;
        }

        return buildingLists;
    }

    //Method below reads type, position and rotation for each block type from .txt files as a string
    List<List<Building>> TemplateBlocks()
    {
        List<List<Building>> templates = new();
        //List<TextAsset> blockFiles = Utility.LoadBlockTemplates("BlockTemplates");
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
                    if (info[0].Substring(0, 2) == "BP") isPassage = true;

                    Vector3 position = new(float.Parse(info[1]), float.Parse(info[2]), float.Parse(info[3]));

                    List<int> edges = new();

                    string substring = info[0].Substring(0, 2);
                    
                    //If it is not a detached buliding and the x and z value is 
                    if (substring != "BD")
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

    public GameObject GenerateBlock(List<System.Tuple<int, float, int>> _edges, bool _addElevator)
    {
        //randomizes which block to create
        int blocknr = Random.Range(0, blocks.Count);
        //int blocknr = 1;

        //Creates empty parent GameObject
        string blockName = "Block" + blocknr.ToString();
        GameObject block = new(blockName);

        //Instantiate ground
        GameObject ground = Instantiate(groundPlate);
        ground.transform.parent = block.transform;

        List<System.Tuple<GameObject, Building>> allBuildings = new();
        List<int> doors = new();
        List<int> forElevator = new();
        List<Vector3> directions = new();

        //loop to instantiate all buildings in the block
        for (int i = 0; i < blocks[blocknr].Count; i++)
        {
            int type = blocks[blocknr][i].TypeIndex;
            int options = buildings[type].Count;
            int chosen = Random.Range(0, options);

            //Keep track of all buildings for the elevator placement
            allBuildings.Add(new System.Tuple<GameObject, Building>(buildings[type][chosen], blocks[blocknr][i]));

            //Check if it is a passage towards a courtyard, if so a door and elevator can be placed
            if (blocks[blocknr][i].IsPassage)
            {
                //checks if the edge is towards another block, if so a door object can be placed
                bool connectedEdge = CheckEdges(blocks[blocknr][i].Edges, _edges);

                if (connectedEdge)
                {
                    doors.Add(i);
                }

                //if a elevator should be placed, the index of the gap and the direction towards the previous building is saved
                if (_addElevator)
                {
                    if (i != 0)
                    {
                        forElevator.Add(i - 1);
                        Vector3 direction = allBuildings[i].Item2.Position - allBuildings[i - 1].Item2.Position;
                        if (System.Math.Abs(direction.x) > System.Math.Abs(direction.z)) direction = new Vector3(direction.x, 0, 0);
                        else direction = new Vector3(0, 0, direction.z);
                        direction.Normalize();
                        directions.Add(direction);
                    }
                }
            }

            //If not a passage, instantiate a building
            else
            {
                GameObject building = Instantiate(allBuildings[i].Item1, blocks[blocknr][i].Position, Quaternion.identity);
                SetBuildingMaterials(building);
                building.transform.Rotate(0, blocks[blocknr][i].Rotation, 0, Space.World);
                building.transform.parent = block.transform;
            }
        }

        //If positions to place the door have been found, randomly choose one 
        if (doors.Count > 0)
        {
            GameObject door = placeDoor.Place(allBuildings, doors, materials[2]);
            door.transform.parent = block.transform;
        }

        //If positions to place the elevator have been found, randomly choose one
        if (forElevator.Count > 0)
        {
            int chosenPostition = Random.Range(0, forElevator.Count);
            GameObject elevator = placeElevator.Place(allBuildings[forElevator[chosenPostition]], directions[chosenPostition]);
            elevator.transform.parent = block.transform;
        }

        return block;
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
            Material[] prefabMaterials = renderer.materials;

                prefabMaterials[0] = materials[0][Random.Range(0, materials[0].Count)];  //Roof
                prefabMaterials[1] = materials[1][Random.Range(0, materials[1].Count)]; //Plinth
                prefabMaterials[2] = materials[2][Random.Range(0, materials[2].Count)]; //Facade

                int windowDoor = Random.Range(0, materials[3].Count);
                prefabMaterials[3] = materials[3][windowDoor]; //Window
                prefabMaterials[5] = materials[3][windowDoor]; //DoorFrame
                prefabMaterials[8] = materials[3][windowDoor]; //DoorPanel
                renderer.materials = prefabMaterials;
    }
}