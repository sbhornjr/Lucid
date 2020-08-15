using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/**
 * manages the combat portion of the game
 */
public class CombatManager : MonoBehaviour
{
    [SerializeField]
    GameObject combatPlayer;

    [SerializeField]
    PlayerStats playerStats;

    [SerializeField]
    Canvas combatUI;

    [SerializeField]
    Canvas equipmentBarCanvas;

    [SerializeField]
    GameObject weaponUIPrefab;

    [SerializeField]
    GameObject weaponPrefab;

    [SerializeField]
    GameObject lootUI;

    [SerializeField]
    Camera combatCamera;

    [SerializeField]
    TextMeshProUGUI multiplierText;

    [SerializeField]
    AudioClip synergySFX;

    [SerializeField]
    GameObject pauseMenu;

    [SerializeField]
    Transform enemyCombatLocation, regularCameraLocation, bossCameraLocation;

    [SerializeField]
    GameObject[] combatScenes;

    [SerializeField]
    GameObject dmgText, dmgTextArea;

    GameObject enemy;
    public static EnemyStats enemyStats;
    EntityManager entityManager;
    public static bool inCombat;
    CombatStates currentState;

    public enum CombatStates { InCombat, PostCombatMenu, Paused };

    /**
     * get some ne
     */
    void Awake()
    {
        if (combatPlayer == null) combatPlayer = GameObject.FindGameObjectWithTag("CombatPlayer");
        if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>();
        if (entityManager == null) entityManager = FindObjectOfType<EntityManager>();
    }

    /**
     * we are entering combat -> set some necessary values
     */
    internal void Init()
    {
        inCombat = true;
        currentState = CombatStates.InCombat;

        // set up combat scene
        combatScenes[GetCombatIndexForCurrentLevel()].SetActive(true);
        if (GameStateMachine.CurrentLevel == GameStateMachine.Level.Boss)
        {
            combatCamera.transform.position = bossCameraLocation.position;
            combatCamera.transform.rotation = bossCameraLocation.rotation;
        }
        else
        {
            combatCamera.transform.position = regularCameraLocation.position;
            combatCamera.transform.rotation = regularCameraLocation.rotation;
        }

        // gets the enemy engaging with the player and its stats
        enemy = entityManager.CombatEnemy;
        enemyStats = enemy.GetComponent<EnemyStats>();
        enemy.GetComponent<EnemyMovement>().ToggleExcl(false);

        // move the enemy to the combat plane and set up its UI
        enemy.transform.position = enemyCombatLocation.position;//new Vector3(-4.31f, 1f, -86.92f);
        enemy.transform.LookAt(combatPlayer.transform);
        enemy.transform.GetComponentInChildren<Billboard>().cam = combatCamera.transform;

        // set up UI -> disable equipment bar, enable combat UI
        //equipmentBarCanvas.GetComponent<Canvas>().enabled = false;
        combatUI.GetComponent<Canvas>().enabled = true;

        // start combat for each of the player's weapons
        foreach (Weapon weapon in playerStats.weapons)
        {
            if (weapon != null)
                weapon.StartCombat();
        }

        // start combat for the enemy
        enemyStats.StartCombat();
    }

    private static int GetCombatIndexForCurrentLevel()
    {
        switch (GameStateMachine.CurrentLevel)
        {
            case GameStateMachine.Level.Boss:
                return 3;
            case GameStateMachine.Level.Hell:
                return 2;
            case GameStateMachine.Level.City:
                return 1;
            default:
                return 0;
        }
    }

    /**
     * combat has ended -> set some necessary values
     */
    internal void Unload()
    {
        inCombat = false;

        // unsetup combat scene
        combatScenes[GetCombatIndexForCurrentLevel()].SetActive(false);

        // player's possible new weapon was not picked up - destroy it
        playerStats.KillPossibleNewWeapon();

        // disable some combat UI values
        combatUI.GetComponent<Canvas>().enabled = false;
        lootUI.transform.parent.transform.parent.gameObject.SetActive(false);

        // make sure any weapon synergy affects do not carry over
        playerStats.shield = 0;
        playerStats.ToggleShield(false);
        playerStats.deflects.Clear();
        playerStats.deflectImage.enabled = false;
        playerStats.deflectImage.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
        multiplierText.text = "1x damage";
        multiplierText.color = Color.black;
    }

