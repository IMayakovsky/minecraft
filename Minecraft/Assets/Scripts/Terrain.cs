using System.Collections.Generic;
using UnityEngine;

public class Terrain 
{
	public readonly bool IsBlocksTypesGenerated;
	public TerrainData TerrainData { get; set; }
	
	public GameObject TerrainObject { get; private set; }
	private MeshRenderer _meshRenderer;
	private MeshFilter _meshFilter;

	private int _blockFaceIndex;

	private readonly WorldSupervisor _worldSupervisor;
	private readonly List<Vector3> _vertices = new List<Vector3> ();
	private readonly List<int> _triangles = new List<int> ();
	private readonly List<Vector2> _uvs = new List<Vector2> ();
	
	private List<int> _transparentTriangles = new List<int>();
	private Material[] _materials = new Material[2];
	
    private Vector3 Position => TerrainObject.transform.position;

    public Terrain (TerrainData terrainData, WorldSupervisor worldSupervisor) 
    {
        TerrainData = terrainData;
        _worldSupervisor = worldSupervisor;
        IsBlocksTypesGenerated = true;
    }

    public void UpdateTerrainData()
    {
	    SetTerrain();
    }

    private void SetTerrain()
    {
	    TerrainObject.transform.position = new Vector3(TerrainData.Position.x * WorldSupervisor.TerrainWidth, 
		    0f, TerrainData.Position.y * WorldSupervisor.TerrainWidth);
	    TerrainObject.name = "Terrain " + TerrainData.Position.x  + ", " + TerrainData.Position.y;
	    
	    ClearTerrain();
	    
	    CreateBlocks();
    }

    public void SetModifications(Queue<TerrainModes> modes)
    {
	    foreach (TerrainModes mode in modes)
	    {
		    Vector3 pos = mode.Position;
		    TerrainData.BlocksTypes[(int)pos.x, (int)pos.y, (int)pos.z] = (byte)mode.BlockType;
	    }
	    
    }

    public void CreateTerrain () {

        TerrainObject = new GameObject();
        _meshFilter = TerrainObject.AddComponent<MeshFilter>();
        _meshRenderer = TerrainObject.AddComponent<MeshRenderer>();
        
        _materials[0] = _worldSupervisor.Material;
        _materials[1] = _worldSupervisor.TransparentMaterial;
        _meshRenderer.materials = _materials;
        
        TerrainObject.transform.SetParent(_worldSupervisor.transform);

        SetTerrain();
        
        MeshCollider collider = TerrainObject.AddComponent<MeshCollider>();
        
        collider.sharedMesh = _meshFilter.mesh;
    }

    private void ClearTerrain () 
    {
	    _blockFaceIndex = 0;
	    _vertices.Clear();
	    _triangles.Clear();
	    _transparentTriangles.Clear();
	    _uvs.Clear();
    }
    
    //todo update only neighbour
    public void UpdateBlock (Vector3 pos, byte newBlockType)
    {
	    Vector3Int blockPos = GetBlockLocalPosFromGlobalCoord(pos);

	    TerrainData.BlocksTypes[blockPos.x, blockPos.y, blockPos.z] = newBlockType;

	    UpdateSurroundingTerrains(blockPos);

	    CreateBlocks();

    }

    private void UpdateSurroundingTerrains (Vector3 blockPos)
    {
	    for (int p = 0; p < 6; p++) {

		    Vector3 curBlock = blockPos + BlockStructure.FaceDist[p];

		    if (!IsBlockInTerrain(curBlock)) 
		    {
			    _worldSupervisor.GetTerrainFromGlobalCoord(curBlock + Position).CreateBlocks();
		    }

	    }

    }
    
    private void CreateBlocks() 
	{
		ClearTerrain();
		
		for (int y = 0; y < WorldSupervisor.TerrainHeight; y++) {
			for (int x = 0; x < WorldSupervisor.TerrainWidth; x++) {
				for (int z = 0; z < WorldSupervisor.TerrainWidth; z++) 
				{
                    if (_worldSupervisor.BlockTypes[TerrainData.BlocksTypes[x,y,z]].IsVisible)
					    AddBlockToTerrain (new Vector3(x, y, z));
				}
			}
		}
		
		CreateMesh();
	}
	
