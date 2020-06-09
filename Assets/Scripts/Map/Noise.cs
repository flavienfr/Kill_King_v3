using System;
using System.Collections;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int widht, int height, System.Random rng, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[widht, height];
        float minNoiseHeight = float.MaxValue;
        float maxNoiseHeight = float.MinValue;

        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = rng.Next(-100000, 100000) + offset.x;
            float offsetY = rng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
            scale = 0.001f;
        for (int x = 0; x < widht; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - (widht / 2f)) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - (widht / 2f)) / scale * frequency + octaveOffsets[i].y;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    noseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                if (noseHeight > maxNoiseHeight)
                    maxNoiseHeight = noseHeight;
                else if (noseHeight < minNoiseHeight)
                    minNoiseHeight = noseHeight;

                noiseMap[x, y] = noseHeight;
            }
        }
        for (int x = 0; x < widht; x++)
        {
            for (int y = 0; y < height; y++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        return (noiseMap);
    }
}
