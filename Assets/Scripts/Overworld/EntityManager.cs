using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    /// <summary>
    /// The movement states. These are cycled through every turn.
    /// </summary>
    public enum MovementStates { WaitingForPlayerInput, RotatingPlayer, MovingPlayer, MovingEnemies, PlayerInMenu };

    /// <summary>
    /// The enemy with which to initiate combat. Accessed by the combat manager. 
    /// </summary>
    public GameObject CombatEnemy { get; private set; }

    /// <summary>
    /// The list of enemies.
    /// </summary>
    EnemyMovement[] enemyMovements;

    /// <summary>
    /// The list of cars.
    /// </summary>
    CarMovement[] carMovements;

    /// <summary>
    /// The player.
    /// </summary>
    PlayerMovement playerMovement;

    /// <summary>
    /// The player stats.
    /// </summary>
    PlayerStats playerStats;

    /// <summary>
    /// The level manager.
    /// </summary>
    LevelManager levelManager;

    /// <summary>
    /// The current state.
    /// </summary>
    public MovementStates currentState;

    /// <summary>
    /// The set of enemies that are currently moving.
    /// </summary>
    private ISet<EnemyMovement> movingEnemies;

    /// <summary>
    /// The set of cars that are currently moving.
    /// </summary>
    private ISet<CarMovement> movingCars;

    /// <summary>
    /// The UI that is currently on screen.
    /// </summary>
    public GameObject menuUI;

    [SerializeField]
    private GameObject pauseMenu;

    private bool isFirstTurn;

    private bool wasPlayerHitByCarThisTurn;

    void Awake()
    { 
        playerMovement = FindObjectOfType<PlayerMovement>();
        playerStats = FindObjectOfType<PlayerStats>();
        levelManager = FindObjectOfType<LevelManager>();

        currentState = MovementStates.WaitingForPlayerInput;

        movingEnemies = new HashSet<EnemyMovement>();
        movingCars = new HashSet<CarMovement>();

        wasPlayerHitByCarThisTurn = false;
    }
     
    /// <summary>
    /// Search for enemies and cars and record them.
    /// </summary>
    internal void FindSpawnedEntities()
    { 
        enemyMovements = FindObjectsOfType<EnemyMovement>().Where(e => e.IsEnemyAlive).ToArray();

        if (GameStateMachine.CurrentLevel == GameStateMachine.Level.City)
        {
            carMovements = FindObjectsOfType<CarMovement>();
        } 
    }
     
    public void RoomEntered()
    {
        isFirstTurn = true;
    }

    /// <summary>
    /// Shadow update function not invoked by Unity (instead invoked by the game state machine).
    /// </summary> 
    public GameStateMachine.GameState Update2ElectricBoogaloo()
    {
        switch(currentState)
        {
            case MovementStates.WaitingForPlayerInput:
                StateWaitingForPlayer();
                return GameStateMachine.GameState.Overworld;
            case MovementStates.RotatingPlayer:
                return StateRotatingPlayer();
            case MovementStates.MovingPlayer:
                return StateMovingPlayer();
            case MovementStates.MovingEnemies: 
                return StateMovingEnemies();
            case MovementStates.PlayerInMenu:
                return StatePlayerInMenu();
            default:
                throw new Exception("dafisdnf");
        } 
    }

    /// <summary>
    /// Await player input. Returns true when valid input is received.
    /// </summary> 
    private bool StateWaitingForPlayer()
    {
        if (Input.GetKey(KeyCode.W))
        {
            return HandleMovement(Direction.S);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            return HandleMovement(Direction.W);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            return HandleMovement(Direction.N);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            return HandleMovement(Direction.E);
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            HandleRotation(RotateDirection.Left);
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            HandleRotation(RotateDirection.Right);
        } 
        else if (Input.GetKeyUp(KeyCode.F))
        {
            return HandleInteraction(); 
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuUI = pauseMenu;
            currentState = MovementStates.PlayerInMenu;
            pauseMenu.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.P))
        {
            GameDataSerializer.SaveGame();
            Debug.Log("Game Saved!");
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            playerMovement.AcceptNextIndex(playerMovement.Index);
            currentState = MovementStates.MovingPlayer;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to move the player in the indicated direction. Returns true if movement is possible.
    /// </summary>
    private bool HandleMovement(Direction direction)
    {
        var rotatedDirection = playerMovement.GetInputDirection(direction);
        if (levelManager.EntityCanMoveTo(playerMovement.Index, rotatedDirection, out var maybeNextIndex))
        {
            playerMovement.AcceptNextIndex(maybeNextIndex);
            currentState = MovementStates.MovingPlayer;
            return true;
        } 

        return false;
    }

    /// <summary>
    /// Instructs the player to begin to rotate in the specified direction.
    /// </summary>
    private void HandleRotation(RotateDirection direction)
    {
        playerMovement.AcceptRotationDirection(direction);
        currentState = MovementStates.RotatingPlayer;
    }
    
    /// <summary>
    /// Tries to begin an interaction with an overworld element. Returns true if there is something
    /// with which to be interacted.
    /// </summary> 
    private bool HandleInteraction()
    { 
        if (levelManager.EntityCanInteractWith(playerMovement.Index, playerMovement.Facing, out var index))
        {
            menuUI = levelManager.InteractWith(index);
            currentState = MovementStates.PlayerInMenu;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Rotate the player. Sets currentState when rotation is complete. 
    /// </summary>
    /// <returns></returns>
    private GameStateMachine.GameState StateRotatingPlayer()
    {
        // Rotate until facing the right direction
        if (playerMovement.Rotate())
        {
            currentState = MovementStates.WaitingForPlayerInput;
        }

        return GameStateMachine.GameState.Overworld;
    }    

    /// <summary>
    /// Moves the player. 
    /// </summary>
    /// <returns></returns>
    private GameStateMachine.GameState StateMovingPlayer()
    {
        var startCombat = GameStateMachine.GameState.Overworld;

        // If movement is done, the player's position
        if (playerMovement.Move())
        { 
            // Quit early if the player was hit by a car - don't move other entities in this case
            if (wasPlayerHitByCarThisTurn)
            {
                currentState = MovementStates.WaitingForPlayerInput;
                wasPlayerHitByCarThisTurn = false;
                return startCombat;
            }

            currentState = MovementStates.MovingEnemies;

            // If the player is on a door or portal, skip the rest
            if (levelManager.IsEntityOnDoor(playerMovement.Index))
            {
                return GameStateMachine.GameState.LoadRoom;
            }
            else if (levelManager.IsEntityOnPortal(playerMovement.Index))
            {
                return GameStateMachine.GameState.LoadLevel;
            }
            else if (levelManager.IsEntityOnLava(playerMovement.Index))
            {
                playerStats.TakeDamage(UnityEngine.Random.Range(20, 30));

                var lavaBurnCount = PlayerPrefs.GetInt("LavaBurnCount", 0);
                PlayerPrefs.SetInt("LavaBurnCount", ++lavaBurnCount);

                if (playerStats.CurrentHealth <= 0)
                {
                    return GameStateMachine.GameState.PlayerDead;
                }
            }

            // I'm not too sure what's going on here
            movingCars.Clear();
            movingEnemies.Clear();

            var done = false;
            while(!done)
            {
                var allGood = true;
                var carsAllGood = true;
                ISet<uint> indices = new HashSet<uint>(); 

                // Do cars first
                if (carMovements != null && GameStateMachine.CurrentLevel == GameStateMachine.Level.City && !isFirstTurn)
                {
                    foreach (var car in carMovements)
                    {
                        movingCars.Add(car);

                        uint? carNextIndex;
                        if ((carNextIndex = car.ComputeNextPosition(indices)) != null)
                        { 
                            var next = carNextIndex.Value;
                            indices.Add(next);
                        } 

                        if (car.Index == playerMovement.Index)
                        { 
                            playerStats.TakeDamage(5);
                            car.Honk();

                            var carAccidentCount = PlayerPrefs.GetInt("CarAccidentCount", 0);
                            PlayerPrefs.SetInt("CarAccidentCount", ++carAccidentCount);

                            levelManager.EntityCanInteractWith(playerMovement.Index, playerMovement.Facing.Opposite(), out var nextIndex);
                            playerMovement.AcceptNextIndex(nextIndex);
                            currentState = MovementStates.MovingPlayer;
                            wasPlayerHitByCarThisTurn = true; 

                            if (playerStats.CurrentHealth <= 0)
                            {
                                return GameStateMachine.GameState.PlayerDead;
                            }
                        }
                    }

                    if (allGood)
                    {
                        carsAllGood = true;
                    }
                }

                allGood = true;
                foreach (var enemy in enemyMovements)
                {
                    if (enemy == null || !enemy.isActiveAndEnabled) continue;

                    movingEnemies.Add(enemy);
                    uint? enemyNextIndex;
                    if ((enemyNextIndex = enemy.ComputeNextPosition(indices)) != null)
                    {
                        uint next = (uint)enemyNextIndex;
                        indices.Add(next);
                    }
                    else
                    {
                        allGood = false;
                        break;
                    }

                    if (enemy.Index == playerMovement.Index)
                    {
                        startCombat = GameStateMachine.GameState.Combat;
                        CombatEnemy = enemy.gameObject;

                        movingEnemies.Remove(enemy);
                    }
                }

                if (allGood)
                {
                    done = carsAllGood;
                }
            }

            isFirstTurn = false; 
        }

        return startCombat;
    }

    /// <summary>
    /// Move enemies.  
    /// </summary>
    private GameStateMachine.GameState StateMovingEnemies()
    {
        if (movingEnemies.Count == 0 && movingCars.Count == 0)
        {
            if (GameStateMachine.CurrentLevel == GameStateMachine.Level.City && carMovements.Any(cm => cm.Index == playerMovement.Index))
            {
                playerStats.TakeDamage(playerStats.CurrentHealth);
                return GameStateMachine.GameState.PlayerDead;
            }

            currentState = MovementStates.WaitingForPlayerInput;
            return GameStateMachine.GameState.Overworld;
        }
         
        var startCombat = GameStateMachine.GameState.Overworld;
        var enemiesToRemove = new HashSet<EnemyMovement>();
        var carsToRemove = new HashSet<CarMovement>();
        
        foreach (var enemy in movingEnemies)
        {
            if (enemy == null || !enemy.isActiveAndEnabled) continue;

            if (enemy.Move())
            { 
                enemiesToRemove.Add(enemy);
                if (enemy.Index == playerMovement.Index)
                {
                    startCombat = GameStateMachine.GameState.Combat;
                    CombatEnemy = enemy.gameObject;
                }
            }
        }

        if (GameStateMachine.CurrentLevel == GameStateMachine.Level.City)
        {
            foreach (var car in movingCars)
            {
                if (car.Move())
                {
                    carsToRemove.Add(car);
                    if (car.Index == playerMovement.Index)
                    {
                        playerStats.TakeDamage(10);
                        car.Honk();

                        var carAccidentCount = PlayerPrefs.GetInt("CarAccidentCount", 0);
                        PlayerPrefs.SetInt("CarAccidentCount", ++carAccidentCount);

                        if (levelManager.EntityCanMoveTo(playerMovement.Index, car.CurrentDirection, out var maybeNextIndex))
                        { 
                            playerMovement.AcceptNextIndex(maybeNextIndex);
                            currentState = MovementStates.MovingPlayer;
                            wasPlayerHitByCarThisTurn = true;
                        }
                        else
                        {
                            playerStats.TakeDamage(playerStats.CurrentHealth);
                        }

                        if (playerStats.CurrentHealth <= 0)
                        {
                            return GameStateMachine.GameState.PlayerDead;
                        }
                    }
                }
            }
        }

        foreach (var enemy in enemiesToRemove)
        {
            movingEnemies.Remove(enemy);
        }

        foreach (var car in carsToRemove)
        {
            movingCars.Remove(car);
        }
         
        return startCombat;
    }

    private GameStateMachine.GameState StatePlayerInMenu()
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            currentState = MovementStates.WaitingForPlayerInput;
            menuUI.SetActive(false);
        }
        return GameStateMachine.GameState.Overworld;
    }

    public void ResumeGame()
    {
        currentState = MovementStates.WaitingForPlayerInput;
        pauseMenu.SetActive(false);
    }
}