    /**
     * the combat manager's psuedo update function.
     * GameStateMachine calls this every frame while combat is running.
     */
    internal GameStateMachine.GameState Update2ElectricBoogaloo()
    {
        switch(currentState)
        {
            // we are in combat -> check for input and check for deaths
            case CombatStates.InCombat:
                HandleInput();
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    currentState = CombatStates.Paused;
                    pauseMenu.SetActive(true);
                }
                return CheckHealth();
            // combat ended and the player is in the post-combat menu
            case CombatStates.PostCombatMenu:
                // the player is leaving the menu -> end combat
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    // disable some UI elements
                    GameObject pW = GameObject.FindGameObjectWithTag("PlayerWeapons");
                    if (pW != null) pW.transform.parent.parent.gameObject.SetActive(false);
                    GameObject.FindGameObjectWithTag("SlotUnlocked").gameObject.GetComponent<TextMeshProUGUI>().enabled = false;
                    //equipmentBarCanvas.GetComponent<Canvas>().enabled = true;
                    combatUI.GetComponent<Canvas>().enabled = false;

                    //// player reached weapon slot 4 -> we win! (temporary)
                    //if (playerStats.WeaponSlots == 4)
                    //    return GameStateMachine.GameState.PlayerWon;

                    // we're going to the overworld
                    return GameStateMachine.GameState.Overworld;
                }

