using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class RoomGridGeneration
{
    public static RoomTemplate ReadRoomTemplateFromFile(string roomTemplatePath)
    {
        // Read the file into a JSON object
        RoomTemplate roomTemplate;
        var fullPath = $"Templates/{roomTemplatePath}";
        var templateFile = Resources.Load<TextAsset>(fullPath); 

        roomTemplate = JsonUtility.FromJson<RoomTemplate>(templateFile.text);

        if (roomTemplate.portal == null || roomTemplate.portal.index == 0)
        {
            roomTemplate.portal = null;
        }

        // Extract dimension data
        var dimensions = roomTemplate.dimensions;
        GameUtils.RoomDimensions = dimensions;
         
        return roomTemplate;
    }

    public static Tile[] GenerateRoom(RoomTemplate roomTemplate, bool[] doors, bool hasTreasure, bool hasPOI)
    {
        // Extract dimension data from json object
        var dimensions = roomTemplate.dimensions;
        var width = dimensions.width;
        var height = dimensions.height;

        var tiles = new Tile[width * height];

        // Initialize the tiles
        for (uint z = 0, i = 0; z < height; ++z)
        {
            for (uint x = 0; x < width; ++x)
            {
                tiles[i] = new Tile(i);
                tiles[i].Type = TileType.Floor;
                ++i;
            }
        }

        // Fill Walls
        foreach (var wall in roomTemplate.walls)
        {
            tiles[wall.index].IsWall = true;
        }

        // Fill Doors
        foreach (var door in roomTemplate.doors)
        {
            Direction direction = (Direction)System.Enum.Parse(typeof(Direction), door.direction);
            if (doors[DirectionMethods.DirectionToNumber(direction)])
            {
                tiles[door.index].IsDoor = true;
                tiles[door.index].DoorDirection = direction;
            }
            else
            {
                tiles[door.index].IsWall = true;
            }
            
            //tiles[door.index].IsDoor = true;
        }

        // Fill nones 
        foreach (var none in roomTemplate.nones)
        {
            tiles[none.index].IsNone = true;
        }

        // Fill all enemy nests
        foreach (var nest in roomTemplate.enemyNests)
        {
            tiles[nest.index].IsEnemyNest = true;
        }

        // Fill treasures
        bool treasureAssigned = false;
        foreach (var treasure in roomTemplate.treasures)
        {
            if (hasTreasure)
            {
                if (!treasureAssigned)
                {
                    tiles[treasure.index].IsTreasure = true;
                    treasureAssigned = true;
                }
            }
        }

        // fill pois
        foreach (var poi in roomTemplate.pois)
        {
            if (hasPOI)
            {
                tiles[poi.index].IsPOI = true;
            }
            //tiles[poi.index].POIType = (POIType)assignedPOI;
        }

        // Fill lavas
        if (roomTemplate.lavas != null)
        {
            foreach (var lava in roomTemplate.lavas)
            {
                tiles[lava.index].Type = TileType.Lava;
            }
        }

        // Fill portal, if it exists
        if (roomTemplate.portal != null)
        {
            tiles[roomTemplate.portal.index].Type = TileType.Portal;
        }
         
        return tiles;
    }
}
