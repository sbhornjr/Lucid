using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClick : MonoBehaviour
{

    [SerializeField]
    TextMeshProUGUI outOfStock;

    [SerializeField]
    GameObject costText;

    int cost = 100;
    int healAmount = 50;

    public void Enable()
    {
        GetComponent<Button>().enabled = true;
    }

    public void Disable()
    {
        GetComponent<Button>().enabled = false;
    }

    public void EnableComponent(GameObject comp)
    {
        comp.SetActive(true);
    }

    public void DisableComponent(GameObject comp)
    {
        comp.SetActive(false);
    }

    public void HealButton()
    {
        PlayerStats player = FindObjectOfType<PlayerStats>();
        if (!(player.gold < cost))
        {
            player.LoseGold(cost);
            outOfStock.enabled = true;
            player.GainHealth(healAmount);
            costText.SetActive(false);
            Disable();
        }
    }

    public void ResetThings()
    {
        Enable();
        costText.SetActive(true);
        outOfStock.enabled = false;
    }
}
