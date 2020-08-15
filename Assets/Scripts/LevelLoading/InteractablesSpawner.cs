using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InteractablesSpawner
{
    public static List<ObjectInstantiationData> SpawnPOIs(
        RoomTemplate.POI[] templatePOIs, IDictionary<POIType, GameObject[]> poiPrefabs)
    {
        var pois = new List<ObjectInstantiationData>();

        foreach (var template in templatePOIs)
        {
            pois.Add(SpawnPOI(template, poiPrefabs));
        }

        return pois;
    }

    private static ObjectInstantiationData SpawnPOI(RoomTemplate.POI poi, IDictionary<POIType, GameObject[]> poiPrefabs)
    {
        // Choose the POI prefab (for this POI type and level) and create a package for it
        if (!Enum.TryParse(poi.type, out POIType typeAsEnum)) throw new ArgumentException($"POI type {poi.type} is invalid!");
        var prefabsForType = poiPrefabs[typeAsEnum];
        var prefab = prefabsForType[(int)GameStateMachine.CurrentLevel];

        return new ObjectInstantiationData
        {
            Index = poi.index,
            GameObject = prefab,
            Type = ObjectInstantiationType.POI,
            Rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 4) * 90f, 0),
            POIType = GameUtils.GetPOIType(poi.type)
        };
    }

    public static List<ObjectInstantiationData> SpawnTreasures(
        RoomTemplate.Treasure[] templateTreasures, GameObject[] treasurePrefabs)
    {
        var treasures = new List<ObjectInstantiationData>(templateTreasures.Length);

        foreach (var template in templateTreasures)
        {
            treasures.Add(SpawnTreasure(template, treasurePrefabs));
        }

        return treasures;
    }

    private static ObjectInstantiationData SpawnTreasure(RoomTemplate.Treasure template, GameObject[] treasurePrefabs)
    { 
        // Choose a random treasure prefab and create a package for it
        var prefabIndex = UnityEngine.Random.Range(0, treasurePrefabs.Length);
        return new ObjectInstantiationData
        {
            Index = template.index,
            GameObject = treasurePrefabs[prefabIndex],
            Type = ObjectInstantiationType.Treasure,
            Rotation = Quaternion.Euler(-90, UnityEngine.Random.Range(0, 4) * 90f, 0),
            ObjectPrefabInstantiationIndex = (uint)prefabIndex
        };
    }
}
