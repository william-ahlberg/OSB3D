using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    [Header("Placing items and cars")]
    [SerializeField] int maxAttempts;

    [Header("Street Items per road segment")]
    [SerializeField] int itemMin;
    [SerializeField] int itemMax;

    [Header("Cars per road segments")]
    [SerializeField] bool parkedPosition;
    [SerializeField] int carMin;
    [SerializeField] int carMax;

    [Header("Referenced Prefabs")]
    [SerializeField] GameObject road;

    List<GameObject> cars;
    List<GameObject> streetItems;
    List<Material> carMaterials;
    List<GameObject> streetTrees;

    private List<float> lightTreePositions;
    private float carSpacing, itemSpacing; 

    private void Awake()
    {
        cars = Utility.FromDirectory("Prefabs/Cars");
        streetItems = Utility.FromDirectory("Prefabs/StreetItems");
        carMaterials = Utility.LoadMaterials("Cars");
        streetTrees = Utility.FromDirectory("Prefabs/StreetTrees");


        lightTreePositions = new List<float>() { -45, -30, -15, 0, 15, 30, 45 };

        //with the included assets, the spacing below ensure no overlaps
        itemSpacing = 3.0f;
        carSpacing = 7.5f;
    }

    //Generates a road object, including randomly placed cars and street items
    public GameObject GenerateRoad(int _roadnr, float _roadWidth)
    {
        string roadName = "Road" + _roadnr.ToString();
        GameObject roadObject = new(roadName);

        GameObject newRoad = Instantiate(road);
        newRoad.transform.parent = roadObject.transform;

        PlaceTrees(newRoad);
        List<Vector3> carPositions = PlacingPositions(true, _roadWidth);
        PlaceItems(roadObject, carPositions, true);

        List<Vector3> itemPositions = PlacingPositions(false, _roadWidth);

        PlaceItems(roadObject, itemPositions, false);

        return roadObject;
    }

    void PlaceTrees(GameObject _newRoad)
    {
        List<Vector3> treePositions = TreePositions(_newRoad);
        int nrTrees = treePositions.Count;

        List<float> scaleFactors = ScaleFactors(nrTrees);

        for (int i = 0; i < nrTrees; i++)
        {
            Vector3 scaleVector = new Vector3(scaleFactors[i], scaleFactors[i], scaleFactors[i]);
            float rotate = Random.Range(0, 359);
            int index = Random.Range(0, streetTrees.Count);
            GameObject newTree = Instantiate(streetTrees[index], treePositions[i], Quaternion.identity);
            newTree.transform.localScale = scaleVector;
            newTree.transform.localRotation = Quaternion.Euler(0, rotate, 0);
            newTree.transform.parent = _newRoad.transform;
        }
    }

     List<Vector3> TreePositions(GameObject _newRoad)
    {
        List<Vector3> treesPos = new(); 

            foreach (Transform tpos in _newRoad.GetComponentsInChildren<Transform>())
            {            

            if (tpos.CompareTag("StreetTree"))
                {
                //trees.Add(tree);
                treesPos.Add(new Vector3(tpos.position.x, 0.2f, tpos.position.z));
                }
            }

            return treesPos;
    }

    List<float> ScaleFactors (int _count)
    {
        List<float> result = new();

        for (int i = 0; i < _count; i++)
        {
            result.Add(Random.Range(0.9f, 1.2f)); 
        }

        return result;
    }

    //Returns a list of car positions
    List<Vector3> PlacingPositions(bool _cars, float _roadWidth)
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
            if (parkedPosition) zPosition = _roadWidth * 0.24f;
            else zPosition = _roadWidth * 0.15f;

            //Threshold of 7.5f ensure no overlap of cars
            threshold = carSpacing;
        }

        else
        {
            nrPositions = Random.Range(itemMin, itemMax);

            for (int i = 0; i < lightTreePositions.Count; i++)
            {
                direction0.Add(new Vector3(lightTreePositions[i], 0, -7));
                direction1.Add(new Vector3(lightTreePositions[i], 0, 7));
            }

            //Z position of 6.8 places street items on the edge of the curb towards the road, between the threes and street lights. 3.0 ensures no overlaps of street items or trees. 
            zPosition = 6.8f;
            threshold = itemSpacing;
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

        if (!_cars)
        {
            direction0.RemoveRange(0, lightTreePositions.Count);
            direction1.RemoveRange(0, lightTreePositions.Count);
        }

        direction0.AddRange(direction1);

        return direction0;
    }

    //Adds a new car position to the list of carPositions, if it succesfully fins a position within a max number of attempts
    List<Vector3> FindPosition(List<Vector3> _positions, float _zPosition, float _threshold)
    {
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
        MeshRenderer renderer = carImport.GetComponent<MeshRenderer>();
        Material[] prefabMaterials = renderer.materials;
        prefabMaterials[0] = carMaterials[Random.Range(0, carMaterials.Count)];
        renderer.materials = prefabMaterials;
    }
}
