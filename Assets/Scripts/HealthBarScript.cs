using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField]
    private Slider slider;

    [SerializeField]
    private Text healthText;

    private int currentHealth;
    private int maxHealth;

    public void SetMaxHealth(int newMaxHealth, int newHealth)
    {
        currentHealth = newHealth;
        maxHealth = newMaxHealth;
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(newMaxHealth, 80);
        healthText.text = currentHealth.ToString() + "/" + maxHealth.ToString();
        slider.maxValue = newMaxHealth;
        slider.value = newHealth;
    }

    public void SetHealth(int health)
    {
        currentHealth = health;
        healthText.text = currentHealth.ToString() + "/" + maxHealth.ToString();
        slider.value = health;
    }
}
