using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// A Room of tiles.
/// </summary>
public class Room
{
    public TileMap tileMap;
    public bool[] Neighbors { get; private set; } // N, E, S, W
    public string templateFileName;
    public Direction directionOfNextRoom;

    private uint width, depth;

    public IDictionary<uint, ObjectInstantiationData> Treasures { get; private set; }
    public IDictionary<uint, ObjectInstantiationData> POIs { get; private set; }

    public IDictionary<uint, ObjectInstantiationData> Cars { get; private set; }
    public IDictionary<uint, ObjectInstantiationData> LavaFX { get; private set; } 
    public IDictionary<uint, ObjectInstantiationData> PortalFX { get; private set; }

    public GameObject[] EnemiesInRoom { get; set; }

    public bool HasRoomBeenLoaded { get; set; }

    public POIType? assignedPOI = null;
    public bool hasTreasure = false;

    public Room(string name)
    { 
        templateFileName = name;
        Neighbors = new bool[4] { false, false, false, false };
        tileMap = new TileMap();

        Treasures = new Dictionary<uint, ObjectInstantiationData>();
        POIs = new Dictionary<uint, ObjectInstantiationData>();
        Cars = new Dictionary<uint, ObjectInstantiationData>();
        LavaFX = new Dictionary<uint, ObjectInstantiationData>(); 
        PortalFX = new Dictionary<uint, ObjectInstantiationData>(); 
    }

    public IList<ObjectInstantiationData> LoadFromTemplate(
        GameObject[] enemyPrefabs, GameObject[] treasurePrefabs, 
        IDictionary<POIType, GameObject[]> poiPrefabs, 
        GameObject lavaParticlesPrefab, GameObject portalParticlesPrefab)
    {
        bool hasPOI = false;
        if (assignedPOI != null)
        {
            hasPOI = true;
        }

        var roomTemplate = tileMap.GenerateRoom(templateFileName, Neighbors, hasTreasure, hasPOI);
        foreach (var poi in roomTemplate.pois) { poi.type = assignedPOI.ToString(); }

        width = roomTemplate.dimensions.width;
        depth = roomTemplate.dimensions.height;

        var objs = new List<ObjectInstantiationData>();

        objs.AddRange(EnemySpawner.SpawnEnemies(tileMap, roomTemplate.enemyNests, enemyPrefabs)); 
        if (hasTreasure)
        {
            var treasures = (InteractablesSpawner.SpawnTreasures(roomTemplate.treasures, treasurePrefabs));
            objs.AddRange(treasures);
        }
        
        if (assignedPOI != null)
        {
            var pois = (InteractablesSpawner.SpawnPOIs(roomTemplate.pois, poiPrefabs));
            objs.AddRange(pois);
        }

        var lavas = ParticleSpawner.SpawnLavaParticles(roomTemplate.lavas, lavaParticlesPrefab);
        objs.AddRange(lavas);

        var portals = ParticleSpawner.SpawnPortalParticles(roomTemplate.portal, portalParticlesPrefab);
        objs.AddRange(portals);

        return objs;
    }

    public void ReloadRoom()
    {
        // Reset the dimensions in the game utils 
        GameUtils.Width = width;
        GameUtils.Depth = depth;
    }

    internal bool EntityCanMoveTo(uint index, Direction direction, out uint maybeNextIndex)
    {
        return tileMap.TryMoveToNeighbor(index, direction, out maybeNextIndex);
    }

    internal bool EntityCanInteractWith(uint index, Direction direction, out uint maybeNeighborIndex)
    {
        if (tileMap.TryGetNeighborIndex(index, direction, out maybeNeighborIndex))
        {
            return tileMap.IsTileInteractable(maybeNeighborIndex);
        }

        return false;
    }
      
    public bool HasDirection(Direction d)
    {
        return Neighbors[DirectionMethods.DirectionToNumber(d)];
    }

    internal IEnumerable<GameObject> Cleanup()
    {
        var objs = new List<GameObject>();

        if (Treasures != null)
        {
            foreach (var treasure in Treasures)
            {
                objs.Add(treasure.Value.GameObject);
            }
        }
        
        if (POIs != null)
        {
            foreach (var poi in POIs)
            {
                objs.Add(poi.Value.GameObject);
            }
        }
       
        if (EnemiesInRoom != null)
        {
            foreach (var enemy in EnemiesInRoom)
            {
                objs.Add(enemy);
            }
        }

        if (Cars != null)
        {
            foreach (var car in Cars)
            {
                objs.Add(car.Value.GameObject);
            }
        }

        if (LavaFX != null)
        {
            foreach (var lava in LavaFX)
            {
                objs.Add(lava.Value.GameObject);
            }
        }

        if (PortalFX != null)
        {
            foreach (var portal in PortalFX)
            {
                objs.Add(portal.Value.GameObject);
            }
        }

        return objs;
    }

    public void CreateConnection(Direction direction)
    {
        Neighbors[DirectionMethods.DirectionToNumber(direction)] = true;
    }

    internal bool IsEntityOnDoor(uint playerIndex)
    {
        if (tileMap.GetTiles()[playerIndex].IsDoor)
        {
            directionOfNextRoom = tileMap.GetTiles()[playerIndex].DoorDirection;
            return true;
        }
        else
        {
            return false;

        }
    }

    internal bool IsEntityOnPortal(uint playerIndex)
    {
        return (tileMap.GetTileType(playerIndex) == TileType.Portal);
    }

    internal bool IsEntityOnLava(uint playerIndex)
    {
        return tileMap.GetTileType(playerIndex) == TileType.Lava;
    }

    internal uint GetIndexOfDoor(Direction direction)
    {
        return tileMap.GetIndexOfDoor(direction);
    } 
}
