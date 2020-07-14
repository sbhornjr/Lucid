using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public bool IsWall { get; set; }

    public bool IsEnemyNest { get; set; }

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

    public Color GetColor()
    {
        return IsWall ? Color.black
            : IsEnemyNest ? Color.red
            : Color.gray;
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
                if (Z < GameUtils.Height - 1)
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