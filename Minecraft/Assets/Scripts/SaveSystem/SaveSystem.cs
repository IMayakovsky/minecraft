using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

public class SaveSystem
{
    private static string _savePath;

    public SaveSystem(string appPath)
    { 
        _savePath = appPath + "/saves/";
        if (!Directory.Exists(_savePath))
            Directory.CreateDirectory(_savePath);
    }

    public void SaveWorld (WorldData worldData) 
     {
        string savePath = _savePath + WorldData.WorldName + "/";
        
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + "world.world", FileMode.Create);

        formatter.Serialize(stream, worldData);
        stream.Close();

        Thread thread = new Thread(() => SaveTerrains(worldData.Terrains.Values.ToList()));
        thread.Start();

    }

    public void SaveTerrains (List<TerrainData> terrains) 
    {
        foreach (TerrainData terrain in terrains) 
        {
            SaveTerrain(terrain);
        }

    }

    public WorldData LoadWorld()
    {
        WorldData worldData = null;

        string path = _savePath + WorldData.WorldName + "/world.world";
        
        if (File.Exists(path)) 
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            
            worldData = formatter.Deserialize(stream) as WorldData;
            stream.Close();
        }

        return worldData;
    }

    public void SaveTerrain (TerrainData terrain) 
    {
        string terrainName = terrain.Position.x + "-" + terrain.Position.y;
        
        string savePath = _savePath + WorldData.WorldName + "/terrains/";
        
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + terrainName + ".terrain", FileMode.Create);

        formatter.Serialize(stream, terrain);
        stream.Close();
    }

    public TerrainData LoadTerrain (Vector2Int position) 
    {
        string terrainName = position.x + "-" + position.y;
        
        string loadPath = _savePath + WorldData.WorldName + "/terrains/" + terrainName + ".terrain";
        
        if (File.Exists(loadPath)) 
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath, FileMode.Open);

            TerrainData terrainData = formatter.Deserialize(stream) as TerrainData;
            stream.Close();
            
            if (terrainData != null)
                return terrainData;

        }

        return null;

    }
}