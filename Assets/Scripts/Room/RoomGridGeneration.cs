using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RoomGridGeneration : MonoBehaviour
{
    [SerializeField]
    private string roomTemplatePath;

    private RoomGridMesh mMesh;

    private RoomTemplate mRoomTemplate;

    public void Awake()
    {
        mMesh = GetComponentInChildren<RoomGridMesh>();
        //mRoomTemplate = ReadRoomDimensions(roomTemplatePath);
    }

    public RoomTemplate ReadRoomDimensions()
    {
        // Read the file into a JSON object
        RoomTemplate roomTemplate;
        using (var reader = new StreamReader(new FileStream(roomTemplatePath, FileMode.Open)))
        {
            var data = reader.ReadToEnd();
            roomTemplate = JsonUtility.FromJson<RoomTemplate>(data);
        }

        // Extract dimension data
        var dimensions = roomTemplate.dimensions; 
        GameUtils.RoomDimensions = dimensions;

        mRoomTemplate = roomTemplate;
        return roomTemplate;
    }

    public Tile[] GenerateRoom()
    {
        var tiles = ParseRoomTemplateFile(roomTemplatePath);
        mMesh.Triangulate(tiles);

        return tiles;
    }

    private Tile[] ParseRoomTemplateFile(string filePath)
    { 
        // Extract dimension data from json object
        var dimensions = mRoomTemplate.dimensions;
        var width = dimensions.width;
        var height = dimensions.height; 

        var tiles = new Tile[width * height];

        // Initialize the tiles
        for (uint z = 0, i = 0; z < height; ++z)
        {
            for (uint x = 0; x < width; ++x)
            {
                tiles[i] = new Tile(i);
                ++i;
            }
        }

        // Fill border
        for (uint z = 0; z < height; z++)
        {
            tiles[z * width].IsWall = true;
            tiles[(z + 1) * width - 1].IsWall = true;

            if (z == 0 || z == height - 1)
            {
                for (uint i = z * width; i < (z + 1) * width; i++)
                {
                    tiles[i].IsWall = true;
                }
            }
        }

        // Fill all other walls
        foreach (var wall in mRoomTemplate.walls)
        {
            tiles[wall.index].IsWall = true;
        }

        // Fill all enemy nests
        foreach (var nest in mRoomTemplate.enemyNests)
        {
            tiles[nest.index].IsEnemyNest = true;
        }

        // Fill the rest
        foreach (var treasure in mRoomTemplate.treasures)
        {
            tiles[treasure.index].IsTreasure = true;
        }
        foreach (var poi in mRoomTemplate.pois)
        {
            tiles[poi.index].POIType = GameUtils.GetPOIType(poi.type);
        }

        return tiles;
    }

}
