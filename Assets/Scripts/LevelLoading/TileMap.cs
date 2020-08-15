using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileMap
{
    private Tile[] mTiles; 

    public int NumTiles { get { return mTiles.Length; } }

    public RoomTemplate GenerateRoom(string templateFileName, bool[] doors, bool hasTreasure, bool hasPOI)
    {
        var roomTemplate = RoomGridGeneration.ReadRoomTemplateFromFile(templateFileName);

        mTiles = RoomGridGeneration.GenerateRoom(roomTemplate, doors, hasTreasure, hasPOI);

        return roomTemplate;
    }

    public bool TryMoveToNeighbor(uint index, Direction direction, out uint maybeNextIndex)
    {
        if (!TryGetNeighborIndex(index, direction, out maybeNextIndex))
        {
            return false;
        }

        var type = mTiles[maybeNextIndex].Type;
        return type != TileType.Wall && type != TileType.Treasure && type != TileType.POI;
    }

    public bool TryGetNeighborIndex(uint index, Direction direction, out uint maybeNeighborIndex)
    {
        return mTiles[index].TryGetNeighborIndex(direction, out maybeNeighborIndex);
    }

    public Vector3 GetCenterPositionOfTileAt(uint index)
    {
        return mTiles[index].Center;
    }

    public uint GetIndexOfTileAt(Vector3 position)
    {
        // Unscale the x and z 
        uint x = (uint)(position.x / GameUtils.TileSize);
        uint z = (uint)(position.z / GameUtils.TileSize);

        // Get the index
        return GameUtils.GetIndex(x, z);
    }
    
    /// <summary>
    /// Returns whether the tile at the given index is interactable.
    /// </summary> 
    internal bool IsTileInteractable(uint maybeNeighborIndex)
    {
        if (maybeNeighborIndex < 0 || maybeNeighborIndex >= mTiles.Length) return false;

        return mTiles[maybeNeighborIndex].Type.IsInteractable(); 
    }

    public IList<uint> GetNeighbors(uint index)
    {
        var list = new List<uint>(4);
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            if (TryGetNeighborIndex(index, dir, out var maybeNeighborIndex))
            {
                list.Add(maybeNeighborIndex);
            }
        }

        return list;
    }
      
    public IList<uint> GetNeighbors(uint index, uint range, Func<Tile, bool> includeTilePred)
    { 
        // BFS until out of range 
        ISet<uint> seen = new HashSet<uint>();
        Queue<(uint, uint)> Q = new Queue<(uint, uint)>();
        Q.Enqueue((index, range));
        BFS(ref seen, ref Q, t => !t.IsWall);

        // Parse out any tiles that do not pass the inclusion predicate
        return seen.Where(t => includeTilePred(mTiles[t])).ToList();
    }  

    private void BFS(ref ISet<uint> tilesSeen, ref Queue<(uint, uint)> bfsQueue, 
        Func<Tile, bool> neighborPredicate = null)
    {
        // Empty the queue
        while (bfsQueue.Count > 0)
        {
            var entry = bfsQueue.Dequeue();
            var tile = entry.Item1;
            var range = entry.Item2;

            // Stop if this tile has already been explored
            if (tilesSeen.Contains(tile)) continue;

            tilesSeen.Add(tile);

            // Stop if the range is 0
            if (range == 0) continue;

            // Otherwise, search neighbors according to the predicate
            foreach (var neighbor in GetNeighbors(tile))
            {
                if ((neighborPredicate == null || neighborPredicate(mTiles[tile]))
                    && !tilesSeen.Contains(neighbor))
                {
                    bfsQueue.Enqueue((neighbor, range - 1));
                }
            }
        }
    } 

    public Tile[] GetTiles()
    {
        return mTiles;
    }

    public TileType GetTileType(uint index)
    {
        return mTiles[index].Type;
    }

    internal uint GetIndexOfDoor(Direction direction)
    {
        // !!! inefficient !!!
        return mTiles.First(t => t.IsDoor && t.DoorDirection == direction).Index;
    }
}