                // player is still in the post-combat menu
                return GameStateMachine.GameState.Combat;
            case CombatStates.Paused:
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
                {
                    ResumeGame();
                }
                return GameStateMachine.GameState.Combat;
            default:
                throw new System.Exception();
        }
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        currentState = CombatStates.InCombat;
    }

    /**
     * check for player inputs and enemy attacks
     */
    private void HandleInput()
    { 
        var attacking = new List<Weapon>();
        // check each player weapon for an attack signal
        foreach (var weapon in playerStats.weapons)
        {
            // this is necessary because the player may have empty weapon slots
            if (weapon != null)
            {
                // this weapon is attacking -> enemy takes damage and we play a sound effect
                if (weapon.IsWeaponActivated())
                {
                    attacking.Add(weapon);
                }
            }
        }

        if (attacking.Count > 1)
        {
            AudioSource.PlayClipAtPoint(synergySFX, combatCamera.transform.position);
            for (var i = 0; i < attacking.Count; i++)
            {
                Weapon weapon = attacking[i];
                attacking.Remove(weapon);
                weapon.Synergy(attacking);
                attacking.Insert(i, weapon);
            }

            var synergyCount = PlayerPrefs.GetInt("SynergyCount", 0);
            PlayerPrefs.SetInt("SynergyCount", ++synergyCount);
        }

        float multiplier = GetMultiplier();
        multiplierText.text = multiplier.ToString("f1") + "X Damage";
        foreach (Weapon weapon in attacking)
        {
            int dmg = (int)Mathf.Floor(multiplier * weapon.DoDamage);
            enemyStats.TakeDamage(dmg);
            AudioSource.PlayClipAtPoint(weapon.SFX, combatCamera.transform.position);
            combatPlayer.GetComponent<Animator>().SetTrigger(weapon.weaponAnimTrigger);
            var newDmgTxt = Instantiate(dmgText);
            newDmgTxt.GetComponent<TextMeshProUGUI>().text = dmg.ToString();
            newDmgTxt.transform.SetParent(dmgTextArea.transform, false);
            newDmgTxt.GetComponent<RectTransform>().localPosition
                = new Vector3(Random.Range(-100, 1), Random.Range(-100, 101), 0);
            Destroy(newDmgTxt, 1);
        }

        // the enemy is attacking -> player takes damage
        if (enemyStats.weapon.IsWeaponActivated())
        {
            enemy.GetComponent<Animator>().SetTrigger("animAttacking");
            playerStats.TakeDamage(enemyStats.weapon.dmg);
            if (enemyStats.GoldToTake > 0) playerStats.LoseGold(enemyStats.GoldToTake);
        }
    }

    /**
     * check on the physical well being of everyone in combat
     */
    private GameStateMachine.GameState CheckHealth()
    {
        // the player is dead -> tell the GSM
        if (playerStats.CurrentHealth <= 0)
        {
            //combatPlayer.GetComponent<Animator>().SetTrigger("animDeath");
            Destroy(enemy);
            return GameStateMachine.GameState.PlayerDead;
        }

        // the enemy is dead
        // -> destroy the enemy, go into the post-combat menu, tell the player they killed someone
        if (enemyStats.CurrentHealth <= 0)
        {
            // Enemy dies
            EnemyDied();
            if (GameStateMachine.CurrentLevel == GameStateMachine.Level.Boss) return GameStateMachine.GameState.PlayerWon;
            currentState = CombatStates.PostCombatMenu;
            SetupLootUI();
            playerStats.KilledEnemy();
        }

        return GameStateMachine.GameState.Combat;
    }

    /**
     * sets up the post-combat menu.
     * could use some refactoring if i have time
     */
    private void SetupLootUI()
    {
        // randomize the rewards -> amt. of gold, weapon
        var gold = Random.Range(5, 15);
        var weapon = gameObject.Instantiate(weaponPrefab, 0).GetComponent<Weapon>();

        // get the number of enemies that the player has killed and how many they need to unlock a slot
        var enemiesKilled = playerStats.EnemiesKilled + 1;
        var enemiesTilNext = playerStats.TilNextSlot;

        // activate the loot UI
        lootUI.transform.parent.transform.parent.gameObject.SetActive(true);

        // put the amt. of gold in the gold list item
        var goldListItem = lootUI.transform.GetChild(1).gameObject;
        goldListItem.GetComponentInChildren<TextMeshProUGUI>().text = gold.ToString() + " gold";

        // set the weapon list item
        lootUI.transform.GetChild(2).gameObject.GetComponent<NewWeaponClick>().SetUpUI(weapon);

        // show how many enemies the player has killed and how many they need to unlock a slot
        var progressListItem = lootUI.transform.GetChild(3).gameObject;
        progressListItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text
            = enemiesKilled.ToString() + "/" + enemiesTilNext.ToString() + " enemies killed";

        // the player has unlocked a new slot! tell them
        if (enemiesKilled == enemiesTilNext)
        {
            GameObject.FindGameObjectWithTag("SlotUnlocked").gameObject.GetComponent<TextMeshProUGUI>().enabled = true;
            playerStats.UnlockedSlot();
        }

        // set the values for the slot progression slider
        Slider slider = progressListItem.GetComponentInChildren<Slider>();
        slider.maxValue = enemiesTilNext;
        slider.value = enemiesKilled;

        if (enemiesTilNext >= 9999)
        {
            slider.gameObject.SetActive(false);
            progressListItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().enabled = false;
            progressListItem.transform.GetChild(4).GetComponent<TextMeshProUGUI>().enabled = true;
        }

        // give the player their gold
        playerStats.GainGold(gold);

        // possible TODO for later: make checking for end of combat items as a seperate fn

        //// checks to see if the player has a magnet, and give them extra gold for
        //// completing a combat with the magnet
        //for(int itemIndex = 0; itemIndex < playerStats.items.Count; ++itemIndex)
        //{

        //  if(playerStats.items[itemIndex].itemEffect == Item.ItemEffect.magnet)
        //  {
        //    playerStats.items[itemIndex].magnetEffect();
        //  } // checks to see if the player has a syringe and heal them at the end of combat
        //  else if(playerStats.items[itemIndex].itemEffect == Item.ItemEffect.syringe)
        //  {
        //    playerStats.items[itemIndex].syringeEffect();
        //  }

        //}

    }

    /**
     * the enemy has died -> destroy it and play a sound effect
     */
    public void EnemyDied()
    {
        AudioSource.PlayClipAtPoint(enemy.GetComponent<EnemyStats>().deadSFX, combatCamera.transform.position);
        enemy.GetComponent<Animator>().SetTrigger("animDeath");
        DestroyEnemy();

        var enemyDeathCount = PlayerPrefs.GetInt("EnemyKillCount", 0);
        PlayerPrefs.SetInt("EnemyKillCount", ++enemyDeathCount);
    }

    /**
     * destroy the enemy and make sure it knows it's dead
     */
    private void DestroyEnemy()
    {
        enemy.GetComponent<EnemyMovement>().IsEnemyAlive = false;
        Destroy(enemy, 1);
    }

    float GetMultiplier()
    {
        float multi = 0;
        foreach (Weapon weapon in playerStats.weapons)
        {
            if (weapon != null)
                foreach (float multiplier in weapon.Multipliers)
                    multi += multiplier;
        }

        if (multi == 0)
            multiplierText.color = Color.black;
        else multiplierText.color = Color.red;

        return multi > 0 ? multi : 1;
    }
}
