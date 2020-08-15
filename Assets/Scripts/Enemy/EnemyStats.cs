using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/**
 * the stats of an enemy
 */ 
public class EnemyStats : MonoBehaviour
{ 
    public EnemyWeapon weapon;
    public AudioClip deadSFX;

    [SerializeField]
    int maxHealth = 20;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private TextMeshProUGUI healthText;

    [SerializeField]
    private Canvas healthBarCanvas;

    public int CurrentHealth { get; private set; }

    public int GoldToTake = 0;

    /**
     * take the specified amount of damage
     */ 
    public void TakeDamage(int dmg)
    {
        if (CurrentHealth > 0)
        {
            CurrentHealth = (int)Mathf.Clamp(CurrentHealth - dmg, 0, Mathf.Infinity); 
        }

        healthText.text = CurrentHealth.ToString() + "/" + maxHealth.ToString();
        slider.value = CurrentHealth;
    }

    /**
     * starting combat - set necessary fields and UI values
     */ 
    public void StartCombat()
    {
        weapon.StartCombat();
        CurrentHealth = maxHealth;
        healthText.text = CurrentHealth.ToString() + "/" + maxHealth.ToString();
        slider.value = CurrentHealth;
        slider.maxValue = CurrentHealth;
        healthBarCanvas.GetComponent<Canvas>().enabled = true;
    }

    public void LengthenTime(int numLengthenedFor)
    {
        weapon.LengthenTime(numLengthenedFor);
    }
}
