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
    private RoomGridMesh mRoomGridMesh;
    private PlayerMovement mPlayerMovement;
    private EnemyMovement mEnemyMovement;
    private bool inCombat;
    private StartEncounter startEncounter;

    private void Awake()
    {
        // Get all references
        mTileMap = FindObjectOfType<TileMap>();
        mRoomGridGeneration = FindObjectOfType<RoomGridGeneration>();
        mRoomGridMesh = FindObjectOfType<RoomGridMesh>();
        mPlayerMovement = FindObjectOfType<PlayerMovement>();
        mEnemyMovement = FindObjectOfType<EnemyMovement>();
        startEncounter = FindObjectOfType<StartEncounter>();
        inCombat = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        // make sure camera setup is correct
        combatCamera.enabled = false;
        mainCamera.enabled = true;

        // Read game dimensions first
        mRoomGridGeneration.ReadRoomDimensions();

        // Generate the room 
        mTileMap.GenerateRoom();

        // Calculate player position
        mPlayerMovement.InitPosition();

        // Calculate enemy position
        mEnemyMovement.InitPosition();
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
