using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

/**
 * UI element for a new weapon that might be picked up in the post-combat menu
 */
public class NewItemClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    PlayerStats playerStats;

    [SerializeField]
    GameObject playerItems;

    public bool isNull;

    public Item item;

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
     * i have been clicked -> show the player's current items list
     */
    public void OnPointerClick(PointerEventData eventData)
    {
        SetItemList(true);
        playerStats.SetupItemUIs(playerItems);
        gameObject.tag = "ItemClicked";

        if (CombatManager.inCombat) AudioSource.PlayClipAtPoint(clickSFX, combatCamera.position);
        else AudioSource.PlayClipAtPoint(clickSFX, Camera.main.transform.position);

        // tell the player it has a possible new item
        playerStats.PossibleNewItem(item);
    }

    /**
     * show the weapon list UI element
     */
    public void SetItemList(bool which)
    {
        playerItems.SetActive(which);
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
    public void SetUpUI(Item item)
    {
        this.item = item;
        var beforeHover = transform.GetChild(1);
        var afterHover = transform.GetChild(2);

        // this is an empty slot -> wipe values
        if (item == null)
        {
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
            Vector2 size = new Vector2(60, 60);
            if (item.fromVendor) size = new Vector2(40, 40);
            beforeHover.GetChild(1).GetComponent<Image>().sprite = item.sprite;
            beforeHover.GetChild(1).gameObject.GetComponent<RectTransform>().sizeDelta
                    = size;
            transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = item.name;
            afterHover.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text
                        = item.description;

            isNull = false;

            if (item.fromVendor)
            {
                var cost = transform.GetChild(3);
                cost.GetComponentInChildren<TextMeshProUGUI>().text = item.price.ToString();
            }
        }
    }
}
