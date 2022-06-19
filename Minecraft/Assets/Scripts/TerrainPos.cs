using UnityEngine;

public class TerrainPos 
{
    public readonly int X;
    public readonly int Z;

    public TerrainPos (Vector3 blockPos) 
    {
        X = Mathf.FloorToInt(blockPos.x / WorldSupervisor.TerrainWidth);
        Z = Mathf.FloorToInt(blockPos.z / WorldSupervisor.TerrainWidth);
    }

    public bool Equals (TerrainPos other)
    {
        if (other == null)
            return false;
        return (other.X == X && other.Z == Z);
    }
}