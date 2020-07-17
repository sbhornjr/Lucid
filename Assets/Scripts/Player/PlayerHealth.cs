using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{

    private Image hp_bar;
    private GameObject player;
    private int playerHealthPercentage;

    // Start is called before the first frame update
    void Start()
    {
      hp_bar = GetComponent<Image>();
      hp_bar.fillAmount = 1f;

      player = GameObject.FindWithTag("Player");


    }

    // Update is called once per frame
    void Update()
    {
      float playerHealthPercentage = player.GetComponent<PlayerInformation>().healthPercentage;
      hp_bar.fillAmount = playerHealthPercentage;
    }
}
