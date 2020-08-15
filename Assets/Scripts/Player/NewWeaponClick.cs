using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

/**
 * UI element for a new weapon that might be picked up in the post-combat menu
 */ 
public class NewWeaponClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    PlayerStats playerStats;

    [SerializeField]
    GameObject playerWeapons;

    public bool isNull;

    public Weapon weapon;

    [SerializeField]
    AudioClip clickSFX;
    [SerializeField]
    Transform combatCamera;

    /**
     * get a reference to the player
     */
    private void Awake()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        isNull = true;
    }

    /**
     * i have been clicked -> show the player's current weapon list
     */ 
    public void OnPointerClick(PointerEventData eventData)
    {
        SetWeaponList(true);
        playerStats.SetupWeaponUIs(playerWeapons);
        gameObject.tag = "Clicked";

        if (CombatManager.inCombat) AudioSource.PlayClipAtPoint(clickSFX, combatCamera.position);
        else AudioSource.PlayClipAtPoint(clickSFX, Camera.main.transform.position);

        // tell the player it has a possible new weapon
        playerStats.PossibleNewWeapon(weapon);
    }

    /**
     * show the weapon list UI element
     */ 
    public void SetWeaponList(bool which)
    {
        playerWeapons.SetActive(which);
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
     * this is called if the player swaps out my original weapon with one of their own
     * -> the old weapon is put into its place so they can always switch back.
     */ 
    public void SetUpUI(Weapon weapon)
    {
        this.weapon = weapon;
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

            isNull = true;
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

            isNull = false;

            if (weapon.Stats.fromVendor)
            {
                var cost = transform.GetChild(3);
                cost.GetComponentInChildren<TextMeshProUGUI>().text = weapon.Stats.price.ToString();
            }
        }
    }
}