    private bool IsBlockInTerrain (Vector3 pos)
    {
	    return pos.x >= 0 && pos.x < WorldSupervisor.TerrainWidth && 
	           pos.y >= 0 && pos.y < WorldSupervisor.TerrainHeight &&
	           pos.z >= 0 && pos.z < WorldSupervisor.TerrainWidth;
    }

    private bool IsBlockFaceVisible (Vector3 pos) 
    {
		int x = Mathf.FloorToInt (pos.x);
		int y = Mathf.FloorToInt (pos.y);
		int z = Mathf.FloorToInt (pos.z);
		
		if (!IsBlockInTerrain(new Vector3(x, y, z)))
			return false;

		BlockType block = _worldSupervisor.BlockTypes[TerrainData.BlocksTypes[x, y, z]];

        return block.IsVisible && !block.IsTransparent;
	}

    public byte GetBlockTypeFromGlobalCoord (Vector3 pos)
    {
	    Vector3Int blockPos = GetBlockLocalPosFromGlobalCoord(pos);

        return TerrainData.BlocksTypes[blockPos.x, blockPos.y, blockPos.z];
    }

    private Vector3Int GetBlockLocalPosFromGlobalCoord(Vector3 pos)
    {
	    int x = Mathf.FloorToInt(pos.x) - Mathf.FloorToInt(Position.x);
	    int y = Mathf.FloorToInt(pos.y);
	    int z = Mathf.FloorToInt(pos.z) - Mathf.FloorToInt(Position.z);

	    return new Vector3Int(x, y, z);
    }

    private void AddBlockToTerrain (Vector3 pos) 
    {
	    byte blockId = TerrainData.BlocksTypes[(int)pos.x, (int)pos.y, (int)pos.z];
	    bool isTransparent = _worldSupervisor.BlockTypes[blockId].IsTransparent;
	    
		for (int p = 0; p < 6; p++)
		{
			if (IsBlockFaceVisible(pos + BlockStructure.FaceDist[p])) 
				continue;

			for (int j = 0; j < 4; j++)
			{
				_vertices.Add (pos + BlockStructure.Vertices [BlockStructure.Faces [p, j]]);
			}

			AddTexture(_worldSupervisor.BlockTypes[blockId].GetTextureId(p));

			if (isTransparent)
			{
				_transparentTriangles.Add (_blockFaceIndex);
				_transparentTriangles.Add (_blockFaceIndex + 1);
				_transparentTriangles.Add (_blockFaceIndex + 2);
				_transparentTriangles.Add (_blockFaceIndex + 2);
				_transparentTriangles.Add (_blockFaceIndex + 1);
				_transparentTriangles.Add (_blockFaceIndex + 3);
			}
			else
			{
				_triangles.Add (_blockFaceIndex);
				_triangles.Add (_blockFaceIndex + 1);
				_triangles.Add (_blockFaceIndex + 2);
				_triangles.Add (_blockFaceIndex + 2);
				_triangles.Add (_blockFaceIndex + 1);
				_triangles.Add (_blockFaceIndex + 3);
			}
			
			_blockFaceIndex += 4;
		}
	}

    private void CreateMesh () 
    {
	    Mesh mesh = new Mesh
	    {
		    vertices = _vertices.ToArray(),
		    uv = _uvs.ToArray()
	    };
	    
	    mesh.subMeshCount = 2;
	    mesh.SetTriangles(_triangles.ToArray(), 0);
	    mesh.SetTriangles(_transparentTriangles.ToArray(), 1);

	    mesh.RecalculateNormals();

		_meshFilter.mesh = mesh;
	}

	private void AddTexture (TextureTypeEnum textureType) 
	{
        float y = (int)textureType / WorldSupervisor.TextureWidthInBlocks;
        float x = (int)textureType - (y * WorldSupervisor.TextureWidthInBlocks);

        x *= WorldSupervisor.NormalizedBlockTextureSize;
        y *= WorldSupervisor.NormalizedBlockTextureSize;

        y = 1f - y - WorldSupervisor.NormalizedBlockTextureSize;

        _uvs.Add(new Vector2(x, y));
        _uvs.Add(new Vector2(x, y + WorldSupervisor.NormalizedBlockTextureSize));
        _uvs.Add(new Vector2(x + WorldSupervisor.NormalizedBlockTextureSize, y));
        _uvs.Add(new Vector2(x + WorldSupervisor.NormalizedBlockTextureSize, y + WorldSupervisor.NormalizedBlockTextureSize));
    }

}