using UnityEngine;

[CreateAssetMenu(fileName = "BiomeInfo", menuName = "Biomes/BiomeInfo")]
public class BiomeInfo : ScriptableObject {

    public string BiomeName;
    public int SolidGroundHeight;
    public int TerrainHeight;
    public float TerrainScale;
    public BlockTypeEnum SurfaceBlock;
    public BlockTypeEnum SubSurfaceBlock;
    public BlockTypeEnum MiddleBlock;

    [Header("Trees")] 
    public float TreeZoneScale = 1.3f;
    [Range(0.1f, 1)]
    public float TreeZoneThreshold = 0.6f;
    public float TreePlacementScale =  15f;
    [Range(0.1f, 1)]
    public float TreePlacementThreshold = 0.8f;
    public int MaxTreeHeight = 12;
    public int MinTreeHeight = 5;
    
    public Lode[] Lodes;

}