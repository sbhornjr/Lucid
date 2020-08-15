using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldManager : MonoBehaviour
{ 
    EntityManager entityManager;

    void Awake()
    { 
        entityManager = FindObjectOfType<EntityManager>();
    } 
    
    public void Init()
    {
        entityManager.FindSpawnedEntities();
        entityManager.RoomEntered();
        entityManager.currentState = EntityManager.MovementStates.WaitingForPlayerInput;
    }

    public void Unload()
    {

    }

    public GameStateMachine.GameState Update2ElectricBoogaloo()
    { 
        // Must update entities 
        return entityManager.Update2ElectricBoogaloo(); 
    }
}
