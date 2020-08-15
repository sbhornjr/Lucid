using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
 * handles the creation of the UI's combat UI element
 */ 
public class WeaponRenderer
{ 
    public TextMeshProUGUI timeText;
    public Slider slider;
    public GameObject ui;
    public TextMeshProUGUI attackText, multiplierText;
    public Image timeShortenImage;
     
    public KeyCode keyCode;

    /**
     * constructs the UI element
     * ui - actual object that is the ui element
     * stats - the weapon's stats
     * keyCode - the weapon's keyCode
     * index - the weapon's index in the player's weapon list - used to know where to place the UI
     */ 
    public WeaponRenderer(GameObject ui, WeaponStats stats, KeyCode keyCode, int index)
    {
        this.ui = ui;
        this.keyCode = keyCode;

        ui.transform.GetChild(0).gameObject.GetComponent<Image>().color = stats.color;
        ui.transform.GetChild(2).gameObject.GetComponent<Image>().sprite = stats.sprite;
        ui.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = keyCode.ToString();

        timeText = ui.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>();
        timeText.text = stats.cooldown.ToString();

        attackText = ui.transform.GetChild(5).gameObject.GetComponent<TextMeshProUGUI>();
        attackText.text = stats.attackName + "!";

        multiplierText = ui.transform.GetChild(6).gameObject.GetComponent<TextMeshProUGUI>();
        timeShortenImage = ui.transform.GetChild(7).gameObject.GetComponent<Image>();

        slider = ui.GetComponent<Slider>();
        slider.maxValue = stats.cooldown;
        slider.value = 0;

        ui.transform.SetParent(GameObject.FindGameObjectWithTag("CombatUIWeapons").transform, false);

        if (index != 0)
            ui.GetComponent<RectTransform>().position
                = new Vector3(ui.GetComponent<RectTransform>().position.x + (275 * index), 5, 0);
    }
}
