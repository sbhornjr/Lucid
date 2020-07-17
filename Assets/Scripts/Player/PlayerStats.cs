using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 100;
    private int currentHealth;

    [SerializeField]
    private HealthBarScript healthBar;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBar = FindObjectOfType<HealthBarScript>();
        healthBar.SetMaxHealth(maxHealth, currentHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            TakeDamage(20);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            IncreaseHealth(20);
        }
    }

    private void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        healthBar.SetHealth(currentHealth);
    }

    private void IncreaseHealth(int health)
    {
        maxHealth += health;
        currentHealth += health;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth, currentHealth);
    }
}
