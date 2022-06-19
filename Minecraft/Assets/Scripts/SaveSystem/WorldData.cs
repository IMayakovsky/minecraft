using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[HideInInspector]
[System.Serializable]
public class WorldData
{
    public static readonly string WorldName = "MyWorld";
    public int Seed { get; private set; }

    private float _x, _y, _z;

    [System.NonSerialized] private bool _isLoadModeActive;

    public Vector3 PlayerPos
    {
        get => new Vector3(_x, _y, _z);
        set
        {
            _x = value.x;
            _y = value.y;
            _z = value.z;
        }
    }

    [System.NonSerialized]
    public Dictionary<Vector2Int, TerrainData> Terrains = new Dictionary<Vector2Int, TerrainData>();
    
    [System.NonSerialized]
    private SaveSystem _saveSystem;

    public WorldData(string appPath)
    {
        _saveSystem = new SaveSystem(appPath);
        LoadWorld();
    }

    public TerrainData GetTerrainData(Vector2Int coord, int terrainIndex) 
    {
        TerrainData terrainData;

        if (Terrains.ContainsKey(coord))
            terrainData = Terrains[coord];
        else 
        { 
            LoadTerrain(coord, terrainIndex);
            terrainData = Terrains[coord];
        }
        
        Terrains[coord].TerrainIndex = terrainIndex;

        return terrainData;
    }

    public int DeactivateTerrain(Vector2Int coord)
    {
        try
        {
            Terrains[coord].TerrainIndex = -1;
            return Terrains[coord].TerrainIndex;
        }
        catch (Exception)
        {
            return -1;
        }
    }

    public int GetTerrainIndex(Vector2Int coord)
    {
        try
        {
            return Terrains[coord].TerrainIndex;
        }
        catch (Exception)
        {
            return -1;
        }
    }

    public void LoadTerrain (Vector2Int coord, int terrainIndex) 
    {
        if (Terrains.ContainsKey(coord))
            return;

        if (_isLoadModeActive)
        {
            TerrainData terrainData = _saveSystem.LoadTerrain(coord);
            if (terrainData != null)
            {
                terrainData.IsGenerated = true;
                terrainData.TerrainIndex = terrainIndex;
                Terrains.Add(coord, terrainData);
                return;
            }
        }
        
        Terrains.Add(coord, new TerrainData(coord, terrainIndex));
        
    }

    public bool IsBlockInWorld (Vector3 pos)
    {
        return pos.x >= 0 && pos.x < WorldSupervisor.WorldWidthInBlocks && pos.y >= 0 
               && pos.y < WorldSupervisor.TerrainHeight && pos.z >= 0 && pos.z < WorldSupervisor.WorldWidthInBlocks;
    }
    
    public void UpdateBlockInTerrain(TerrainData terrainData, Vector3Int blockPos, byte value) 
    {
        try
        {
            terrainData.BlocksTypes[blockPos.x, blockPos.y, blockPos.z] = value;
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public void LoadWorld()
    {
        _isLoadModeActive = true;
        WorldData worldData = _saveSystem.LoadWorld();
        if (worldData == null)
        {
            StartNewGame();
        }
        else
        {
            Seed = worldData.Seed;
            PlayerPos = worldData.PlayerPos;
            Terrains.Clear();
        }
    }

    public void StartNewGame()
    {
        Seed = Random.Range(1000000, 9999999);
        PlayerPos = new Vector3(0,WorldSupervisor.TerrainHeight, 0);
        Terrains.Clear();
        _isLoadModeActive = false;
    }

    public void SaveWorld(Vector3 playerPos)
    {
        PlayerPos = playerPos;
        _saveSystem.SaveWorld(this);
    }
}