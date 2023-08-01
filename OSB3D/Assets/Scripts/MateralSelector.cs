using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
        string folderPath = "Assets/Materials/Buildings/" + _folder;

        List<Material> allMaterials = new List<Material>();

        //Finds guids for all materials in folders
        string[] guids = AssetDatabase.FindAssets("t:material", new string[] { folderPath });

        foreach (string guid in guids)
        {
            //loads materials from guid and add to list
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var buildingPrefab = AssetDatabase.LoadAssetAtPath(path, typeof(Material));
            allMaterials.Add((Material)buildingPrefab);
        }

        return allMaterials;
    }


    public void SetMaterials(GameObject _prefab)
    {

        for (int i = 0; i<4 ; i++) 
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
}
