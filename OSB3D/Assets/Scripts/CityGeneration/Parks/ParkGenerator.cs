using UnityEngine;
using System.Collections;
//using UnityEditor.AssetImporters;
using UnityEngine.ProBuilder.MeshOperations;
using static UnityEngine.Mesh;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using Google.Protobuf.WellKnownTypes;
using Unity.VisualScripting;

public class ParkGenerator : MonoBehaviour
{

    public enum DrawMode { NoiseMap, Mesh, FalloffMap };
    public DrawMode drawMode;
    public Renderer planeRenderer; 

    public Material material;
  //  public Texture2D testTexture;

    public int resolution;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public float heightMultiplier; 

    public int seed;
    public Vector2 offset;

    [Range(0.0f, 1.0f)]
    public float edgeDistance;

    public bool useFalloff; 
    public bool autoUpdate;

    float[,] falloffMap;


    public void GenerateMap()
    {
        float[,] noiseMap = NoiseMap.GenerateNoise(resolution, seed, noiseScale, octaves, persistance, lacunarity, offset);

            
           falloffMap = new float[resolution, resolution];
            Gradient gradient = EdgeGradient();


            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {

                    float value1 = Mathf.Abs(x / (float)(resolution - 1f) -0.5f);
                    float value2 = Mathf.Abs(y / (float)(resolution - 1f) - 0.5f);

                    float toEvaluate = Mathf.Max(value1, value2);

                    Color colour = gradient.Evaluate(toEvaluate);
                    falloffMap[x, y] = 1 - colour.b; 
                    

                if (useFalloff)  noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);

                }
            }

        DisplayTerrain(noiseMap);
    }

    Gradient EdgeGradient()
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colourKeys = new GradientColorKey[5];

        float fallOff = edgeDistance / 2f;
        Debug.Log("fallOff: " + fallOff.ToString());

        colourKeys[0] = new GradientColorKey(Color.white, 0.0f);
        colourKeys[1] = new GradientColorKey(Color.white, fallOff);
        colourKeys[2] = new GradientColorKey(Color.black, 0.5f);
        colourKeys[3] = new GradientColorKey(Color.white, 1.0f - fallOff);
        colourKeys[4] = new GradientColorKey(Color.white, 1.0f);

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };

        gradient.SetKeys(colourKeys, alphaKeys);

        return gradient;
    }

    void DisplayTerrain(float[,] _noiseMap)
    {
        if (drawMode == DrawMode.NoiseMap || drawMode == DrawMode.FalloffMap)
        {
            Texture2D texture;

            if (drawMode == DrawMode.FalloffMap)
            {
                texture = TextureFromNoise(falloffMap);
            }
            else
            {
                texture = TextureFromNoise(_noiseMap);
            }

            Material newMaterial = new Material(Shader.Find("Standard"));
            newMaterial.mainTexture = texture;
            planeRenderer.sharedMaterial = newMaterial;
            planeRenderer.transform.localScale = new Vector3(resolution, 1, resolution);
        }

        else
        {
            GameObject terrain = new GameObject("Park");
            MeshRenderer renderer = terrain.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = terrain.AddComponent<MeshFilter>();
            MeshCollider meshcollider = terrain.AddComponent<MeshCollider>();

            Mesh mesh;

            renderer.material = material;

            mesh = CreateMesh(true, _noiseMap);
            meshFilter.sharedMesh = mesh;
            meshcollider.sharedMesh = mesh;
        }
    }

    Texture2D TextureFromNoise(float[,] _noiseMap)
    {
        Color[] colourMap = new Color[resolution * resolution];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                colourMap[y * resolution + x] = Color.Lerp(Color.black, Color.white, _noiseMap[x, y]);
            }
        }

        Texture2D texture = new(resolution, resolution);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    Mesh CreateMesh(bool _useHeights, float[,] heightMap)
    {
        float topLeftX = (resolution - 1) / -2f;
        float topLeftZ = (resolution - 1) / 2f;

        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        Vector2[] uvs = new Vector2[resolution * resolution];

        int vertexIndex = 0;
        int triangleIndex = 0; 

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float thisHeight;

                if (_useHeights) thisHeight = heightMap[x, y] * heightMultiplier;
                else thisHeight = 0;

                vertices[vertexIndex] = new Vector3(topLeftX + x, thisHeight, topLeftZ - y);
                uvs[vertexIndex] = new Vector2(x / (float)resolution, y / (float)resolution);

                if (x < resolution - 1 && y < resolution - 1)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + resolution + 1;
                    triangles[triangleIndex + 2] = vertexIndex + resolution;

                    triangles[triangleIndex + 3] = vertexIndex + resolution + 1;
                    triangles[triangleIndex + 4] = vertexIndex;
                    triangles[triangleIndex + 5] = vertexIndex + 1;
                    
                    triangleIndex += 6;
                }

                vertexIndex++;
            }
        }

        Mesh mesh = new();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }

    void OnValidate()
    {
        if (resolution < 1)
        {
            resolution = 1;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }
}