using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

/**
 * this is part of the UI element showing the player's old weapons
 */
public class OldItemClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    PlayerStats playerStats;

    NewItemClick newItemClick;

    public GameObject playerItems;

    public int index { get; set; }
    public Item item { get; set; }

    [SerializeField]
    AudioClip errorSFX;

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
        newItemClick = GameObject.FindGameObjectWithTag("ItemClicked").GetComponent<NewItemClick>();
        if (!newItemClick.isNull)
        {
            if (newItemClick.item.fromVendor && playerStats.gold < newItemClick.item.price)
            {
                GameObject.FindGameObjectWithTag("NotEnoughGold").GetComponent<TextMeshProUGUI>().enabled = true;
                Invoke("DisableGoldText", 1);
                AudioSource.PlayClipAtPoint(errorSFX, Camera.main.transform.position);
            }
            else
            {
                playerStats.SwapItems(index);
                newItemClick.SetUpUI(item);
                playerStats.SetupItemUIs(playerItems);
            }
        }

        newItemClick.gameObject.tag = "Untagged";
        newItemClick.SetItemList(false);
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
    public void SetUpUI(Item item)
    {
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
        }
        // this is a weapon -> set the sprite, name, damage, cooldown, color
        else
        {
            beforeHover.GetChild(1).GetComponent<Image>().sprite = item.sprite;
            beforeHover.GetChild(1).gameObject.GetComponent<RectTransform>().sizeDelta
                    = new Vector2(60, 60);
            transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = item.name;
            afterHover.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text
                        = item.description;
        }
    }
}
