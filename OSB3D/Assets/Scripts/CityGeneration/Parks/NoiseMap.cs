using UnityEngine;
using System.Collections;

public static class NoiseMap
{

    public static float[,] GenerateNoise(int _resolution, int _seed, float _scale, int _octaves, float _persistance, float _lacunarity, Vector2 _offset)
    {
        float[,] noise = new float[_resolution, _resolution];

        System.Random prng = new System.Random(_seed);
        Vector2[] octaveOffsets = new Vector2[_octaves];
        for (int i = 0; i < _octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + _offset.x;
            float offsetY = prng.Next(-100000, 100000) + _offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (_scale <= 0)
        {
            _scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfSize = _resolution / 2f;

        for (int y = 0; y < _resolution; y++)
        {
            for (int x = 0; x < _resolution; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < _octaves; i++)
                {
                    float sampleX = (x - halfSize) / _scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfSize) / _scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= _persistance;
                    frequency *= _lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noise[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < _resolution; y++)
        {
            for (int x = 0; x < _resolution; x++)
            {
                noise[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noise[x, y]);
            }
        }

        return noise;
    }

}