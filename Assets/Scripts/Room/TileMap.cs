using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public IList<uint> GetNeighbors(uint index, Func<Tile, bool> pred)
    {
        var list = new List<uint>(4);
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            if (TryGetNeighborIndex(index, dir, out var maybeNeighborIndex) && pred(mTiles[maybeNeighborIndex]))
            {
                list.Add(maybeNeighborIndex);
            }
        }

        return list;
    }

    public IList<uint> GetNeighbors(uint index, uint range, Func<Tile, bool> pred)
    {
        // BFS until out of range 
        ISet<uint> seen = new HashSet<uint>();
        Queue<(uint, uint)> Q = new Queue<(uint, uint)>(); 
        Q.Enqueue((index, range));

        BFSTileMap(ref seen, ref Q, pred, t => !t.IsWall);

        return seen.ToList();
    } 

    public void BFSTileMap(ref ISet<uint> tilesSeen, ref Queue<(uint, uint)> bfsQueue, 
        Func<Tile, bool> visitationPredicate = null, Func<Tile, bool> neighborPredicate = null)
    {
        // Stop if the queue is empty
        if (bfsQueue.Count == 0) return;

        var entry = bfsQueue.Dequeue();
        var index = entry.Item1;
        var range = entry.Item2;

        // Add the current index if it satisfies the visitation predicate 
        if (visitationPredicate != null && visitationPredicate(mTiles[index]))
        {
            tilesSeen.Add(index);
        }

        // Stop if the range is 0
        if (range == 0) return;

        // Add all neighbors that satisfy the neighbor predicate to the 
        var neighbors = GetNeighbors(index, neighborPredicate);
        foreach (var neighbor in neighbors)
        {
            if (!tilesSeen.Contains(neighbor) && neighborPredicate != null && neighborPredicate(mTiles[neighbor]))
            {
                bfsQueue.Enqueue((neighbor, range - 1));
            }

            tilesSeen.Add(neighbor);
        }

        // Recur
        BFSTileMap(ref tilesSeen, ref bfsQueue, visitationPredicate, neighborPredicate);
    }

    public void HandleMouseClick(Vector3 point)
    {
        var position = transform.InverseTransformPoint(point);
        var coordinates = position / GameUtils.TileSize;
        var index = (int)coordinates.x + (int)coordinates.z * GameUtils.Width;

        Debug.Log($"Index: {index}");
    }
}
