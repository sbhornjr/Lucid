using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class StartMenu : MonoBehaviour
{

    public GameObject aboutMenu, statsMenu, controlsMenu;
    public Button continueButton;
    bool aboutMenuOpen = false;
    bool statsMenuOpen = false;
    bool controlsMenuOpen = false;

    private void Start()
    {
        if (!GameDataSerializer.HasSaveData())
        {
            continueButton.interactable = false;
        }
    }

    public void Load(bool save)
    {
        GameStateMachine.loadFromSaveFile = save;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    public void About()
    {
        if (!aboutMenuOpen)
        {
            statsMenu.SetActive(false);
            statsMenuOpen = false;
            aboutMenu.SetActive(true);
            aboutMenuOpen = true;
            controlsMenu.SetActive(false);
            controlsMenuOpen = false;
        }
        else
        {
            aboutMenu.SetActive(false);
            aboutMenuOpen = false;
        }
    }

    public void Stats()
    {
        if (!statsMenuOpen)
        {
            aboutMenu.SetActive(false);
            aboutMenuOpen = false;
            statsMenu.SetActive(true);
            statsMenuOpen = true;
            controlsMenu.SetActive(false);
            controlsMenuOpen = false;

            statsMenu.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text
                = "Wins: "+ PlayerPrefs.GetInt("WinCount", 0).ToString();
            statsMenu.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text
                = "Deaths: " + PlayerPrefs.GetInt("DeathCount", 0).ToString();
            statsMenu.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text
                = "Enemies Killed: " + PlayerPrefs.GetInt("EnemyKillCount", 0).ToString();
            statsMenu.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text
                = "Synergies Activated: " + PlayerPrefs.GetInt("SynergyCount", 0).ToString();
            statsMenu.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text
                = "Car Accidents: " + PlayerPrefs.GetInt("CarAccidentCount", 0).ToString();
            statsMenu.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text
                = "3rd Degree Burns by Lava: " + PlayerPrefs.GetInt("LavaBurnCount", 0).ToString();
            statsMenu.transform.GetChild(7).GetComponent<TextMeshProUGUI>().text
                = "Treasure Chests Opened: " + PlayerPrefs.GetInt("TreasureCount", 0).ToString();
            statsMenu.transform.GetChild(8).GetComponent<TextMeshProUGUI>().text
                = "Firepits Rested At: " + PlayerPrefs.GetInt("FirepitCount", 0).ToString();
            statsMenu.transform.GetChild(9).GetComponent<TextMeshProUGUI>().text
                = "Gold Barrels Pilfered: " + PlayerPrefs.GetInt("GoldBarrelCount", 0).ToString();
            statsMenu.transform.GetChild(10).GetComponent<TextMeshProUGUI>().text
                = "Goods and Services Purchased: " + PlayerPrefs.GetInt("VendorCount", 0).ToString();
        }
        else
        {
            statsMenu.SetActive(false);
            statsMenuOpen = false;
        }
    }

    public void Controls()
    {
        if (!controlsMenuOpen)
        {
            aboutMenu.SetActive(false);
            aboutMenuOpen = false;
            statsMenu.SetActive(false);
            statsMenuOpen = false;
            controlsMenu.SetActive(true);
            controlsMenuOpen = true;
        }
        else
        {
            controlsMenu.SetActive(false);
            controlsMenuOpen = false;
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}
