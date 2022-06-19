using System.Collections.Generic;
using UnityEngine;

public static class TreeGenerator
{
    public static void MakeTree (Vector3 position, Queue<TerrainModes> queue, int minTrunkHeight, int maxTrunkHeight)
    {
        int height = (int)(maxTrunkHeight * NoiseGenerator.GetPerlinNoise(new Vector2(position.x, 
            position.z), 250f, 3f));
        
        if (position.x - 2 < 0 || position.z - 2 < 0 || position.x + 3 > WorldSupervisor.TerrainWidth 
            || position.z + 3 >= WorldSupervisor.TerrainWidth || position.y + height + 4 >= WorldSupervisor.TerrainHeight)
            return;

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i < height; i++)
            queue.Enqueue(new TerrainModes(new Vector3(position.x, position.y + i, position.z), BlockTypeEnum.Wood));

        for (int x = -2; x < 3; x++) {
            for (int y = 0; y < 4; y++) {
                for (int z = -2; z < 3; z++) {
                    queue.Enqueue(new TerrainModes(new Vector3(position.x + x, 
                        position.y + height + y, position.z + z), BlockTypeEnum.Leaves));
                }
            }
        }

    }
}