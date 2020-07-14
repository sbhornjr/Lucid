using System;
using System.Collections;
using System.Collections.Generic;
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

    [Serializable]
    public class Wall
    {
        public uint index;
    }

    [Serializable]
    public class EnemyNest
    {
        public uint index;
    }

    [Serializable]
    public class Dimensions
    {
        public uint width, height, tileSize;
    }
}
