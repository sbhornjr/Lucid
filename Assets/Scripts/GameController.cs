using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    private TileMap mTileMap;
    private RoomGridGeneration mRoomGridGeneration;
    private RoomGridMesh mRoomGridMesh;
    private PlayerMovement mPlayerMovement;
    private EnemySpawner mEnemySpawner;

    private void Awake()
    {
        // Get all references
        mTileMap = FindObjectOfType<TileMap>();
        mRoomGridGeneration = FindObjectOfType<RoomGridGeneration>();
        mRoomGridMesh = FindObjectOfType<RoomGridMesh>();
        mPlayerMovement = FindObjectOfType<PlayerMovement>();
        mEnemySpawner = GetComponent<EnemySpawner>();
    }

    // Start is called before the first frame update
    void Start()
    { 
        // Read game dimensions first
        var roomTemplate = mRoomGridGeneration.ReadRoomDimensions();

        // Generate the room 
        mTileMap.GenerateRoom();

        // Calculate player position
        mPlayerMovement.InitPosition();

        // Spawn enemies
        mEnemySpawner.SpawnEnemies(roomTemplate);
    }
}
