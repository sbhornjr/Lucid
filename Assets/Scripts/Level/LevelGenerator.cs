using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class LevelGenerator : MonoBehaviour
{
    // Level specifications
    public int mainPathLength;
    public int totalNumberOfRooms;
    public int desiredBranchLength;
    public int totalNumberOfLoops;
    public int desiredNone;
    public int desiredVendor;
    public int desiredTotemHealth;
    public int desiredTotemGold;
    public int desiredTotemMystery;
    public int desiredTreasure;

    public bool ignoreSeed = true;
    public int seed;
    public string[] level1Templates;
    public string[] level2Templates;
    public string[] level3Templates;

    public const string startRoomTemplate = "template_start"; //"start_room";
    public const string endRoomTemplate = "template_portal"; // "template_boss" "end_room";
    public const string bossTemplate = "template_final";

    Random r;
    int roomCounter;
    List<string> availableTemplates;

    // Generates a level based on class variables
    public Level GenerateLevel()
    {
        if (totalNumberOfRooms < mainPathLength) { totalNumberOfRooms = mainPathLength; } // ensures that the number of rooms requested is high enough
        Dictionary<Vector2, Room> roomAccumulator = new Dictionary<Vector2, Room>();
        if (ignoreSeed) { r = new Random(); }
        else { r = new Random(seed); }
        availableTemplates = GetTemplates();
        roomAccumulator.Add(Vector2.zero, CreateRoom(startRoomTemplate));
        roomCounter = 2;

        CreateMainPath(roomAccumulator);

        if (desiredBranchLength > 0) { CreateBranches(roomAccumulator); } // ensures we can actually create branches

        CreateLoops(roomAccumulator);
        AssignPOI(roomAccumulator);
        AssignTreasure(roomAccumulator);
        return new Level(roomAccumulator);
    }

    // Creates mainPathLength amount of rooms. Labels the last one created as the end room
    private void CreateMainPath(Dictionary<Vector2, Room> level)
    {
        Stack<Vector2> stack = new Stack<Vector2>();
        stack.Push(Vector2.zero);
        Vector2 current;
        Vector2 next;
        string roomTemplate;
        int firstDirection;
        Direction attemptDirection;

        while (roomCounter <= mainPathLength)
        {
            current = stack.Peek();
            next = current; // Dummy assignment - I literally have to set next to something so the code compiles.
            firstDirection = r.Next(4);
            attemptDirection = Direction.N; // necessary for compiling properly. Is literally changed immediately (we love c#)
            for (int i = 0; i < 4; i++)
            {
                attemptDirection = DirectionMethods.NumberToDirection((firstDirection + i) % 4);
                if (!IsRoomHere(level, current, attemptDirection))
                {
                    next = GetPositionInDirection(current, attemptDirection);
                    break;
                }
            }
            // If next is still equal to current we know we weren't able to find a new room and must go back a room and try again
            if (next == current)
            {
                stack.Pop();
            }
            else
            {
                if (roomCounter == mainPathLength)
                {
                    roomTemplate = endRoomTemplate;
                }
                else
                {
                    int templateIndex = r.Next(availableTemplates.Count);
                    roomTemplate = availableTemplates[templateIndex];
                    availableTemplates.RemoveAt(templateIndex);
                    if (availableTemplates.Count == 0)
                    {
                        availableTemplates = GetTemplates();
                    }
                }
                Room nextRoom = CreateRoom(roomTemplate);
                level.Add(next, nextRoom);
                stack.Push(next);
                AddConnection(level, current, attemptDirection);
                roomCounter++;
            }
        }
    }

    // Creates branches until we have satisfied totalNumberOfRooms. Branches stop either at desiredBranchLength or until they get 'stuck'
    private void CreateBranches(Dictionary<Vector2, Room> level)
    {
        Vector2[] roomPool = level.Keys.ToArray<Vector2>();
        Vector2 current;
        Vector2 next;
        int firstDirection;
        Direction attemptDirection;

        while (roomCounter <= totalNumberOfRooms)
        {
            // Pick a random room
            current = roomPool[r.Next(0, roomPool.Length)];
            next = current;

            // Create a branch
            for (int i = 0; i < desiredBranchLength; i++)
            {
                firstDirection = r.Next(4);
                attemptDirection = Direction.N; // necessary for compiling properly. Is literally changed immediately (we love c#)
                for (int j = 0; j < 4; j++)
                {
                    attemptDirection = DirectionMethods.NumberToDirection((firstDirection + i) % 4);
                    if (!IsRoomHere(level, current, attemptDirection))
                    {
                        next = GetPositionInDirection(current, attemptDirection);
                        break;
                    }
                }
                // room cannot be created - just stop the branch
                if (next == current)
                {
                    break;
                }
                else
                {
                    int templateIndex = r.Next(availableTemplates.Count);
                    Room nextRoom = CreateRoom(availableTemplates[templateIndex]);
                    availableTemplates.RemoveAt(templateIndex);
                    if (availableTemplates.Count == 0)
                    {
                        availableTemplates = GetTemplates();
                    }
                    level.Add(next, nextRoom);
                    AddConnection(level, current, attemptDirection);
                    current = next;
                    roomCounter++;
                    if (roomCounter > totalNumberOfRooms) { break; }
                }
            }
        }
    }

    // Creates totalNumberofLoops amount of loops in the given level
    private void CreateLoops(Dictionary<Vector2, Room> level)
    {
        // Get a list of all Positions, and then randomize it
        Vector2[] roomPool = level.Keys.ToArray<Vector2>();
        RandomizeArray<Vector2>(roomPool);
        int loopCounter = 0;
        int firstDirection;
        int randDirection;
        Direction attemptDirection;

        foreach (Vector2 current in roomPool)
        {
            if (loopCounter >= totalNumberOfLoops)
            {
                break;
            }

            firstDirection = r.Next(4);
            randDirection = firstDirection; // necessary for compiling properly. Is literally changed immediately (we love c#)
            for (int i = 0; i < 4; i++)
            {
                randDirection = (firstDirection + i) % 4;
                attemptDirection = DirectionMethods.NumberToDirection(randDirection);
                if (IsRoomHere(level, current, attemptDirection))
                {
                    if (!IsConnected(level, current, attemptDirection))
                    {
                        AddConnection(level, current, attemptDirection);
                        loopCounter++;
                    }
                }
            }
        }
    }

    // Randomly assign the requested points of interest to each of the rooms. Currently, rooms may not have POIs, so sometimes the request can get lost, but thats RNG, baby.
    private void AssignPOI(Dictionary<Vector2, Room> level)
    {
        int[] POICounts = new int[5] { desiredNone, desiredVendor, desiredTotemHealth, desiredTotemGold, desiredTotemMystery };
        // Get a list of all Positions, and then randomize it
        Vector2[] roomPool = level.Keys.ToArray<Vector2>();
        RandomizeArray<Vector2>(roomPool);
        int currentPOIIndex = 0;
        int currentPOIIndexCount = 0;

        foreach (Vector2 roomPos in roomPool)
        {
            if (currentPOIIndex < POICounts.Length)
            {
                if (currentPOIIndexCount < POICounts[currentPOIIndex])
                {
                    POIType toBeAssigned;

                    switch (currentPOIIndex)
                    {
                        case 0:
                            toBeAssigned = POIType.NONE;
                            break;
                        case 1:
                            toBeAssigned = POIType.Vendor;
                            break;
                        case 2:
                            toBeAssigned = POIType.TotemHealth;
                            break;
                        case 3:
                            toBeAssigned = POIType.TotemGold;
                            break;
                        default:
                            toBeAssigned = POIType.TotemMystery;
                            break;

                    }
                    level[roomPos].assignedPOI = toBeAssigned;
                    //Debug.Log("Assigned room " + level[roomPos].templateFileName + " the " + toBeAssigned.ToString());
                    currentPOIIndexCount++;
                }
                
                if (currentPOIIndexCount >= POICounts[currentPOIIndex])
                {
                    currentPOIIndex++;
                    currentPOIIndexCount = 0;
                }
            }
            else
            {
                break;
            }
        }
    }

    private void AssignTreasure(Dictionary<Vector2, Room> level)
    {
        Vector2[] roomPool = level.Keys.ToArray<Vector2>();
        RandomizeArray<Vector2>(roomPool);
        int treasureCount = 0;

        foreach (Vector2 roomPos in roomPool)
        {
            level[roomPos].hasTreasure = true;
            treasureCount++;
            //Debug.Log("Assigned room " + level[roomPos].templateFileName + " treasure");

            if (treasureCount >= desiredTreasure)
            {
                break;
            }
        }
    }

    // Adds a connection in the direction from the parent in the level. Presumes a node exists in that spot
    private void AddConnection(Dictionary<Vector2, Room> level, Vector2 parent, Direction direction)
    {
        level[parent].CreateConnection(direction);
        switch (direction)
        {
            case Direction.N:
                level[new Vector2(parent.x, parent.y + 1)].CreateConnection(Direction.S);
                break;
            case Direction.E:
                level[new Vector2(parent.x + 1, parent.y)].CreateConnection(Direction.W);
                break;
            case Direction.S:
                level[new Vector2(parent.x, parent.y - 1)].CreateConnection(Direction.N);
                break;
            default:
                level[new Vector2(parent.x - 1, parent.y)].CreateConnection(Direction.E);
                break;
        }
    }

    // Determines if there is a Room in the given direction in the level from the parent
    private bool IsRoomHere(Dictionary<Vector2, Room> level, Vector2 parent, Direction direction)
    {
        Vector2 attempt;
        switch (direction)
        {
            case Direction.N:
                attempt = new Vector2(parent.x, parent.y + 1);
                break;
            case Direction.E:
                attempt = new Vector2(parent.x + 1, parent.y);
                break;
            case Direction.S:
                attempt = new Vector2(parent.x, parent.y - 1);
                break;
            default:
                attempt = new Vector2(parent.x - 1, parent.y);
                break;
        }
        return level.ContainsKey(attempt);
    }

    // Returns whether or not the given parent Vector is connected to the Node in the given direction. Presumes that the level contains such a node
    private bool IsConnected(Dictionary<Vector2, Room> level, Vector2 parent, Direction direction)
    {
        switch (direction)
        {
            case Direction.N:
                return level[parent].HasDirection(direction) || level[new Vector2(parent.x, parent.y + 1)].HasDirection(Direction.S);
            case Direction.E:
                return level[parent].HasDirection(direction) || level[new Vector2(parent.x + 1, parent.y)].HasDirection(Direction.W);
            case Direction.S:
                return level[parent].HasDirection(direction) || level[new Vector2(parent.x, parent.y - 1)].HasDirection(Direction.N);
            default:
                return level[parent].HasDirection(direction) || level[new Vector2(parent.x - 1, parent.y)].HasDirection(Direction.E);
        }
    }

    // Gets the Vector that is in the given direction
    private Vector2 GetPositionInDirection(Vector2 parent, Direction direction)
    {
        switch (direction)
        {
            case Direction.N:
                return new Vector2(parent.x, parent.y + 1);
            case Direction.E:
                return new Vector2(parent.x + 1, parent.y);
            case Direction.S:
                return new Vector2(parent.x, parent.y - 1);
            default:
                return new Vector2(parent.x - 1, parent.y);
        }
    }

    // Returns the available templates. Will probably load a json file
    private List<string> GetTemplates()
    {
        switch (GameStateMachine.CurrentLevel)
        {
            case GameStateMachine.Level.Dungeon:
                return level1Templates.ToList();
            case GameStateMachine.Level.City:
                return level2Templates.ToList();
            case GameStateMachine.Level.Hell:
                return level3Templates.ToList();
            default:
                return level1Templates.ToList(); // default to level1 templates since they dont have anything tricky goin on
        }
    }

    // Randomizes Array of type T using Fisher-Yates
    private void RandomizeArray<T>(T[] roomPool)
    {
        for (int i = 0; i < roomPool.Length; i++)
        {
            int j = r.Next(i, roomPool.Length);
            T temp = roomPool[i];
            roomPool[i] = roomPool[j];
            roomPool[j] = temp;
        }
    }

    // Creates a Node. Will change based on Sam's implementation
    private Room CreateRoom(string roomTemplate)
    {
        return new Room(roomTemplate);
    }

    public Level BossLevel()
    {
        Dictionary<Vector2, Room> roomAccumulator = new Dictionary<Vector2, Room>();
        if (ignoreSeed) { r = new Random(); }
        else { r = new Random(seed); }
        availableTemplates = GetTemplates();
        roomAccumulator.Add(Vector2.zero, CreateRoom(bossTemplate));

        return new Level(roomAccumulator);
    }
}
