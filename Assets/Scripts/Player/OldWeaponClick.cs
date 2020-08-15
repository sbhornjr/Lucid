using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

/**
 * this is part of the UI element showing the player's old weapons
 */ 
public class OldWeaponClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    PlayerStats playerStats;

    NewWeaponClick newWeaponClick;

    public GameObject playerWeapons;

    public int index { get; set; }
    public Weapon weapon { get; set; }

    [SerializeField]
    AudioClip errorSFX, clickSFX;

    /**
     * get necessary references
     */ 
    private void Awake()
    {
        index = 0;
        playerStats = FindObjectOfType<PlayerStats>();
    }

    /**
     * the player swapped me out :( -> swap the weapons and set up the UI to reflect it
     */ 
    public void OnPointerClick(PointerEventData eventData)
    {
        newWeaponClick = GameObject.FindGameObjectWithTag("Clicked").GetComponent<NewWeaponClick>();
        if (!newWeaponClick.isNull)
        {
            if (newWeaponClick.weapon.Stats.fromVendor && playerStats.gold < newWeaponClick.weapon.Stats.price)
            {
                GameObject.FindGameObjectWithTag("NotEnoughGold").GetComponent<TextMeshProUGUI>().enabled = true;
                Invoke("DisableGoldText", 1);
                AudioSource.PlayClipAtPoint(errorSFX, Camera.main.transform.position);
            }
            else
            {
                playerStats.SwapWeapons(index);
                newWeaponClick.SetUpUI(weapon);
                playerStats.SetupWeaponUIs(playerWeapons);
            }
        }

        newWeaponClick.gameObject.tag = "Untagged";
        playerWeapons.SetActive(false);
    }

    void DisableGoldText()
    {
        GameObject.FindGameObjectWithTag("NotEnoughGold").GetComponent<TextMeshProUGUI>().enabled = false;
    }

    /**
     * player's mouse is over me -> show stats instead of name
     */
    public void OnPointerEnter(PointerEventData eventData)
    {
        gameObject.transform.GetChild(1).gameObject.SetActive(false);
        gameObject.transform.GetChild(2).gameObject.SetActive(true);
    }

    /**
     * player's mouse left me -> show the name instead of stats
     */
    public void OnPointerExit(PointerEventData eventData)
    {
        gameObject.transform.GetChild(2).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).gameObject.SetActive(true);
    }

    /**
     * set up my UI with a new weapon.
     * this is called if the player swaps out my original weapon with a new one
     * -> the old weapon is put into its place so they can always switch back.
     */
    public void SetUpUI(Weapon weapon)
    {
        var beforeHover = transform.GetChild(1);
        var afterHover = transform.GetChild(2);

        // this is an empty slot -> wipe values
        if (weapon == null)
        {
            transform.GetChild(0).GetComponent<Image>().color = Color.gray;
            beforeHover.GetChild(1).gameObject.GetComponent<RectTransform>().sizeDelta
                    = new Vector2(0, 0);

            beforeHover.GetComponentInChildren<TextMeshProUGUI>().text = "";
            foreach (TextMeshProUGUI text in afterHover.GetComponentsInChildren<TextMeshProUGUI>())
                text.text = "";
        }
        // this is a weapon -> set the sprite, name, damage, cooldown, color
        else
        {
            transform.GetChild(0).GetComponent<Image>().color = weapon.Stats.color;
            beforeHover.GetChild(1).GetComponent<Image>().sprite = weapon.Stats.sprite;
            beforeHover.GetChild(1).gameObject.GetComponent<RectTransform>().sizeDelta
                    = new Vector2(80, 80);
            transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = weapon.Stats.name;
            afterHover.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text
                        = "Damage: " + weapon.Stats.dmg;
            afterHover.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text
                = "Cooldown: " + weapon.Stats.cooldown;
            afterHover.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text
                = "Synergy: " + weapon.Stats.SynergyToString();
            afterHover.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text
                = "Rating: " + weapon.Stats.level.ToString();
            afterHover.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text
                = "Type: " + weapon.Stats.type.ToString();
        }
    }
}
