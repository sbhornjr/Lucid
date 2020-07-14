using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    private Tile[] mTiles;

    private RoomGridGeneration mRoomGridGeneration;

    private Type test;

    // Start is called before the first frame update
    void Awake()
    {
        mRoomGridGeneration = GetComponent<RoomGridGeneration>();
        // mTiles = mRoomGridGeneration.GenerateRoom();
    }

    public void GenerateRoom()
    { 
        mTiles = mRoomGridGeneration.GenerateRoom();
    }

    public bool TryMoveToNeighbor(uint index, Direction direction, out uint maybeNextIndex)
    {
        if (!TryGetNeighborIndex(index, direction, out maybeNextIndex))
        {
            return false;
        }

        return !mTiles[maybeNextIndex].IsWall;
    }

    public bool TryGetNeighborIndex(uint index, Direction direction, out uint maybeNeighborIndex)
    {
        return mTiles[index].TryGetNeighborIndex(direction, out maybeNeighborIndex);
    }

    public Vector3 GetCenterPositionOfTileAt(uint index)
    {
        return mTiles[index].Center;
    }

    internal void HandleMouseClick(Vector3 point)
    {
        var position = transform.InverseTransformPoint(point);
        var coordinates = position / GameUtils.TileSize;
        var index = (int)coordinates.x + (int)coordinates.z * GameUtils.Width;

        Debug.Log($"Index: {index}");
    }
}
