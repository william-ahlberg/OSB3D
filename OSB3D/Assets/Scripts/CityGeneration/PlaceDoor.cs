using System.Collections.Generic;
using UnityEngine;

public class PlaceDoor : MonoBehaviour
{
    //Main function to place the door
    public GameObject Place(List<System.Tuple<GameObject, Building>> _allBuildings, List<int> _doors, List<Material> _facadeMaterials)
    {
        int chosenDoor = Random.Range(0, _doors.Count);
        GameObject door = Instantiate(_allBuildings[_doors[chosenDoor]].Item1, _allBuildings[_doors[chosenDoor]].Item2.Position, Quaternion.identity);
        SetMaterial(door, _facadeMaterials);
        door.transform.Rotate(0, _allBuildings[_doors[chosenDoor]].Item2.Rotation, 0, Space.World);
        return door;
    }


    //Sets the material of the facade, the other materials are fixed
    void SetMaterial(GameObject _prefab, List<Material> _facadeMaterials)
    {
        MeshRenderer renderer = _prefab.GetComponentInChildren<MeshRenderer>();
        Material[] prefabMaterials = renderer.sharedMaterials;

        prefabMaterials[0] = _facadeMaterials[Random.Range(0, _facadeMaterials.Count)]; //Facade
        renderer.sharedMaterials = prefabMaterials;
    }
}
