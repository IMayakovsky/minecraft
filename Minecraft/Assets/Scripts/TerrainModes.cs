using UnityEngine;

public class TerrainModes {

    public Vector3 Position;
    public readonly BlockTypeEnum BlockType;

    public TerrainModes (Vector3 position, BlockTypeEnum blockType) 
    {
        Position = position;
        BlockType = blockType;
    }

}