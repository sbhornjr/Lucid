using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

/**
 * representation of an enemy's weapon (slightly different than the player's weapon)
 */
public class EnemyWeapon : MonoBehaviour
{
    public string attackName = "Slash";
    public int dmg = 5;

    [SerializeField]
    int timeToAttack = 5;

    int actualTime = 5;
     
    [SerializeField]
    TextMeshProUGUI attackText;
    [SerializeField]
    Image timeLengthenImage;

    int numLengthenedFor;

    float cooldown;

    /**
     * get necessary references
     */ 
    private void Awake()
    {
        //Debug.Log($"Time to attack: {timeToAttack}");
        actualTime = timeToAttack;
        //Debug.Log($"Actual time: {actualTime}");
        numLengthenedFor = 0; 
    }

    /**
     * called every frame by the combat manager
     */ 
    public bool IsWeaponActivated()
    {
        // increment cooldown
        cooldown += Time.deltaTime;
        cooldown = Mathf.Clamp(cooldown, 0, actualTime);

        // if the cooldown is complete then attack the player
        var attack = false;
        if (cooldown >= actualTime)
        {
            // set UI stuff (this doesn't work right now and im mad about it)
            if (attackText != null)
            {
                attackText.enabled = true;
                attackText.text = attackName + "!";
                Invoke("DisableAttackText", 1);
            }
            
            if (numLengthenedFor > 0)
            {
                numLengthenedFor -= 1;
                if (numLengthenedFor == 0)
                {
                    timeLengthenImage.enabled = false;
                    timeLengthenImage.gameObject.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
                    actualTime = timeToAttack;
                }
            }
            cooldown = 0;
            attack = true;
        }

        return attack;
    }

    internal void LengthenTime(int numLengthenedFor)
    {
        this.numLengthenedFor += numLengthenedFor;
        if (this.numLengthenedFor == numLengthenedFor)
        {
            actualTime *= 2;
            Debug.Log(timeLengthenImage);
            timeLengthenImage.enabled = true;
            var txt = timeLengthenImage.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            txt.enabled = true;
            txt.text = numLengthenedFor.ToString();
        }
    }

    /**
     * starting combat
     */
    public void StartCombat()
    {
        cooldown = 0; 
    }

    /**
     * disable the attack text, called 1 second after it is enabled
     */ 
    void DisableAttackText()
    {
        attackText.enabled = false;
    }
}
