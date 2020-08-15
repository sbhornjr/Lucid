using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillItems : MonoBehaviour
{
    private void OnDisable()
    {
        foreach(NewItemClick nic in gameObject.GetComponentsInChildren<NewItemClick>())
        {
            if (nic.item != null) Item.exists.Remove(nic.item.itemEffect.ToString());
        }
    }
}
