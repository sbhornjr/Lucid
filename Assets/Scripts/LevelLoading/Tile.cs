using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tiles are STATELESS: they do not contain any information about opened chests, enemy positions,
/// interaction statuses of POIs, etc!
/// </summary>
public class Tile
{ 
    private uint _Index;
    public uint Index
    {
        get { return _Index; }
        private set
        {
            _Index = value;
            CalculateLocations();
        }
    }

    public uint X { get { return GameUtils.GetX(_Index); } }
    public uint Z { get { return GameUtils.GetZ(_Index); } }

    public Vector3 TopLeft { get; private set; }
    public Vector3 TopRight { get; private set; }
    public Vector3 BotLeft { get; private set; }
    public Vector3 BotRight { get; private set; }
    public Vector3 Center { get; private set; }

    public TileType Type { get; set; }

    public bool IsWall { get { return Type == TileType.Wall; } set { Type = value ? TileType.Wall : Type; } }

    public bool IsEnemyNest { get { return Type == TileType.Nest; } set { Type = value ? TileType.Nest : Type; } }

    public bool IsTreasure { get { return Type == TileType.Treasure; } set { Type = value ? TileType.Treasure : Type; } }

    public bool IsNone { get { return Type == TileType.None; } set { Type = value ? TileType.None : Type; } }

    public bool IsDoor { get { return Type == TileType.Door; } set { Type = value ? TileType.Door : Type; } }

    public bool IsPOI { get { return Type == TileType.POI; } set { Type = value ? TileType.POI : Type; } }

    public bool IsPortal { get { return Type == TileType.Portal; } set { Type = value ? TileType.Portal : Type; } }

    /// <summary>
    /// This has no meaning if IsDoor is false!
    /// </summary>
    public Direction DoorDirection { get; set; }

    public POIType POIType { get; set; }

    public Tile(uint index)
    {
        Index = index;
    }

    private void CalculateLocations()
    {
        TopLeft = new Vector3(X * GameUtils.TileSize, 0f, Z * GameUtils.TileSize);
        TopRight = new Vector3((X + 1) * GameUtils.TileSize, 0f, Z * GameUtils.TileSize);
        BotLeft = new Vector3(X * GameUtils.TileSize, 0f, (Z + 1) * GameUtils.TileSize);
        BotRight = new Vector3((X + 1) * GameUtils.TileSize, 0f, (Z + 1) * GameUtils.TileSize);
        Center = new Vector3(X * GameUtils.TileSize + GameUtils.TileSize / 2, 0f, Z * GameUtils.TileSize + GameUtils.TileSize / 2);
    }
     
    internal bool TryGetNeighborIndex(Direction direction, out uint index)
    {
        switch (direction)
        {
            case Direction.N:
                if (Z > 0)
                {
                    index = Index - GameUtils.Width;
                    return true;
                }
                break;
            case Direction.E:
                if (X < GameUtils.Width - 1)
                {
                    index = Index + 1;
                    return true;
                }
                break;
            case Direction.S:
                if (Z < GameUtils.Depth - 1)
                {
                    index = Index + GameUtils.Width;
                    return true;
                }
                break;
            case Direction.W:
                if (X > 0)
                {
                    index = Index - 1;
                    return true;
                }
                break;
        }
        index = 0; return false;
    }
}