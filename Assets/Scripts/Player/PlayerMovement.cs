using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private uint startingIndex;


    private TileMap mTileMap;

    private uint mIndex;

    private void Awake()
    {
        mTileMap = FindObjectOfType<TileMap>();
    }
     
    public void InitPosition()
    {
        mIndex = startingIndex;

        transform.position = mTileMap.GetCenterPositionOfTileAt(mIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.W))
        {
            HandleKey(Direction.S);
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            HandleKey(Direction.W);
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            HandleKey(Direction.N);
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            HandleKey(Direction.E);
        } 
    }

    private void HandleKey(Direction direction)
    {
        if (mTileMap.TryMoveToNeighbor(mIndex, direction, out var nextIndex))
        {
            mIndex = nextIndex; 
            transform.position = mTileMap.GetCenterPositionOfTileAt(mIndex);
        }
    } 
}
