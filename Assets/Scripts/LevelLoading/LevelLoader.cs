using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{ 
    LevelManager levelManager;
    PlayerStats playerStats;
    LevelGenerator levelGenerator; 

    // Start is called before the first frame update
    void Awake()
    {
        levelManager = FindObjectOfType<LevelManager>();
        playerStats = FindObjectOfType<PlayerStats>();
        levelGenerator = FindObjectOfType<LevelGenerator>(); 
    }

    // Load the level and give it to the level manager
    public void LoadLevel(bool wipePlayerData, bool loadFromSaveFile)
    {
        if (loadFromSaveFile)
        {
            var currentLevel = GameDataSerializer.LoadGame();
            GameStateMachine.CurrentLevel = currentLevel;
        }
        else if (wipePlayerData)
        {
            // Wipe player data if desired
            playerStats.Reset();
        }

        Level level;
        if (GameStateMachine.CurrentLevel  == GameStateMachine.Level.Boss) level = levelGenerator.BossLevel();
        else level = levelGenerator.GenerateLevel();

        levelManager.AcceptLevel(level);

        levelManager.LoadCurrentRoom();
        levelManager.RenderCurrentRoom();
    } 
}
