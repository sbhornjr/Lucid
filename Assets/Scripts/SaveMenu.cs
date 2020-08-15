using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveMenu : MonoBehaviour
{
    public static bool done = false;

    public void SaveGame()
    {
        GameDataSerializer.SaveGame();
        done = true;
    }

    public void DontSave()
    {
        done = true;
    }
}
