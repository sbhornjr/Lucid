using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private int seed;

    // When there are more than one enemy type this strategy will have to change!
    [SerializeField]
    private GameObject enemyPrefab;

    private TileMap mTileMap;

    // Tiles that already contain enemies (do not stack enemies on a tile).
    private ISet<uint> mEnemyTileIndices;

    void Awake()
    {
        mTileMap = FindObjectOfType<TileMap>();
        mEnemyTileIndices = new HashSet<uint>();
    }

    public void SpawnEnemies(RoomTemplate roomTemplate)
    {
        Random.InitState(seed);
        foreach (var nest in roomTemplate.enemyNests)
        {
            SpawnEnemiesAtNest(nest);
        }
    }

    private void SpawnEnemiesAtNest(RoomTemplate.EnemyNest nest)
    {
        // Determine how many enemies to spawn (within range)
        var numEnemies = Random.Range((int)nest.spawnAttemptsMin, (int)nest.spawnAttemptsMax);
        if (numEnemies == 0) return;

        // Find the set of valid neighbors in which enemies may spawn
        var spawnTileOptions = mTileMap.GetNeighbors(nest.index, nest.spawnRadius, SpawnEnemyPredicate);

        // Shuffle the list
        var shuffledSpawnTileOptions = spawnTileOptions.OrderBy(u => Random.value);

        // Attempt to spawn the enemies
        var spawnAttempts = Mathf.Min(spawnTileOptions.Count, numEnemies);
        foreach (var tile in shuffledSpawnTileOptions.Take(spawnAttempts))
        {
            SpawnEnemyAtNest(nest.spawnChance, tile);
        } 
    }

    private void SpawnEnemyAtNest(float spawnChance, uint spawnTileIndex)
    {
        // Quit if bad roll
        if (Random.value > spawnChance)
        {
            return;
        } 

        // Create an enemy on that tile
        var enemyLocation = mTileMap.GetCenterPositionOfTileAt(spawnTileIndex);
        Instantiate(enemyPrefab, enemyLocation, Quaternion.identity);

        // Mark that this tile has an enemy on it 
        mEnemyTileIndices.Add(spawnTileIndex);
    }

    private bool SpawnEnemyPredicate(Tile candidate)
    {
        return !candidate.IsWall && !candidate.IsPOI && !mEnemyTileIndices.Contains(candidate.Index);
    }

}
