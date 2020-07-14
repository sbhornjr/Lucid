using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    private TileMap mTileMap;
    private RoomGridGeneration mRoomGridGeneration;
    private RoomGridMesh mRoomGridMesh;
    private PlayerMovement mPlayerMovement;

    private void Awake()
    {
        // Get all references
        mTileMap = FindObjectOfType<TileMap>();
        mRoomGridGeneration = FindObjectOfType<RoomGridGeneration>();
        mRoomGridMesh = FindObjectOfType<RoomGridMesh>();
        mPlayerMovement = FindObjectOfType<PlayerMovement>(); 
    }

    // Start is called before the first frame update
    void Start()
    { 
        // Read game dimensions first
        mRoomGridGeneration.ReadRoomDimensions();

        // Generate the room 
        mTileMap.GenerateRoom();

        // Calculate player position
        mPlayerMovement.InitPosition();
    }
}
