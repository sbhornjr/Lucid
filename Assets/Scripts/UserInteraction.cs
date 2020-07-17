using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UserInteraction : MonoBehaviour
{
    private RoomGridGeneration mRoomGridGeneration;
    private TileMap mTileMap;

    // Start is called before the first frame update
    void Start()
    {
        mRoomGridGeneration = GetComponent<RoomGridGeneration>();
        mTileMap = GetComponent<TileMap>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            mRoomGridGeneration.GenerateRoom();
        }

        if (Input.GetMouseButtonUp(0))
        { 
            HandleMouseClick(Input.mousePosition);
        }
    }

    private void HandleMouseClick(Vector3 mousePosition)
    {
        // Raycast the click 
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            mTileMap.HandleMouseClick(hit.point);
        }
        else
        {
            Debug.Log("Mouse click missed.");
        }
    }
}
