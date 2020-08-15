using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/**
 * the topmost script in the heirarchy of our game.
 * tells the various managers what to do and when.
 * manages the state that the game is in.
 */
public class GameStateMachine : MonoBehaviour
{
    [SerializeField]
    private int seed;

    [SerializeField]
    private Camera mainCamera, combatCamera;

    [SerializeField]
    TextMeshProUGUI wonText, lostText;

    [SerializeField]
    Image wonBackground;

    [SerializeField]
    private Level startingLevel = Level.Dungeon;

    [SerializeField]
    private bool ignoreSeed;

    [SerializeField]
    SaveMenu saveMenu;

    LevelLoader levelLoader;
    OverworldManager overworldManager;
    CombatManager combatManager;
    LevelManager levelManager;

    [SerializeField]
    DialogBox dialogBox;

    private GameState currentState;

    /// <summary>
    /// True when a fresh game is to be loaded (and the player's stats are to be wiped).
    /// </summary>
    private bool wipePlayerDataOnLoad;

    /// <summary>
    /// True when a saved game is to be loaded from disk.
    /// </summary>
    public static bool loadFromSaveFile = false;

    public static Level CurrentLevel { get; set; }
    public enum Level { Dungeon, City, Hell, Boss }; 

    /**
     * get references to all necessary managers
     */ 
    void Awake()
    {
        CurrentLevel = startingLevel;
        if (!ignoreSeed) { Random.InitState(seed); }
        currentState = GameState.Overworld;

        levelLoader = FindObjectOfType<LevelLoader>();
        overworldManager = FindObjectOfType<OverworldManager>();
        combatManager = FindObjectOfType<CombatManager>();
        levelManager = FindObjectOfType<LevelManager>();
    }

    /**
     * make sure that we start by loading the level and that
     * the main/combat cameras have the correct configuration
     */ 
    private void Start()
    {
        currentState = GameState.LoadLevel;
        wipePlayerDataOnLoad = true; 

        SetCombatCamera(false);
        SetMainCamera(true);
    }

    /**
     * the main update loop in the game.
     * the current state dictates what happens in each frame.
     */ 
    private void Update()
    {
        switch (currentState)
        {
            case GameState.Overworld:
                StateOverworld();
                break;
            case GameState.Combat:
                StateCombat();
                break;

            // the game (or a new level) is starting. load the level and go to the overworld.
            case GameState.LoadLevel:
                levelLoader.LoadLevel(wipePlayerDataOnLoad, loadFromSaveFile);
                currentState = GameState.Overworld;
                overworldManager.Init();
                dialogBox.LevelSetup(CurrentLevel); 
                break;

            // the player is moving to another room. render the room and go to the overworld.
            case GameState.LoadRoom:
                levelManager.UpdateToNextRoom();
                currentState = GameState.Overworld;
                overworldManager.Init();
                break;

            // the player has died. show the lost text and respawn.
            case GameState.PlayerDead:
                levelManager.playerMovement.anim.SetTrigger("animDeath");
                combatManager.Unload();
                currentState = GameState.None;
                lostText.enabled = true;
                wipePlayerDataOnLoad = true;
                loadFromSaveFile = false;
                CurrentLevel = Level.Dungeon;
                Invoke("StartOver", 5);

                var lossCount = PlayerPrefs.GetInt("DeathCount", 0);
                PlayerPrefs.SetInt("DeathCount", ++lossCount);

                break;

            // the player has won. show the won text and respawn.
            case GameState.PlayerWon:
                combatManager.Unload();
                currentState = GameState.None;
                wonText.enabled = true;
                wonBackground.enabled = true;
                Invoke("StartMenu", 5);

                var winCount = PlayerPrefs.GetInt("WinCount", 0);
                PlayerPrefs.SetInt("WinCount", ++winCount); 

                break;
            case GameState.SaveGame:
                if (SaveMenu.done)
                {
                    //if (CurrentLevel == Level.Boss) currentStatee = GamGameState
                    currentState = GameState.LoadLevel;
                    saveMenu.gameObject.SetActive(false);
                    SaveMenu.done = false;
                }
                break;
            default:
                break;
        }
    }

    /**
     * we are in the overworld. call the overworld manager's update function.
     */ 
    void StateOverworld()
    {
        var nextState = overworldManager.Update2ElectricBoogaloo();

        // next state is combat (entering combat)
        if (nextState == GameState.Combat)
        {
            overworldManager.Unload();
            combatManager.Init();

            SetMainCamera(false);
            SetCombatCamera(true);

            currentState = nextState;
        }
        else if (nextState == GameState.LoadLevel)
        {
            CurrentLevel += 1;
            saveMenu.gameObject.SetActive(true);
            wipePlayerDataOnLoad = false;
            currentState = GameState.SaveGame;
        }
        else
        {
            currentState = nextState;
        }
    }

    /**
     * we are in combat. call the combat manager's update function
     */ 
    void StateCombat()
    {
        var nextState = combatManager.Update2ElectricBoogaloo();

        // next state is overworld (ending combat)
        if (nextState == GameState.Overworld)
        {
            currentState = GameState.Overworld;
            combatManager.Unload();
            overworldManager.Init();

            SetMainCamera(true);
            SetCombatCamera(false);
        }

        // player won or lost
        else if (nextState == GameState.PlayerDead || nextState == GameState.PlayerWon)
        {
            currentState = nextState;

            SetCombatCamera(false);
            SetMainCamera(true);
        }
    }

    /**
     * enables or disables the main camera
     */ 
    public void SetMainCamera(bool which)
    {
        mainCamera.enabled = which;
    }

    /**
     * enables or disables the combat camera
     */ 
    public void SetCombatCamera(bool which)
    {
        combatCamera.enabled = which; 
    }

    /**
     * called 2 seconds after the player loses -> erase that text and load the level
     */ 
    private void StartOver()
    {
        lostText.enabled = false;
        currentState = GameState.LoadLevel;
    }

    /**
     * called 2 seconds after the player wins -> erase that text and load the start menu
     */
    private void StartMenu()
    {
        wonText.enabled = false;
        wonBackground.enabled = false;
        SceneManager.LoadScene(0);
    }

    // enum for game states
    // Overworld -> the player is in the overworld (main map)
    // Combat -> the player is engaged in combat with an enemy
    // LoadLevel -> the game is starting or the player entered level 2 or 3
    // PlayerWon -> the player has won the game
    // PlayerDead -> the player has lost the game
    // None -> doing nothing
    public enum GameState { Overworld, Combat, LoadLevel, LoadRoom, PlayerWon, PlayerDead, SaveGame, None }
}
