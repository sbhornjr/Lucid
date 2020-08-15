using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameDataSerializer : MonoBehaviour
{
    [SerializeField]
    private static string fileName = "save.dat";   // make static

    private static PlayerStats playerStats; 

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>(); 
    }

    public static bool HasSaveData()
    {
        var path = Path.Combine(Application.persistentDataPath, fileName);
        return File.Exists(path);
    }

    public static void SaveGame()
    {
        var toSerialize = playerStats.ToSerializable();
        var json = JsonUtility.ToJson(toSerialize);

        var path = Path.Combine(Application.persistentDataPath, fileName);

        using (var writer = new StreamWriter(path, false))
        {
            writer.Write(json);
        } 
    }

    public static GameStateMachine.Level LoadGame()
    {
        string json;

        var path = Path.Combine(Application.persistentDataPath, fileName); 
        using (var reader = new StreamReader(path))
        {
            json = reader.ReadToEnd();
        }

        var obj = JsonUtility.FromJson<SerializablePlayerStats>(json);
        return playerStats.FromSerializable(obj);
    } 
}
