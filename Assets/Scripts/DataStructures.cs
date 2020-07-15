using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Runtime.Serialization;
using UnityEngine;

public class GameUtils
{
    public static uint Width { get; set; }
    public static uint Height { get; set; }
    public static uint TileSize { get; set; }

    public static RoomTemplate.Dimensions RoomDimensions
    {
        set
        {
            Width = value.width;
            Height = value.height;
            TileSize = value.tileSize;
        }
    }

    public static void GetCoordinates(uint index, out uint X, out uint Z)
    {
        X = GetX(index);
        Z = GetZ(index);
    }

    public static uint GetX(uint index)
    {
        return index % Width;
    }

    public static uint GetZ(uint index)
    {
        return index / Width;
    }

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

public enum POIType
{
    NONE, Vendor, TotemHealth, TotemGold, TotemMystery
}

public enum Direction
{
    N, E, S, W
} 

[Serializable]
public class RoomTemplate
{
    public Dimensions dimensions;
    public Wall[] walls;
    public EnemyNest[] enemyNests;
    public POI[] pois;
    public Treasure[] treasures;

    [Serializable]
    public class Wall
    {
        public uint index;
    }

    [Serializable]
    public class EnemyNest
    {
        public uint index;
        public uint spawnRadius;
        public uint spawnChance;
        public uint spawnMin, spawnMax;
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
    public class Dimensions
    {
        public uint width, height, tileSize;
    }
}
