using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class Utility
{
    //Returns the Building materials from the Resources folder
    public static List<List<Material>> GetBuildingMaterials()
    {
        List<List<Material>> materials;

        List<string> materialFolders = new List<string>() {"Facade", "Plinth", "DoorWindow", "Roof"};
        materials = new List<List<Material>>();
        for (int i = 0; i < materialFolders.Count; i++)
        {
            List<Material> tempList = LoadMaterials("Buildings/" + materialFolders[i]);
            materials.Add(tempList);
        }

        return materials;
    }

    //Returns a GameObject from a directory in Resources
    public static List<GameObject> FromDirectory(string _path)
    {
        var tempList = Resources.LoadAll(_path, typeof(GameObject)).OfType<GameObject>().ToList();
        return tempList;
    }

    //Returns all Block Templates in the Resources folder
    public static List<TextAsset> LoadBlockTemplates(string _path)
    {
        //For all language computers to accept dot decimal seperation
        CultureInfo englishUSCulture = new("en-US");
        System.Threading.Thread.CurrentThread.CurrentCulture = englishUSCulture;

        var tempList = Resources.LoadAll(_path, typeof(TextAsset)).OfType<TextAsset>().ToList();
        return tempList;
    }

    //Loads Materials from a folder in Resources
    public static List<Material> LoadMaterials(string _folder)
    {
        string folderPath = "Materials/" + _folder;
        List<Material> allMaterials = Resources.LoadAll(folderPath, typeof(Material)).Cast<Material>().ToList();

        return allMaterials;
    }
}
    
