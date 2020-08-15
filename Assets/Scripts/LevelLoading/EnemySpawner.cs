using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner
{

    /// <summary>
    /// The tile indices that currently have enemies on them.
    /// </summary>
    private static ISet<uint> sEnemyTileIndices = new HashSet<uint>();

    /// <summary>
    /// Spawns random enemies around nests on the map. Returns a list of pairs of enemy prefab
    /// instantiation instances and the index of that instance.
    /// </summary> 
    public static List<ObjectInstantiationData> SpawnEnemies(TileMap tileMap, RoomTemplate.EnemyNest[] nests, GameObject[] enemyPrefabs)
    {
        var enemies = new List<ObjectInstantiationData>();
        sEnemyTileIndices.Clear();

        foreach (var nest in nests)
        { 
            enemies.AddRange(SpawnEnemiesAtNest(tileMap, nest, enemyPrefabs));
        }

        return enemies;
    } 

    /// <summary>
    /// Spawn random enemies around the given nest.
    /// </summary> 
    private static IList<ObjectInstantiationData> SpawnEnemiesAtNest(
        TileMap tileMap, RoomTemplate.EnemyNest nest, GameObject[] enemyPrefabs)
    {
        // Determine how many enemies to spawn (within range)
        var numEnemies = Random.Range((int)nest.spawnAttemptsMin, (int)nest.spawnAttemptsMax + 1);
        if (numEnemies == 0) return new List<ObjectInstantiationData>(0);

        var enemies = new List<ObjectInstantiationData>();

        // Find the set of valid neighbors in which enemies may spawn
        var spawnTileOptions = tileMap.GetNeighbors(nest.index, nest.spawnRadius, SpawnEnemyPredicate);

        // Shuffle the list
        var shuffledSpawnTileOptions = spawnTileOptions.OrderBy(u => Random.value);

        // Attempt to spawn the enemies
        var spawnAttempts = Mathf.Min(spawnTileOptions.Count, numEnemies);
        foreach (var tile in shuffledSpawnTileOptions.Take(spawnAttempts))
        {
            var maybeEnemy = SpawnEnemyAtNest(nest.spawnChance, tile, enemyPrefabs);
            if (maybeEnemy.HasValue) enemies.Add(maybeEnemy.Value);
        }

        return enemies;
    }

    /// <summary>
    /// Spawn one enemy at the given nest.
    /// </summary> 
    private static ObjectInstantiationData? SpawnEnemyAtNest(float spawnChance, uint spawnTileIndex,
        GameObject[] enemyPrefabs)
    {
        // Quit if bad roll
        if (Random.value > spawnChance)
        {
            return null;
        }
         
        // Mark that this tile has an enemy on it
        sEnemyTileIndices.Add(spawnTileIndex);
        return new ObjectInstantiationData 
        {
            GameObject = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], 
            Index = spawnTileIndex, 
            Type = ObjectInstantiationType.Enemy, 
            Rotation = Quaternion.identity 
        };
    }

    /// <summary>
    /// BFS predicate: spawn an enemy only if the tile is not a special tile and if the tile 
    /// does not have an enemy on it already.
    /// </summary> 
    private static bool SpawnEnemyPredicate(Tile candidate)
    {
        return !candidate.IsWall && !candidate.IsTreasure && !candidate.IsPOI && !sEnemyTileIndices.Contains(candidate.Index);
    }

}
