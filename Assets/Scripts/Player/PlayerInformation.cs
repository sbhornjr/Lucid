using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInformation : MonoBehaviour
{

    public int health;
    public int maxhealth;
    public List<GameObject> equipments;
    public float healthPercentage;

    // Start is called before the first frame update
    void Start()
    {
      // health initialization may be more dynamic in the future, for example:
      // health may be determined on items, game modifiers, player class, etc
      maxhealth = 100;
      health = 100;

      equipments = new List<GameObject>();

      healthPercentage = (float)health / (float)maxhealth;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
