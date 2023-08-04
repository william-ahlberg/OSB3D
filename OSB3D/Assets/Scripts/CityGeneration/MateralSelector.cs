using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class MateralSelector
{
    int seed;
    List<List<Material>> materials;

    public MateralSelector(int _seed)
    {
        seed = _seed;

        //OBS! connect index with string?
        List<string> materialFolders = new List<string>() { "Roof", "Plinth", "Facade", "DoorWindow" };
        materials = new List<List<Material>>();
        for (int i = 0; i < materialFolders.Count; i++)
        {
            List<Material> tempList = LoadMaterials(materialFolders[i]);
            materials.Add(tempList);
        }
    }

    List<Material> LoadMaterials(string _folder)
    {
        string folderPath = "Materials/Buildings/" + _folder;
        List<Material> allMaterials = new List<Material>();
        allMaterials = Resources.LoadAll(folderPath, typeof(Material)).Cast<Material>().ToList();

        return allMaterials;
    }

    public void SetMaterials(GameObject _prefab)
    {

        for (int i = 0; i<4 ; i++) 
        {
            MeshRenderer renderer = _prefab.GetComponentInChildren<MeshRenderer>();
            Material[] prefabMaterials = renderer.materials;
            prefabMaterials[0] = materials[0][Random.Range(0, materials[0].Count)]; //Roof
            prefabMaterials[0] = materials[0][Random.Range(0, materials[0].Count)]; //Roof
            prefabMaterials[1] = materials[1][Random.Range(0, materials[1].Count)]; //Plinth
            prefabMaterials[2] = materials[2][Random.Range(0, materials[2].Count)]; //Facade

            int windowDoor = Random.Range(0, materials[3].Count);
            prefabMaterials[3] = materials[3][windowDoor]; //Window
            prefabMaterials[5] = materials[3][windowDoor]; //DoorFrame
            prefabMaterials[8] = materials[3][windowDoor]; //DoorPanel
            renderer.materials = prefabMaterials;
        }
    }
}
