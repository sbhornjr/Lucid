using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Camera combatCamera;

    private TileMap mTileMap;
    private RoomGridGeneration mRoomGridGeneration; 
    private PlayerMovement mPlayerMovement;  
    private bool inCombat;
    private StartEncounter startEncounter; 
    private EnemySpawner mEnemySpawner; 

    private void Awake()
    {
        // Get all references
        mTileMap = FindObjectOfType<TileMap>();
        mRoomGridGeneration = FindObjectOfType<RoomGridGeneration>(); 
        mPlayerMovement = FindObjectOfType<PlayerMovement>();  
        startEncounter = FindObjectOfType<StartEncounter>();
        inCombat = false; 
        mEnemySpawner = GetComponent<EnemySpawner>(); 
    }

    // Start is called before the first frame update
    void Start()
    {
        // make sure camera setup is correct
        combatCamera.enabled = false;
        mainCamera.enabled = true;

        // Read game dimensions first
        var roomTemplate = mRoomGridGeneration.ReadRoomDimensions();

        // Generate the room 
        mTileMap.GenerateRoom();

        // Calculate player position
        mPlayerMovement.InitPosition();
         
        // Spawn enemies
        mEnemySpawner.SpawnEnemies(roomTemplate);

        // Tell the player about them
        mPlayerMovement.FindEnemiesInScene();
    }

    private void Update()
    {
        if (inCombat && Input.GetKeyDown(KeyCode.Q))
        {
            inCombat = false;
            startEncounter.DestroyEnemy();
            SetCombatCamera(false);
            SetMainCamera(true);
        }
    }

    public void SetMainCamera(bool which)
    {
        mainCamera.enabled = which;
    }

    public void SetCombatCamera(bool which)
    {
        combatCamera.enabled = which;
    }

    public void StartCombat()
    {
        inCombat = true; 
    }
}
