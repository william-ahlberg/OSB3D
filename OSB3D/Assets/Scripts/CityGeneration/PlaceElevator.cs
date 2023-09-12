using UnityEngine;

public class PlaceElevator : MonoBehaviour
{
    [SerializeField] GameObject elevator;
    [SerializeField] GameObject elevatorFrame;

    //Main function to add a elevator to a block, used by the BlockGenerator-class
    public GameObject Place(System.Tuple<GameObject, Building> _building, Vector3 _direction)
    {
        int floors = FloorsFromName(_building.Item1);
        Vector3 buildingSize = BuildingSize(_building.Item1);
        float buildingHeight = (buildingSize.z * 100);
        float floorHeight = buildingHeight / floors;

        Vector3 scaleVector = new(1, floorHeight, 1);

        GameObject gameObject = ElevatorFrame(floors);
        gameObject.transform.localScale = scaleVector;
        gameObject.transform.parent = gameObject.transform;

        GameObject elevatorObject = Instantiate(elevator);

        float rotation = ElevatorRotation(_direction);

        elevatorObject.transform.Rotate(0, rotation, 0, Space.World);
        elevatorObject.transform.Find("3").position += new Vector3(0, floors * floorHeight, 0);
        elevatorObject.transform.parent = gameObject.transform;

        gameObject.transform.position = ElevatorPosition(_building.Item1.name, buildingSize, _direction, _building.Item2.Position);

        elevatorObject.GetComponent<MoveElevator>().SetBuildingHeight(buildingHeight);

        return gameObject;
    }

    //Checks how many floor the building next to the elevator has
    int FloorsFromName(GameObject _building)
    {
        string name = _building.name;
        return int.Parse(name.Substring(name.IndexOf("_F") + 2, 2));
    }

    //Checks the size of the building next to the elevator
    Vector3 BuildingSize(GameObject _building)
    {
        Mesh buildingMesh = _building.GetComponentsInChildren<MeshFilter>()[0].sharedMesh;
        return buildingMesh.bounds.size;
    }

    //Generates the elevator frame, based on the number of floors
    GameObject ElevatorFrame(int _floors)
    {
        GameObject elevatorFrameObject = new("ElevatorFrame");

        Vector3 moveUp = Vector3.zero;
        int counter = 0;

        for (int i = 0; i < _floors + 1; i++)
        {
            GameObject frame = Instantiate(elevatorFrame);
            frame.transform.position += moveUp;
            frame.transform.parent = elevatorFrameObject.transform;

            moveUp.y += 1;
            counter += 1;
        }

        return elevatorFrameObject;
    }

    //Rotates the elevator based on where the building next to it is located
    float ElevatorRotation(Vector3 _direction)
    {
        if (_direction.x == -1) return -90;
        else if (_direction.x == 1) return 90;
        else if (_direction.z == -1) return 180;
        else return 0;
    }

    //Calculates the elevators position, based on the type of building standing next to it, it's postion and size
    Vector3 ElevatorPosition(string _buildingName, Vector3 _buildingSize, Vector3 _direction, Vector3 _buildingPosition)
    {
        float buildingWidth; 

        if (_buildingName.Substring(0, 3) == "BCC") buildingWidth = _buildingSize.y * 100 / 2;
        else buildingWidth = _buildingSize.x * 100 / 2;

        float addToPosition = System.Math.Abs(_direction.x) * buildingWidth + System.Math.Abs(_direction.z) * buildingWidth;
        float elevatorWidth = elevatorFrame.transform.localScale.x * 2;
        return _buildingPosition + (_direction * (addToPosition + elevatorWidth / 2));
    }
}

