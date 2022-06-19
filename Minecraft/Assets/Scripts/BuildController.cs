using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildController : MonoBehaviour
{

    [SerializeField]
    private Transform HighlightBlock = null;
    [SerializeField]
    private Transform MainCamera = null;
    [SerializeField]
    private WorldSupervisor WorldSupervisorScript = null;

    public byte SelectedBlock { private get; set; }

    private Vector3 _newBlockPlacePos;
    private const float VIEW_DISTANCE_INTERVAL = 0.1f;
    private const float VIEW_DIST = 8f;
    private string _selectedBlockName;
    private int _highlightBlockStrength = -1;
    private DateTime _lastTime;
    private bool _highlightBlockWasChanged;

    private void Start()
    {
        _lastTime = DateTime.Now;
    }

    // Update is called once per frame
    private void Update()
    {
        CursorBlock();

        if (!HighlightBlock.gameObject.activeSelf)
        {
            return;
        }

        if (_highlightBlockWasChanged)
        {
            _highlightBlockWasChanged = false;
            _highlightBlockStrength = -1;
        }

        DateTime time = _lastTime;
        
        if (Input.GetMouseButton(0) && time.AddMilliseconds(400) < System.DateTime.Now)
        {
            _lastTime = DateTime.Now;
            
            Terrain terrain = WorldSupervisorScript.GetTerrainFromGlobalCoord(HighlightBlock.position);
            
            if (_highlightBlockStrength == -1)
            {
                int idx = terrain.GetBlockTypeFromGlobalCoord(HighlightBlock.position);
                _highlightBlockStrength = WorldSupervisorScript.BlockTypes[idx].Strength - 1;
            }

            if (_highlightBlockStrength == 0)
            {
                terrain.UpdateBlock(HighlightBlock.position, 0);
                Vector3Int vecInt = new Vector3Int((int) HighlightBlock.position.x,
                    (int) HighlightBlock.position.y, (int) HighlightBlock.position.z);
                WorldSupervisorScript.WorldDataScript.UpdateBlockInTerrain(terrain.TerrainData, vecInt, 0);
            }
            else
            {
                _highlightBlockStrength--;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _highlightBlockStrength = -1;
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 playerExactPos = WorldSupervisorScript.Player.position;
            Vector3Int playerPos = new Vector3Int(Mathf.FloorToInt(playerExactPos.x), 
                Mathf.FloorToInt(playerExactPos.y), Mathf.FloorToInt(playerExactPos.z));
            Vector3Int blockPos = new Vector3Int(Mathf.FloorToInt(_newBlockPlacePos.x), 
                Mathf.FloorToInt(_newBlockPlacePos.y), Mathf.FloorToInt(_newBlockPlacePos.z));
            if (playerPos.x == blockPos.x && 
                (playerPos.y == blockPos.y || playerPos.y + 1 == blockPos.y) && 
                playerPos.z == blockPos.z )
            {
                return;
            }
            Terrain terrain = WorldSupervisorScript.GetTerrainFromGlobalCoord(_newBlockPlacePos);
            terrain.UpdateBlock(_newBlockPlacePos, SelectedBlock);
            Vector3Int vecInt = new Vector3Int((int)_newBlockPlacePos.x, 
                (int)_newBlockPlacePos.y, (int)_newBlockPlacePos.z);
            WorldSupervisorScript.WorldDataScript.UpdateBlockInTerrain(terrain.TerrainData, vecInt, SelectedBlock);
        }
    }
    
    private void CursorBlock()
    {
        float interval = VIEW_DISTANCE_INTERVAL;
        Vector3 lastCameraViewPos = new Vector3();

        while (interval < VIEW_DIST)
        {
            Vector3 cameraViewPos = MainCamera.position + (MainCamera.forward * interval);
            Vector3 floorCameraViewPos = new Vector3(Mathf.FloorToInt(cameraViewPos.x), 
                Mathf.FloorToInt(cameraViewPos.y), Mathf.FloorToInt(cameraViewPos.z));

            if (WorldSupervisorScript.CheckBlockCollision(cameraViewPos))
            {
                if (HighlightBlock.position != floorCameraViewPos)
                    _highlightBlockWasChanged = true;
                HighlightBlock.position = floorCameraViewPos;
                _newBlockPlacePos = lastCameraViewPos;
                HighlightBlock.gameObject.SetActive(true);
                return;
            }

            lastCameraViewPos = floorCameraViewPos;
            interval += VIEW_DISTANCE_INTERVAL;
        }
        
        HighlightBlock.gameObject.SetActive(false);
    }

}
