using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class OverworldPauseMenu : MonoBehaviour
{
    [SerializeField]
    GameObject MenuPanel, WeaponsPanel, ItemsPanel, AbilitiesPanel, WeaponsList, ItemsList;

    [SerializeField]
    Button MenuButton, WeaponsButton, ItemsButton, AbilitiesButton;

    [SerializeField]
    TMP_Dropdown dropdown;

    Panels currentPanel;

    PlayerStats playerStats;

    EntityManager entityManager;
    CombatManager combatManager;

    enum Panels { Menu, Weapons, Items, Abilities };

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        entityManager = FindObjectOfType<EntityManager>();
        combatManager = FindObjectOfType<CombatManager>();
        ShowMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            switch(currentPanel)
            {
                case Panels.Menu:
                    ShowAbilities();
                    break;
                case Panels.Weapons:
                    ShowMenu();
                    break;
                //case Panels.Items:
                //    ShowWeapons();
                //    break;
                case Panels.Abilities:
                    ShowWeapons();
                    break;
                default:
                    throw new System.Exception();
            }
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            switch (currentPanel)
            {
                case Panels.Menu:
                    ShowWeapons();
                    break;
                case Panels.Weapons:
                    ShowAbilities();
                    break;
                //case Panels.Items:
                //    ShowAbilities();
                //    break;
                case Panels.Abilities:
                    ShowMenu();
                    break;
                default:
                    throw new System.Exception();
            }
        }
    }

    public void ShowAbilities()
    {
        currentPanel = Panels.Abilities;

        MenuPanel.SetActive(false);
        WeaponsPanel.SetActive(false);
        //ItemsPanel.SetActive(false);
        AbilitiesPanel.SetActive(true);

        MenuButton.GetComponent<Image>().color = new Color(0.4117647f, 0.3882353f, 0.6196079f);
        WeaponsButton.GetComponent<Image>().color = new Color(0.4117647f, 0.3882353f, 0.6196079f);
        //ItemsButton.GetComponent<Image>().color = new Color(0.4117647f, 0.3882353f, 0.6196079f);
        AbilitiesButton.GetComponent<Image>().color = new Color(0.2638712f, 0.2509804f, 0.3882353f);
    }

    public void ShowMenu()
    {
        currentPanel = Panels.Menu;

        MenuPanel.SetActive(true);
        WeaponsPanel.SetActive(false);
        //ItemsPanel.SetActive(false);
        AbilitiesPanel.SetActive(false);

        MenuButton.GetComponent<Image>().color = new Color(0.2638712f, 0.2509804f, 0.3882353f);
        WeaponsButton.GetComponent<Image>().color = new Color(0.4117647f, 0.3882353f, 0.6196079f);
        //ItemsButton.GetComponent<Image>().color = new Color(0.4117647f, 0.3882353f, 0.6196079f);
        AbilitiesButton.GetComponent<Image>().color = new Color(0.4117647f, 0.3882353f, 0.6196079f);
    }

    public void ShowWeapons()
    {
        currentPanel = Panels.Weapons;

        MenuPanel.SetActive(false);
        WeaponsPanel.SetActive(true);
        //ItemsPanel.SetActive(false);
        AbilitiesPanel.SetActive(false);

        MenuButton.GetComponent<Image>().color = new Color(0.4117647f, 0.3882353f, 0.6196079f);
        WeaponsButton.GetComponent<Image>().color = new Color(0.2638712f, 0.2509804f, 0.3882353f);
        //ItemsButton.GetComponent<Image>().color = new Color(0.4117647f, 0.3882353f, 0.6196079f);
        AbilitiesButton.GetComponent<Image>().color = new Color(0.4117647f, 0.3882353f, 0.6196079f);

        playerStats.SetupWeaponUIs(WeaponsList);
    }

    public void ShowItems()
    {
        currentPanel = Panels.Items;

        MenuPanel.SetActive(false);
        WeaponsPanel.SetActive(false);
        ItemsPanel.SetActive(true);
        AbilitiesPanel.SetActive(false);

        MenuButton.GetComponent<Image>().color = new Color(0.4117647f, 0.3882353f, 0.6196079f);
        WeaponsButton.GetComponent<Image>().color = new Color(0.4117647f, 0.3882353f, 0.6196079f);
        ItemsButton.GetComponent<Image>().color = new Color(0.2638712f, 0.2509804f, 0.3882353f);
        AbilitiesButton.GetComponent<Image>().color = new Color(0.4117647f, 0.3882353f, 0.6196079f);

        playerStats.SetupItemUIs(ItemsList);
    }

    public void Resume()
    {
        entityManager.ResumeGame();
        combatManager.ResumeGame();
        transform.parent.gameObject.SetActive(false);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetAbilitiesText()
    {
        switch(dropdown.value)
        {
            // more dmg (attack based)
            case 0:
                AbilitiesPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text
                    = "Level 1: This attack does x1.5 damage";
                AbilitiesPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text
                    = "Level 2: This attack does x2 damage";
                AbilitiesPanel.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text
                    = "Level 3: This attack, along with all attacks synergized with this one, does x1.5 damage";
                AbilitiesPanel.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text
                    = "Level 4: This attack, along with all attacks synergized with this one, does x2 damage";
                AbilitiesPanel.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text
                    = "Level 5: this attack does x3 damage";
                break;
            // more dmg (time based)
            case 1:
                AbilitiesPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text
                    = "Level 1: All attacks do x1.5 damage for 3 seconds";
                AbilitiesPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text
                    = "Level 2: All attacks do x1.5 damage for 5 seconds";
                AbilitiesPanel.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text
                    = "Level 3: All attacks do x2 damage for 3 seconds";
                AbilitiesPanel.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text
                    = "Level 4: All attacks do x2 damage for 5 seconds";
                AbilitiesPanel.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text
                    = "Level 5: All attacks do x3 damage for 3 seconds";
                break;
            // gain shield
            case 2:
                AbilitiesPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text
                    = "Level 1: Gain 3 shield";
                AbilitiesPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text
                    = "Level 2: Gain 5 shield";
                AbilitiesPanel.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text
                    = "Level 3: Gain 8 shield";
                AbilitiesPanel.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text
                    = "Level 4: Gain 10 shield";
                AbilitiesPanel.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text
                    = "Level 5: Gain 15 shield";
                break;
            // deflect
            case 3:
                AbilitiesPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text
                    = "Level 1: Block 20% of the enemy's next attack";
                AbilitiesPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text
                    = "Level 2: Block 40% of the enemy's next attack";
                AbilitiesPanel.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text
                    = "Level 3: Block 50% of the enemy's next attack";
                AbilitiesPanel.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text
                    = "Level 4: Block 65% of the enemy's next attack";
                AbilitiesPanel.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text
                    = "Level 5: Block 80% of the enemy's next attack";
                break;
            // shorten cooldowns
            case 4:
                AbilitiesPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text
                    = "Level 1: This weapon's cooldown is halved until its next attack";
                AbilitiesPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text
                    = "Level 2: This weapon's cooldown and that of the weapon(s) synergized with are halved until their next attack";
                AbilitiesPanel.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text
                    = "Level 3: This weapon's cooldown is halved for its next 2 attacks";
                AbilitiesPanel.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text
                    = "Level 4: This weapon's cooldown and that of the weapon(s) synergized with are halved for their next 2 attacks";
                AbilitiesPanel.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text
                    = "Level 5: This weapon's timer is halved for the rest of the combat";
                break;
            // lengthen enemy cooldowns
            case 5:
                AbilitiesPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text
                    = "Level 1: The enemy's timer is doubled until its next attack";
                AbilitiesPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text
                    = "Level 2: The enemy's timer is doubled for its next 2 attacks";
                AbilitiesPanel.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text
                    = "Level 3: The enemy's timer is doubled for its next 3 attacks";
                AbilitiesPanel.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text
                    = "Level 4: The enemy's timer is doubled for the number of attacks equal to the number of synergized weapons this attack";
                AbilitiesPanel.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text
                    = "Level 5: The enemy's timer is doubled for the rest of the combat";
                break;
            default:
                throw new System.Exception();
        }
    }
}
