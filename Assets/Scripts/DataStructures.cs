using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Several static utility properties and functions.
/// </summary>
public class GameUtils
{
    /// <summary>
    /// The width of the room, in tiles.
    /// </summary>
    public static uint Width { get; set; }

    /// <summary>
    /// The depth of the room, in tiles.
    /// </summary>
    public static uint Depth { get; set; }

    /// <summary>
    /// The size of a tile, in meters/Unity units idk
    /// </summary>
    public static uint TileSize { get; set; }

    /// <summary>
    /// Store the room dimension information.
    /// </summary>
    public static RoomTemplate.Dimensions RoomDimensions
    {
        set
        {
            Width = value.width;
            Depth = value.height;
            TileSize = value.tileSize;
        }
    }

    /// <summary>
    /// Gets the X and Z coordinates of an index based on the width/depth.
    /// </summary>
    public static void GetCoordinates(uint index, out uint X, out uint Z)
    {
        X = GetX(index);
        Z = GetZ(index);
    }

    /// <summary>
    /// Get the X coordinate of an index based on the width/depth.
    /// </summary> 
    public static uint GetX(uint index)
    {
        return index % Width;
    }

    /// <summary>
    /// Get the Z coordinate of an index based on the width/depth.
    /// </summary> 
    public static uint GetZ(uint index)
    {
        return index / Width;
    }

    /// <summary>
    /// Get the index of an x/z pair based on the width/depth.
    /// </summary> 
    public static uint GetIndex(uint X, uint Z)
    {
        return Z * Width + X;
    }

    /// <summary>
    /// Gets the center position of an index in world space.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static Vector3 GetPosition(uint index)
    {
        return new Vector3(
            GetX(index) * TileSize + TileSize / 2,
            0,
            GetZ(index) * TileSize + TileSize / 2
            );
    }

    /// <summary>
    /// Translates a POI string name to the POIType value. Returns NONE if no match is found.
    public static POIType GetPOIType(string poiString)
    {
        switch (poiString)
        {
            case "Vendor": return POIType.Vendor;
            case "TotemHealth": return POIType.TotemHealth;
            case "TotemGold": return POIType.TotemGold;
            case "TotemMystery": return POIType.TotemMystery;
            default: return POIType.NONE;
        }
    }
}

public enum TileType
{
    None, 
    Floor,
    Wall,
    Nest,
    Treasure,
    POI,
    Door, 
    Portal, 
    Lava
}

/// <summary>
/// The type of the point of interest.
/// </summary>
public enum POIType 
{
    NONE, Vendor, TotemHealth, TotemGold, TotemMystery
}

/// <summary>
/// Cardinal directions.
/// </summary>
public enum Direction
{
    N, E, S, W
}

/// <summary>
/// The direction of rotation, rotating about the y-axis.
/// </summary>
public enum RotateDirection
{
    Left, Right
}


/// <summary>
/// Extension methods callable on Directions.
/// </summary>
public static class Ext
{
    private static readonly Direction[] cachedDirections = (Direction[])Enum.GetValues(typeof(Direction));

    /// <summary>
    /// Gets the opposite direction for this cardinal direction.
    public static Direction Opposite(this Direction dir)
    {
        return (Direction)((((int)dir) + 2) % 4);
    }

    /// <summary>
    /// Gets all possible Direction values.
    public static Direction[] GetValues(this Direction _)
    {
        return cachedDirections;
    }

    public static bool IsInteractable(this TileType type)
    {
        return type == TileType.Treasure || type == TileType.POI;
    }

}

/// <summary>
/// Other Direction methods.
/// </summary>
public class DirectionMethods
{
    public static Direction NumberToDirection(int n)
    {
        return (Direction)n;
    }

    public static int DirectionToNumber(Direction d)
    {
        return (int)d;
    }
}

[Serializable]
public class RoomTemplate
{
    public Dimensions dimensions;
    public Wall[] walls;
    public None[] nones;
    public EnemyNest[] enemyNests;
    public POI[] pois;
    public Treasure[] treasures;
    public Door[] doors;
    public Portal portal;
    public Lava[] lavas;

    [Serializable]
    public class Wall
    {
        public uint index;
    }

    [Serializable]
    public class None
    {
        public uint index;
    }

    [Serializable]
    public class EnemyNest
    {
        public uint index;
        public uint spawnRadius;
        public float spawnChance;
        public uint spawnAttemptsMin, spawnAttemptsMax;
    }

    [Serializable]
    public class POI
    {
        public uint index;
        public string type;
    }

    [Serializable]
    public class Treasure
    {
        public uint index;
    }

    [Serializable]
    public class Door
    {
        public uint index;
        public String direction;
    }

    [Serializable]
    public class Dimensions
    {
        public uint width, height, tileSize;
    }

    [Serializable]
    public class Portal
    {
        public uint index;
    }

    [Serializable]
    public class Lava
    {
        public uint index;
    }
} 
