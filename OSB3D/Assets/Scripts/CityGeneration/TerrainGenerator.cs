using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    enum Resolution { Lowest = 32, Low = 64, MidLow = 128, MidHigh = 256, High = 512, Highest = 1024 };
    [SerializeField] Resolution resolution;

    [SerializeField] float scale;
    [SerializeField] float minHeight;
    [SerializeField] float maxHeight;
    [SerializeField] float edgeDistance;

    [SerializeField] Material material;

    public GameObject Generate(int _blockSize)
    {
        GameObject ground = new GameObject("Ground");
        GameObject terrainObject = new GameObject("Terrain");
        Terrain terrain = terrainObject.AddComponent<Terrain>();

        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);


        int res = (int)resolution;

        float[,] perlinNoise = CalculateNoise(res, offsetX, offsetY);

        float[,] falloffMap = new float[res, res];
        Gradient gradient = EdgeGradient();


            for (int i = 0; i < res; i++)
            {
                for (int j = 0; j < res; j++)
                {

                    float x = i / (float)res * 2 - 1;
                    float y = j / (float)res * 2 - 1;
                    float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                    Color colour = gradient.Evaluate(value);
                    falloffMap[i, j] = 1 - colour.b;

                    perlinNoise[i, j] *= falloffMap[i, j];

                }
            }

        terrain.terrainData = CalculateTerrain(perlinNoise, res, _blockSize);

        terrain.materialTemplate = material;

        terrainObject.transform.position = new Vector3(_blockSize/2*(-1), 0, _blockSize/2*(-1));
        terrain.transform.parent = ground.transform;

        return ground;
    }

    Gradient EdgeGradient()
    {
        Gradient gradient = new ();

        GradientColorKey[] colourKeys = new GradientColorKey[3];
        colourKeys[0] = new GradientColorKey(Color.black, 0.0f);
        colourKeys[1] = new GradientColorKey(Color.black, 1.0f - edgeDistance);
        colourKeys[2] = new GradientColorKey(Color.white, 1.0f);

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 1.0f), new GradientAlphaKey(1.0f, 1.0f) };

        gradient.SetKeys(colourKeys, alphaKeys);

        return gradient;
    }

    TerrainData CalculateTerrain(float[,] _noise, int _res, int _blockSize)
    {
        TerrainData terrainData = new();

        terrainData.heightmapResolution = _res + 1;
        terrainData.SetHeights(0, 0, _noise);
        terrainData.size = new Vector3(_blockSize, maxHeight, _blockSize);

        return terrainData;
    }

    float[,] CalculateNoise(int _res, float _offsetX, float _offsetY)
    {
        float[,] noise = new float[_res, _res];

        for (int x = 1; x < _res; x++)
        {
            for (int y = 1; y < _res; y++)
            {
                 float xCoord = (float)x / _res * scale + _offsetX;
                float yCoord = (float)y / _res * scale + _offsetY;

                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                noise[x, y] = sample;   
            }
        }

        return noise; 
    }

    private void OnValidate()
    {
        if (scale < 0.5) scale = 0.5f;
        if (minHeight < 0) minHeight = 0;
        if (maxHeight < 0.5) maxHeight = 0.5f;
        if (edgeDistance < 0) edgeDistance = 0;
        if (edgeDistance > 1) edgeDistance = 1;
    }
}
