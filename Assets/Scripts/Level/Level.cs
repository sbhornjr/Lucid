using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Level
{
    public uint Width { get; private set; }
    public uint Height { get; private set; }

    public ISet<Hallway> Hallways {get; private set; }

    public IEnumerable<Vector2> RoomLocations { get { return rooms.Keys; } }

    private Dictionary<Vector2, Room> rooms; 

    public Level(Dictionary<Vector2, Room> r)
    {
        rooms = r;
        CalculateWidthAndHeight();

        Hallways = CreateHallways(r);
    }

    private ISet<Hallway> CreateHallways(IDictionary<Vector2, Room> rooms)
    {
        var hallways = new HashSet<Hallway>();

        // BFS to add hallways 
        var Q = new Queue<KeyValuePair<Vector2, Room>>();
        Q.Enqueue(rooms.First());

        var seen = new HashSet<KeyValuePair<Vector2, Room>>();

        while (Q.Count > 0)
        {
            var kvp = Q.Dequeue();
            seen.Add(kvp);

            var room = kvp.Value;
            var loc = kvp.Key;

            for (int i = 0; i < room.Neighbors.Length; i++)
            {
                bool dirBool = room.Neighbors[i];
                if (!dirBool) continue;

                var dir = (Direction)i;
                var neighborLoc = GetPositionInDirection(loc, dir);
                var neighborRoom = rooms[neighborLoc];
                var neighborKVP = new KeyValuePair<Vector2, Room>(neighborLoc, neighborRoom);

                if (!seen.Contains(neighborKVP))
                {
                    Q.Enqueue(neighborKVP);

                    var hallway = new Hallway(room, neighborRoom, loc, neighborLoc);
                    hallways.Add(hallway);

                }
            }
        }

        return hallways;
    }

    private void CalculateWidthAndHeight()
    {
        int minX = 0, maxX = 0, minY = 0, maxY = 0;

        foreach (var coord in rooms.Keys)
        { 
            minX = Math.Min(minX, (int)coord.x);
            minY = Math.Min(minY, (int)coord.y);
            maxX = Math.Max(maxX, (int)coord.x);
            maxY = Math.Max(maxY, (int)coord.y);
        }

        Width = (uint)(maxX - minX) + 1;    // Add 1 to account for zeros
        Height = (uint)(maxY - minY) + 1;
    }

    public IEnumerable<GameObject> Cleanup()
    {
        var objs = new List<GameObject>();

        // Clean all levels
        foreach (var room in rooms)
        {
            objs.AddRange(room.Value.Cleanup());
        }

        return objs;
    }

    public bool Contains(Vector2 location)
    {
        return rooms.ContainsKey(location);
    }

    public Room Get(Vector2 location)
    {
        if (Contains(location))
        { 
            return rooms[location];
        }
        else
        {
            throw new Exception("Level.Get(Vector2 location): There is no room at " + location.ToString());
        }
    }

    public Vector2 GetPositionInDirection(Vector2 location, Direction direction)
    {
        Vector2 attempt;

        switch (direction)
        {
            case Direction.N:
                attempt = new Vector2(location.x, location.y + 1);
                break;
            case Direction.E:
                attempt = new Vector2(location.x + 1, location.y);
                break;
            case Direction.S:
                attempt = new Vector2(location.x, location.y - 1);
                break;
            default:
                attempt = new Vector2(location.x - 1, location.y);
                break;
        }
        if (Contains(attempt))
        {
            return attempt;
        }
        else
        {
            throw new Exception("Level.Travel(Vector2 location): There is no room at " + location.ToString());
        }
    }

    public class Hallway
    {
        public Room RoomA { get; private set; }
        public Room RoomB { get; private set; }

        public Vector2 LocationOfRoomA { get; private set; }
        public Vector2 LocationOfRoomB { get; private set; }

        public Hallway(Room a, Room b, Vector2 locA, Vector2 locB)
        {
            RoomA = a; RoomB = b;
            LocationOfRoomA = locA; LocationOfRoomB = locB;
        }
    }
}
