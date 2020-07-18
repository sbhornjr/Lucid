using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartEncounter : MonoBehaviour
{
    [SerializeField]
    private GameObject player;

    [SerializeField]
    private Canvas combatUI;

    [SerializeField]
    private GameController gc;

    private bool playerFirst;

    private EnemyMovement enemy;

    public void PlayerStartEncounter(EnemyMovement em)
    {
        playerFirst = true;
        Begin(em);
    }

    public void EnemyStartEncounter(EnemyMovement em)
    {
        playerFirst = false;
        Begin(em);
    }

    private void Begin(EnemyMovement em)
    {
        enemy = em;

        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<PlayerStats>().enabled = true;

        combatUI.gameObject.SetActive(true);
        enemy.ToggleExcl(false);

        gc.SetMainCamera(false);
        gc.SetCombatCamera(true);
        gc.StartCombat();
    }

    public void DestroyEnemy()
    {
        Destroy(enemy.gameObject);
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<PlayerStats>().enabled = false;
        combatUI.gameObject.SetActive(false);
    }
}
