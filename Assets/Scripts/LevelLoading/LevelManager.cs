using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] enemyPrefabs1, enemyPrefabs2, enemyPrefabs3, enemyPrefabsBoss,
        treasureUninteractedPrefabs, treasureInteractedPrefabs;

    [SerializeField]
    private GameObject lavaParticleSystemPrefab, portalParticleSystemPrefab;

    [SerializeField]
    private GameObject treasureParent, poiParent, enemyParent, carParent, propParent, particleSystemParent;

    [SerializeField]
    private GameObject[] poiVendorPrefabs, poiTotemGoldPrefabs, poiTotemHealthPrefabs;

    [SerializeField]
    private GameObject[] propDungeonPrefabs, propCityPrefabs, propHellPrefabs;

    [SerializeField]
    private GameObject carPrefab; 

    [SerializeField]
    private GameObject treasureUI, healthTotemUI, goldTotemUI, vendorUI;

    [SerializeField]
    Material level1Background, level2Background, level3Background;

    public PlayerMovement playerMovement;
    PlayerStats playerStats;

    private MinimapRenderer minimapRenderer;

    //Dictionary<Vector2, Room> roomsDict;
    Level level;
    Vector2 currentCoordinates;
    RoomGridMesh roomGridMesh;

    private IDictionary<POIType, GameObject[]> poiPrefabs; 

    private Room CurrentRoom { get { return level.Get(currentCoordinates); } }

    [SerializeField]
    GameObject weaponPrefab;
     
    void Awake()
    {
        roomGridMesh = FindObjectOfType<RoomGridMesh>();
        playerMovement = FindObjectOfType<PlayerMovement>();
        playerStats = FindObjectOfType<PlayerStats>();
        minimapRenderer = FindObjectOfType<MinimapRenderer>();

        // Zip up POI prefabs (assume equal length)
        poiPrefabs = ZipPOIPrefabs();
    }

    private IDictionary<POIType, GameObject[]> ZipPOIPrefabs()
    {
        return new Dictionary<POIType, GameObject[]>
        {
            { POIType.Vendor, poiVendorPrefabs },
            { POIType.TotemGold, poiTotemGoldPrefabs },
            { POIType.TotemHealth, poiTotemHealthPrefabs }
        };
    }

    // get the level from the level loader
    public void AcceptLevel(Level l)
    {
        // Cleanup the previous level, if necessary
        if (level != null)
        {
            var objsToDestroy = level.Cleanup();

            foreach (var obj in objsToDestroy)
            {
                Destroy(obj);
            }

            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                Destroy(enemy);
            }
        }

        level = l;
        currentCoordinates = Vector2.zero;

        switch (GameStateMachine.CurrentLevel)
        {
            case GameStateMachine.Level.Dungeon:
                RenderSettings.skybox = level1Background;
                break;
            case GameStateMachine.Level.City:
                RenderSettings.skybox = level2Background;
                break;
            default:
                RenderSettings.skybox = level3Background;
                break;
        }

        RenderLevelOnMinimap();
    }

    public void LoadCurrentRoom(Direction directionOfEntry)
    {
        // If enemies have already been loaded, grab them from the room
        if (CurrentRoom.HasRoomBeenLoaded)
        {
            foreach (var enemy in CurrentRoom.EnemiesInRoom)
            {
                enemy.SetActive(true);
            }
            foreach (var treasure in CurrentRoom.Treasures)
            {
                treasure.Value.GameObject.SetActive(true);
            }
            foreach (var poi in CurrentRoom.POIs)
            {
                poi.Value.GameObject.SetActive(true);
            }
            foreach (var car in CurrentRoom.Cars)
            {
                car.Value.GameObject.SetActive(true);
            }
            foreach (var lava in CurrentRoom.LavaFX)
            {
                lava.Value.GameObject.SetActive(true);
            } 
            foreach (var portal in CurrentRoom.PortalFX)
            {
                portal.Value.GameObject.SetActive(true);
            }

            CurrentRoom.ReloadRoom();
        }
        else
        {
            LoadAndPopulateRoom();
        }

        // Move the player to the door
        playerMovement.InitPosition(CurrentRoom.GetIndexOfDoor(directionOfEntry));
    }

    public void LoadCurrentRoom()
    {
        LoadAndPopulateRoom();

        playerMovement.InitPosition(60);
        if (GameStateMachine.CurrentLevel == GameStateMachine.Level.Boss)
        {
            playerMovement.InitPosition(38);
        }
    }

    private void LoadAndPopulateRoom()
    {
        GameObject[] enemyPrefabs;
        switch(GameStateMachine.CurrentLevel)
        {
            case GameStateMachine.Level.Dungeon:
                enemyPrefabs = enemyPrefabs1; 
                break;
            case GameStateMachine.Level.City:
                enemyPrefabs = enemyPrefabs2; 
                break;
            case GameStateMachine.Level.Hell:
                enemyPrefabs = enemyPrefabs3; 
                break;
            case GameStateMachine.Level.Boss:
                enemyPrefabs = enemyPrefabsBoss;
                break;
            default:
                throw new Exception();
        }

        var objects = CurrentRoom.LoadFromTemplate(enemyPrefabs, treasureUninteractedPrefabs, 
            poiPrefabs, lavaParticleSystemPrefab, portalParticleSystemPrefab);

        var occupiedIndices = new HashSet<uint>();

        foreach (var obj in objects)
        {
            var prefab = obj.GameObject;
            var index = obj.Index;
            var rotation = obj.Rotation;

            occupiedIndices.Add(index);

            // Create an object on that tile 
            var location = GameUtils.GetPosition(index);
            var inst = Instantiate(prefab, location, rotation);

            // Finish up per-type
            switch (obj.Type)
            {
                case ObjectInstantiationType.Enemy:
                    var movement = inst.GetComponent<EnemyMovement>();
                    movement.InitPosition(index);
                    inst.transform.SetParent(enemyParent.transform);
                    break;
                case ObjectInstantiationType.Treasure:
                    AddObjectInstantiationDataToList(obj, inst, CurrentRoom.Treasures, treasureParent);
                    break;
                case ObjectInstantiationType.POI:
                    // Store the POI, replacing the prefab with the instantiated object
                    AddObjectInstantiationDataToList(obj, inst, CurrentRoom.POIs, poiParent);
                    break; 
                case ObjectInstantiationType.LavaParticleSystem:
                    AddObjectInstantiationDataToList(obj, inst, CurrentRoom.LavaFX, particleSystemParent);
                    break;
                case ObjectInstantiationType.PortalParticleSystem:
                    AddObjectInstantiationDataToList(obj, inst, CurrentRoom.PortalFX, particleSystemParent);
                    break;
            }
        }

        if (GameStateMachine.CurrentLevel == GameStateMachine.Level.City)
        {
            SpawnCars(occupiedIndices);
        }
    }

    private void SpawnCars(HashSet<uint> occupiedIndices)
    {
        // Choose a few valid spots
        var carIndices = new HashSet<uint>();

        do
        {
            var index = (uint)UnityEngine.Random.Range(0, CurrentRoom.tileMap.NumTiles);

            if (!occupiedIndices.Contains(index) && !carIndices.Contains(index)
                && CurrentRoom.tileMap.GetTileType(index) == TileType.Floor
                && index != playerMovement.Index) 
            { 

                carIndices.Add(index);

                var carObject = Instantiate(carPrefab, GameUtils.GetPosition(index), Quaternion.identity);
                carObject.transform.SetParent(carParent.transform);
                var car = carObject.GetComponent<CarMovement>();
                car.InitPosition(index);

                var obj = new ObjectInstantiationData
                {
                    Type = ObjectInstantiationType.Car,
                    GameObject = carObject,
                    Index = index,
                    Rotation = Quaternion.identity
                };
                CurrentRoom.Cars[index] = obj;
            }
        } while (carIndices.Count < 2);
    }

    private void AddObjectInstantiationDataToList(ObjectInstantiationData obj, GameObject inst,
        IDictionary<uint, ObjectInstantiationData> dict, GameObject parent)
    {
        // Override the prefab game object with the instantiation 
        var objWithInst = obj;
        objWithInst.GameObject = inst;
        dict.Add(obj.Index, objWithInst);

        inst.transform.SetParent(parent.transform);
    }

    public void UpdateToNextRoom()
    {
        // Store and disable enemies
        CurrentRoom.EnemiesInRoom = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in CurrentRoom.EnemiesInRoom)
        {
            enemy.SetActive(false);
        }

        foreach (var treasure in CurrentRoom.Treasures)
        {
            treasure.Value.GameObject.SetActive(false);
        }
        foreach (var poi in CurrentRoom.POIs)
        {
            poi.Value.GameObject.SetActive(false);
        }
        foreach (var car in CurrentRoom.Cars)
        {
            car.Value.GameObject.SetActive(false);
        }
        foreach (var lava in CurrentRoom.LavaFX)
        {
            lava.Value.GameObject.SetActive(false);
        } 
        foreach (var portal in CurrentRoom.PortalFX)
        {
            portal.Value.GameObject.SetActive(false);
        }

        CurrentRoom.HasRoomBeenLoaded = true;

        var direction = CurrentRoom.directionOfNextRoom;

        currentCoordinates = level.GetPositionInDirection(currentCoordinates, direction);

        LoadCurrentRoom(direction.Opposite());
        RenderCurrentRoom();

        minimapRenderer.UpdateCurrentRoom(currentCoordinates);
    }

    public void RenderCurrentRoom()
    {
        roomGridMesh.Triangulate(level.Get(currentCoordinates));
    }

    private void RenderLevelOnMinimap()
    {
        minimapRenderer.RenderLevel(level, currentCoordinates);
    }

    internal bool EntityCanMoveTo(uint index, Direction direction, out uint maybeNextIndex)
    {
        return CurrentRoom.EntityCanMoveTo(index, direction, out maybeNextIndex);
    }

    internal bool EntityCanInteractWith(uint index, Direction direction, out uint maybeInteractableIndex)
    {
        if (!CurrentRoom.EntityCanInteractWith(index, direction, out maybeInteractableIndex))
        {
            return false;
        }

        // If that is true, find the relevant interactable and make sure it hasn't been clicked already
        if (CurrentRoom.Treasures.ContainsKey(maybeInteractableIndex))
        {
            return !CurrentRoom.Treasures[maybeInteractableIndex].InteractionComplete;
        }
        else if (CurrentRoom.POIs.ContainsKey(maybeInteractableIndex))
        {
            return !CurrentRoom.POIs[maybeInteractableIndex].InteractionComplete;
        }
        else
        {
            throw new ArgumentException($"Neighbor index {maybeInteractableIndex} (from index {index} is interactable," +
                $"but no interactable was found in LevelManager!");
        }
    }

    internal GameObject InteractWith(uint index)
    {
        // Determine what is being interacted with
        if (CurrentRoom.Treasures.ContainsKey(index))
        {
            return InteractWithTreasure(index);
        }
        else if (CurrentRoom.POIs.ContainsKey(index))
        {
            return InteractWithPOI(index);
        }
        else
        {
            throw new ArgumentException($"Index {index} does not contain an interactable!");
        }
    }

    private GameObject InteractWithTreasure(uint index)
    {
        // !TODO open item menu etc
        var treasure = CurrentRoom.Treasures[index];

        var openedPrefab = treasureInteractedPrefabs[treasure.ObjectPrefabInstantiationIndex];
        var location = GameUtils.GetPosition(treasure.Index);

        Destroy(treasure.GameObject);
        var openedTreasure = Instantiate(openedPrefab, location, treasure.Rotation);

        // Mark the interactable as interacted and update its game object to the new instantiation
        treasure.InteractionComplete = true;
        treasure.GameObject = openedTreasure;
        CurrentRoom.Treasures[index] = treasure;    // <-- structs are readonly i think

        // Spawn a random weapon and return the UI to the entity manager
        treasureUI.SetActive(true);
        var weapon = gameObject.Instantiate(weaponPrefab, 0).GetComponent<Weapon>();
        var nwc = treasureUI.GetComponentInChildren<NewWeaponClick>();
        nwc.SetUpUI(weapon);

        var treasureCount = PlayerPrefs.GetInt("TreasureCount", 0);
        PlayerPrefs.SetInt("TreasureCount", ++treasureCount);

        return treasureUI;
    }

    private GameObject InteractWithPOI(uint index)
    {
        var poi = CurrentRoom.POIs[index];

        GameObject uiToReturn;
        switch (poi.POIType)
        {
            case POIType.Vendor:
                vendorUI.SetActive(true);
                NewWeaponClick[] nwcs = vendorUI.GetComponentsInChildren<NewWeaponClick>(); 
                for (var i = 0; i < 3; ++i)
                {
                    nwcs[i].SetUpUI(gameObject.Instantiate(weaponPrefab, i + 1 + (int)GameStateMachine.CurrentLevel, true).GetComponent<Weapon>());
                }

                var healButton = vendorUI.transform.GetChild(0).GetChild(4);
                healButton.GetComponent<ButtonClick>().ResetThings();

                uiToReturn = vendorUI;

                poi.GameObject.SetActive(false);

                var vendorCount = PlayerPrefs.GetInt("VendorCount", 0);
                PlayerPrefs.SetInt("VendorCount", ++vendorCount);

                break;

            case POIType.TotemHealth:
                healthTotemUI.SetActive(true);
                foreach (ButtonClick bc in healthTotemUI.GetComponentsInChildren<ButtonClick>()) bc.Enable();
                uiToReturn = healthTotemUI;

                poi.GameObject.GetComponentInChildren<FlickerLight>().Active = false;
                var smonk = poi.GameObject.GetComponentInChildren<ParticleSystem>();
                smonk.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                var smonkMain = smonk.main;
                smonkMain.playOnAwake = false;

                var firepitCount = PlayerPrefs.GetInt("FirepitCount", 0);
                PlayerPrefs.SetInt("FirepitCount", ++firepitCount);

                break;

            case POIType.TotemGold:
                var gold = UnityEngine.Random.Range(40, 60);
                playerStats.GainGold(gold);
                goldTotemUI.SetActive(true);
                goldTotemUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text
                    = "You have gained " + gold.ToString() + " gold";
                uiToReturn = goldTotemUI;
                 
                poi.GameObject.GetComponentInChildren<FlickerLight>().Active = false;
                 
                var goldBarrelCount = PlayerPrefs.GetInt("GoldBarrelCount", 0);
                PlayerPrefs.SetInt("GoldBarrelCount", ++goldBarrelCount);
                
                break;

            case POIType.TotemMystery:
            case POIType.NONE:
            default:
                throw new ArgumentException($"POI type {poi.POIType} unimplemented!");
        }

        // Mark the interactable as interacted and update its game object to the new instantiation
        poi.InteractionComplete = true; 
        CurrentRoom.POIs[index] = poi;    // <-- structs are readonly i think

        return uiToReturn;
    }

    internal bool IsEntityOnDoor(uint playerIndex)
    {
        return CurrentRoom.IsEntityOnDoor(playerIndex);
    }

    internal bool IsEntityOnPortal(uint playerIndex)
    {
        return CurrentRoom.IsEntityOnPortal(playerIndex);
    }

    internal bool IsEntityOnLava(uint playerIndex)
    {
        return CurrentRoom.IsEntityOnLava(playerIndex);
    }
    
}
