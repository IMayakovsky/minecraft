using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class TerrainData
{
    private int _x;
    private int _y;
    
    [System.NonSerialized]
    public int TerrainIndex;

    public bool IsGenerated;
    
    public Vector2Int Position {

        get => new Vector2Int(_x, _y);
        set {
            _x = value.x;
            _y = value.y;
        }
    }

    [HideInInspector]
    public byte[,,] BlocksTypes = new byte[WorldSupervisor.TerrainWidth, 
        WorldSupervisor.TerrainHeight, WorldSupervisor.TerrainWidth];
    
    public TerrainData(Vector2Int pos, int terrainIndex)
    {
        Position = pos;
        TerrainIndex = terrainIndex;
    }

    public void GenerateBlocksTypes () 
    {
        for (int y = 0; y < WorldSupervisor.TerrainHeight; y++) {
            for (int x = 0; x < WorldSupervisor.TerrainWidth; x++) {
                for (int z = 0; z < WorldSupervisor.TerrainWidth; z++) 
                {
                    BlocksTypes[x, y, z] = (byte)WorldSupervisor.Instance.GetBlockType(
                        new Vector3Int(x, y, z), Position);
                }
            }
        }

        IsGenerated = true;
    }
    
}