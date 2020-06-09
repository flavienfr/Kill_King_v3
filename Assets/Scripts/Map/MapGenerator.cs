using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;
using UnityEditor;

public class MapGenerator : MonoBehaviour
{
    public string title = "default";
    public enum PrintMapScene { CellularAutomatonTileGizmos, CellularAutomatonTile, Noise, None }
    public PrintMapScene printMapScene = PrintMapScene.None;

    [Min(1)]
    public int width = 1;
    [Min(1)]
    public int height = 1;
    public string seed;
    public bool useRandomSeed;

    //------------------- CellularAutomaton ---------------------
    public int cellularAutomatonRepetition = 4;
    [Range(0,100)]
    public int randomFillPercent;
    [Range(0, 1000)]
    public int fullThreasholdSize;
    [Range(0, 1000)]
    public int emptyThreasholdSize;

    public Tilemap botMap;
    public Tilemap topMap;
    public AnimatedTile botTile;
    public RuleTile topTile;
    //------------------------------------------------------------
    int[,] map;

    //-------------------------- Noise ---------------------------
    public float noiseScale;
    [Min(0)]
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    [Min(1)]
    public float lacunarity = 1;
    public Vector2 offset;
    //------------------------------------------------------------
    float[,] noiseMap;

    System.Random rng;

    public void GenerateMap()
    {
        if (useRandomSeed)
            seed = Time.time.ToString();
        rng = new System.Random(seed.GetHashCode());

        if (printMapScene == PrintMapScene.CellularAutomatonTile)
        {
            GenerateCellularAutomaton();
            PrintCellularAutomatonMapTile();
        }
        else if (printMapScene == PrintMapScene.CellularAutomatonTileGizmos)
            GenerateCellularAutomaton();
        else if (printMapScene == PrintMapScene.Noise)
            GenerateNoise();
    }

    void GenerateCellularAutomaton()
    {
        map = new int[width, height];

        RandomFillMap();
        for (int i = 0; i < cellularAutomatonRepetition; i++)
            CellularAutomaton();
        ProcessMapRegion();
    }

    void GenerateNoise()
    {
        noiseMap = Noise.GenerateNoiseMap(width, height, rng, noiseScale, octaves, persistance, lacunarity, offset);
    }

  

    public void ClearMap()
    {
        botMap.ClearAllTiles();
        topMap.ClearAllTiles();
    }

    public void SaveMap()
    {
        var mf = GameObject.Find("Grid");//change fird
        var savePath = "Assets/Maps/" + title + ".prefab";

        if (PrefabUtility.SaveAsPrefabAsset(mf, savePath)) 
        {
            EditorUtility.DisplayDialog("Tile map saved: ", "Path: " + savePath, "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Tile map not saved: ", "Path: " + savePath, "OK");
        }
    }



    void PrintCellularAutomatonMapTile()
    {
        int xpos = (int)transform.position.x;
        int ypos = (int)transform.position.y;

        ClearMap();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 0)
                {
                    topMap.SetTile(new Vector3Int(xpos - width / 2 + x, ypos - height / 2 + y, 0), topTile);
                    if (GetSurroundingWallCount(x, y) > 0)
                        botMap.SetTile(new Vector3Int(xpos - width / 2 + x, ypos - height / 2 + y, 0), botTile);
                }
                else
                    botMap.SetTile(new Vector3Int(xpos - width / 2 + x, ypos - height / 2 + y, 0), botTile);
            }
        }
    }

    void ProcessMapRegion()
    {
        List<List<Vector2Int>> fullRegions = GetRegions(1);

        foreach (List<Vector2Int> fullRegion in fullRegions)
            if (fullRegion.Count < fullThreasholdSize)
                RebuildMapRegion(fullRegion, 0);

        List<List<Vector2Int>> emptyRegions = GetRegions(0);

        foreach (List<Vector2Int> emptyRegion in emptyRegions)
            if (emptyRegion.Count < emptyThreasholdSize)
                RebuildMapRegion(emptyRegion, 1);
    }

    void RebuildMapRegion(List<Vector2Int> region, int newTile)
    {
        foreach (Vector2Int tile in region)
            map[tile.x, tile.y] = newTile;
    }

    List<Vector2Int> GetRegionTiles(int startX, int startY)//get all type of startx/y type of the region
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        int[,] mapFlags = new int[width, height];//1=is checked, 0=not checked
        int tileType = map[startX, startY];//is a wall, empty ?

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Vector2Int tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.x - 1; x <= tile.x + 1; x++)
            {
                for (int y = tile.y - 1; y <= tile.y + 1; y++)
                {
                    if (IsInMapRange(x, y) && (x == tile.x || y == tile.y))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Vector2Int(x, y));
                        }
                    }
                }

            }
        }
        return (tiles);
    }

    List<List<Vector2Int>> GetRegions(int tileType)
    {
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Vector2Int> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Vector2Int tile in newRegion)
                        mapFlags[tile.x, tile.y] = 1;
                }
            }
        }

        return (regions);
    }

    void RandomFillMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                /*if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    map[x, y] = 1;
                else*/
                    map[x, y] = (rng.Next(0, 100) < randomFillPercent) ? 1 : 0;
            }
        }
    }

    void CellularAutomaton()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                    map[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    map[x, y] = 0;
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;

        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                        wallCount += map[neighbourX, neighbourY];//with weight of the wall
                }
                else
                    wallCount++;//considere fin de map comme mur
            }
        }

        return (wallCount);
    }

    bool IsInMapRange(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    void OnDrawGizmosSelected()
    {
        if (printMapScene == PrintMapScene.CellularAutomatonTileGizmos && map != null)
        {
            int xpos = (int)transform.position.x;
            int ypos = (int)transform.position.y;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(xpos - width / 2 + x, ypos - height / 2 + y, 0);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
        else if (printMapScene == PrintMapScene.Noise && map != null)
        {
            int xpos = (int)transform.position.x;
            int ypos = (int)transform.position.y;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
                    Vector3 pos = new Vector3(xpos - width / 2 + x, ypos - height / 2 + y, 0);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}
