using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleRoom : MonoBehaviour
{

    private string[] rooms = new string[] {
      "Assets/Templates/sample_template.json",
      "Assets/Templates/sample_template.json",
      "Assets/Templates/sample_template.json",
      "Assets/Templates/sample_template.json"
    };

    private int roomIndex = 0;

    private GameObject RoomGrid;

    private string currentRoom;

    // Start is called before the first frame update
    void Start()
    {
        RoomGrid = GameObject.Find("RoomGrid");
        currentRoom = rooms[0]; // initial room
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C) && roomIndex == 3)
        {
          roomIndex = 0; // cycles back to the first room
          currentRoom = rooms[roomIndex];
          RoomGrid.GetComponent<RoomGridGeneration>().setTemplatePath(currentRoom);
        } else if (Input.GetKeyDown(KeyCode.C) && roomIndex < 3) {
          roomIndex++;
          currentRoom = rooms[roomIndex];
          RoomGrid.GetComponent<RoomGridGeneration>().setTemplatePath(currentRoom);
        }
    }
}
