using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarScript : MonoBehaviour
{
    public WorldSupervisor World;
    public BuildController BuildControllerScript;
    
    private Transform _highlightIcon;

    private float _offsetX = float.NegativeInfinity;

    private Vector2 _toolBarSize;
    
    private byte _activeSlot = 0;

    private readonly byte[] _blocksInSlots = new byte[10];
    
    // Start is called before the first frame update
    void Start()
    {
        Rect rect = transform.GetComponent<RectTransform>().rect;
        _toolBarSize = new Vector2(transform.position.x, transform.position.x + rect.width);
        
        Transform[] children = transform.GetComponentsInChildren<Transform>();

        byte blockIndex = 1;

        foreach (Transform child in children)
        {
            if (child.CompareTag("toolbarHighlight"))
            {
                _highlightIcon = child;
            }
            else if (child.CompareTag("slot"))
            {
                if (blockIndex < World.BlockTypes.Length &&
                    World.BlockTypes[blockIndex].IsVisible)
                {
                   Image image = child.GetComponentsInChildren<Image>()
                       .First(x => x.transform != child.transform);
                   image.sprite = World.BlockTypes[blockIndex].Icon;
                    if (float.IsNegativeInfinity(_offsetX))
                        _offsetX = child.GetComponent<RectTransform>().rect.width;
                    _blocksInSlots[_activeSlot] = blockIndex;
                }
                else
                {
                    _blocksInSlots[_activeSlot] = 0;
                }

                blockIndex++;
                _activeSlot++;
            }
        }

        _activeSlot = 0;
        BuildControllerScript.SelectedBlock = _blocksInSlots[0];
    }

    // Update is called once per frame
    void Update()
    {
        CheckScroll();
    }
    
    private void CheckScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll == 0.0f)
            return;
        
        Vector3 newPos = _highlightIcon.position;

        if (scroll < 0)
        {
            newPos.x += _offsetX;
            _activeSlot++;
            
            if (newPos.x >= _toolBarSize.y)
            {
                newPos.x  = _toolBarSize.x;
                _activeSlot = 0;
            }
            
            _highlightIcon.position = newPos;
        }
        else if (scroll > 0)
        {
            newPos.x -= _offsetX;
            _activeSlot--;
            
            if (newPos.x < _toolBarSize.x)
            {
                newPos.x  = _toolBarSize.y - _offsetX;
                _activeSlot = (byte) (_blocksInSlots.Length - 1);
            }
            
            _highlightIcon.position = newPos;
        }

        BuildControllerScript.SelectedBlock = _blocksInSlots[_activeSlot];
    }
}
