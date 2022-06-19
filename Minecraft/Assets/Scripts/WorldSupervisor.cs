using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldSupervisor : MonoBehaviour 
{
    public const int TerrainWidth = 16;
    public const int TerrainHeight = 128;
    private const int WORLD_WIDTH_IN_TERRAINS = 10;
    public static int WorldWidthInBlocks => WORLD_WIDTH_IN_TERRAINS * TerrainWidth;
    private const int VIEW_DISTANCE = 3;

    public const int TextureWidthInBlocks = 16;
    public static float NormalizedBlockTextureSize => 1f / TextureWidthInBlocks;
    
    public static WorldSupervisor Instance { get; private set; }
    private string _appPath;

    public BiomeInfo[] Biomes;
    public Transform Player;
    public Material Material;
    public Material TransparentMaterial;
    public BlockType[] BlockTypes;

    private readonly Terrain[] _terrains = new Terrain[VIEW_DISTANCE * VIEW_DISTANCE * 4];
    private TerrainPos _playerPos;
    private TerrainPos _playerLastPos;
    public WorldData WorldDataScript { get; private set; }
    
    private readonly List<int> _terrainsForCreating = new List<int>();
    private bool _isTerrainsCreatingProcessActive;
    private int _seed;
    private int _activeBiome;
    private AudioSource _audioSource;

    private Queue<TerrainModes> modifications = new Queue<TerrainModes>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            _appPath = Application.dataPath;
            WorldDataScript = new WorldData(_appPath);
            _audioSource = gameObject.GetComponent<AudioSource>();
        }
    }

    private void Start() 
    {
        Init();
    }

    private void Update() 
    {
        _playerPos = new TerrainPos(Player.position);

        if (!_playerPos.Equals(_playerLastPos))
        {
            CheckViewDistance();
            _playerLastPos = _playerPos;
        }

        if (_terrainsForCreating.Count > 0 && !_isTerrainsCreatingProcessActive)
            StartCoroutine(nameof(CreateTerrains));

        CheckInputs();
    }

    private void Init()
    {
        _activeBiome = Random.Range(0, 2);
        _seed = WorldDataScript.Seed;
        
        Player.position = WorldDataScript.PlayerPos;
        _playerLastPos = new TerrainPos(Player.position);
        
        GenerateWorld();
    }

    private void GenerateWorld ()
    {
        int terrainIndex = 0;
        TerrainPos playerPos = new TerrainPos(new Vector3(Player.position.x, 0f, Player.position.z));
        for (int x = playerPos.X - VIEW_DISTANCE; x < playerPos.X + VIEW_DISTANCE; x++) 
        {
            for (int z = playerPos.Z - VIEW_DISTANCE; z < playerPos.Z + VIEW_DISTANCE; z++)
            {
                TerrainData terrainData = WorldDataScript.GetTerrainData(
                    new Vector2Int(x, z), terrainIndex);
                
                if (!terrainData.IsGenerated)
                {
                    terrainData.GenerateBlocksTypes();
                    _terrains[terrainIndex] = new Terrain(terrainData, this);
                    _terrains[terrainIndex].SetModifications(modifications);
                    modifications.Clear();
                } 
                else 
                    _terrains[terrainIndex] = new Terrain(terrainData, this);
                
                _terrains[terrainIndex].CreateTerrain();
                terrainIndex++;
            }
        }
    }

    private IEnumerator CreateTerrains () 
    {
        _isTerrainsCreatingProcessActive = true;

        while (_terrainsForCreating.Count > 0)
        {
            Terrain terrain = _terrains[_terrainsForCreating[0]];
            if (!terrain.TerrainData.IsGenerated)
            {
                terrain.TerrainData.GenerateBlocksTypes();
                terrain.SetModifications(modifications);
                modifications.Clear();
            }
            terrain.UpdateTerrainData();
            _terrainsForCreating.RemoveAt(0);
            yield return null;
        }

        _isTerrainsCreatingProcessActive = false;
    }

    private void StartGame()
    {
        _terrainsForCreating.Clear();
        foreach (Terrain ter in _terrains)
        {
            Destroy(ter.TerrainObject);
        }
        Init();
    }

    private void CheckInputs()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            WorldDataScript.StartNewGame();
            StartGame();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            WorldDataScript.SaveWorld(Player.position);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            WorldDataScript.LoadWorld();
            StartGame();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (_audioSource.isPlaying)
                _audioSource.Stop();
            else
                _audioSource.Play();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void CheckViewDistance ()
    {
        int[] freeIndexes = new int[_terrains.Length];
        int freeIdx = 0;
        TerrainPos playerPos = new TerrainPos(new Vector3(Player.position.x, 0f, Player.position.z));

        for (int i = 0; i < _terrains.Length; i++)
        {
            Vector2Int pos = _terrains[i].TerrainData.Position;
            if (playerPos.X - VIEW_DISTANCE <= pos.x && playerPos.X + VIEW_DISTANCE > pos.x &&
                playerPos.Z - VIEW_DISTANCE <= pos.y && playerPos.Z + VIEW_DISTANCE > pos.y)
            {
                continue;
            }
            freeIndexes[freeIdx++] = i;
        }

        freeIdx = 0;
        
        for (int x = playerPos.X - VIEW_DISTANCE; x < playerPos.X + VIEW_DISTANCE; x++) 
        {
            for (int z = playerPos.Z - VIEW_DISTANCE; z < playerPos.Z + VIEW_DISTANCE; z++)
            {
                Vector2Int coord = new Vector2Int(x, z);
                int idx = WorldDataScript.GetTerrainIndex(coord);
                if (idx != -1) 
                    continue;

                idx = freeIndexes[freeIdx++];
                
                WorldDataScript.DeactivateTerrain(_terrains[idx].TerrainData.Position);
                _terrains[idx].TerrainData = WorldDataScript.GetTerrainData(coord, idx);
                _terrainsForCreating.Add(idx);

            }
        }
    }
    
    public Terrain GetTerrainFromGlobalCoord (Vector3 pos)
    {
        TerrainPos terrainPos = new TerrainPos(pos);
        int index = WorldDataScript.GetTerrainIndex(new Vector2Int(terrainPos.X, terrainPos.Z));
        return index < 0 ? null : _terrains[index];
    }

    public bool CheckBlockCollision (Vector3 pos) 
    {
        if (pos.y < 0 || pos.y > TerrainHeight - 1 || _terrains == null)
            return false;

        Terrain terrain = GetTerrainFromGlobalCoord(pos);

        if (terrain != null && terrain.IsBlocksTypesGenerated)
            return BlockTypes[terrain.GetBlockTypeFromGlobalCoord(pos)].IsVisible;

        return false;
    }

    public BlockTypeEnum GetBlockType (Vector3Int blockPos, Vector2Int terrainPos) 
    {
        if (blockPos.y == 0)
            return BlockTypeEnum.Bedrock;
        
        Vector2 realPos = new Vector3(blockPos.x + terrainPos.x * TerrainWidth, blockPos.z + terrainPos.y * TerrainWidth);
        
        int placeHeight = (int) (Biomes[_activeBiome].TerrainHeight * 
            NoiseGenerator.GetPerlinNoise(new Vector2(realPos.x, realPos.y), _seed, 
                Biomes[_activeBiome].TerrainScale) + Biomes[_activeBiome].SolidGroundHeight);
        
        BlockTypeEnum blockType;

        if (blockPos.y == placeHeight)
            blockType = Biomes[_activeBiome].SurfaceBlock;
        else if (blockPos.y < placeHeight && blockPos.y > placeHeight - 4)
            blockType = Biomes[_activeBiome].SubSurfaceBlock;
        else if (blockPos.y > placeHeight)
            blockType = BlockTypeEnum.Air;
        else
            blockType = Biomes[_activeBiome].MiddleBlock;
        

        if (blockType == Biomes[_activeBiome].MiddleBlock) 
        {
            foreach (Lode lode in Biomes[0].Lodes)
            {
                if (blockPos.y <= lode.MinHeight || blockPos.y >= lode.MaxHeight) 
                    continue;
                if (NoiseGenerator.IsHereLode(blockPos, lode.NoiseOffset, lode.Scale, lode.Frequency))
                    blockType = lode.BlockType;
            }
        }

        if (blockPos.y == placeHeight)
        {
            BiomeInfo biome = Biomes[_activeBiome];
            
            if (NoiseGenerator.GetPerlinNoise(new Vector2(realPos.x, realPos.y), 
                _seed, biome.TreeZoneScale) > biome.TreeZoneThreshold)
            {
                if (NoiseGenerator.GetPerlinNoise(new Vector2(realPos.x, realPos.y), _seed,
                    biome.TreePlacementScale) > biome.TreePlacementThreshold) {

                    blockType = BlockTypeEnum.Dirt;
                    TreeGenerator.MakeTree(blockPos, modifications, biome.MinTreeHeight, biome.MaxTreeHeight);
                }
                
            }
        }

        return blockType;
    }
    

}