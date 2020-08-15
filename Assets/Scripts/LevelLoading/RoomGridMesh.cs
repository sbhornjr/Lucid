using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// If you have questions about this shit just ask me lmao
/// </summary>
public class RoomGridMesh : MonoBehaviour
{
    [SerializeField]
    private GameObject[] wallPrefabs, doorPrefabs;

    [SerializeField]
    private GameObject wallAndDoorParent;
     

    private Mesh mMesh;
    private MeshCollider mMeshCollider;

    private List<Vector3> mVertices;
    private List<int> mTriangles; 
    private List<Vector2> mUVs;

    private List<GameObject> walls, doors;

    private void Awake()
    {
        // Assign members
        GetComponent<MeshFilter>().mesh = mMesh = new Mesh();
        mMeshCollider = gameObject.AddComponent<MeshCollider>();

        mVertices = new List<Vector3>();
        mTriangles = new List<int>(); 
        mUVs = new List<Vector2>();

        walls = new List<GameObject>();
        doors = new List<GameObject>();  
    }

    public void Triangulate(Room room)
    {
        var tiles = room.tileMap.GetTiles();

        ClearMeshData();

        // Determine which walls/doors to use
        var wallPrefab = wallPrefabs[(int)GameStateMachine.CurrentLevel];
        var doorPrefab = doorPrefabs[(int)GameStateMachine.CurrentLevel]; 

        foreach (var tile in tiles)
        {
            AddTile(tile, wallPrefab, doorPrefab);
        }

        SetMeshData();
    }
     
    private void AddTile(Tile tile, GameObject wallPrefab, GameObject doorPrefab)
    { 
        Vector2[] uvs;
        switch (tile.Type)
        {
            case TileType.None:
                uvs = GetNoneUVs();
                break; 
            case TileType.Portal:
                uvs = GetPortalUVs();
                break;
            case TileType.Lava:
                uvs = GetLavaUVs();
                break;
            default:
                uvs = GetFloorUVs(GameStateMachine.CurrentLevel);
                break;
        }

        AddQuad(tile.TopRight, tile.TopLeft, tile.BotRight, tile.BotLeft, uvs);

        switch (tile.Type)
        { 
            case TileType.Wall:
                AddWall(tile.Center, wallPrefab);
                break;
            case TileType.Door:
                AddDoor(tile.Center, doorPrefab, tile.DoorDirection);
                break;
            default:
                // Do nothing for the rest
                break;
        }
    }
     
    /// <summary>
    /// The uv offset of the top-left index based on the rotation amount.
    /// </summary>
    private readonly int[] rotationIndices = new int[] { 0, 2, 3, 1 };

    private Vector2[] GetFloorUVs(GameStateMachine.Level level)
    {
        switch (level)
        {
            case GameStateMachine.Level.Dungeon:
                return GetRandomConcreteUVs(UnityEngine.Random.Range(0, 4)); 
            case GameStateMachine.Level.City:
                return GetRandomAsphaltUVs(UnityEngine.Random.Range(0, 4));
            case GameStateMachine.Level.Hell:
                return GetRandomAsphaltUVs(UnityEngine.Random.Range(0, 4));
            case GameStateMachine.Level.Boss:
                return GetRandomAsphaltUVs(UnityEngine.Random.Range(0, 4));
            default:
                throw new ArgumentException($"Level {level} has not been implemented in GetFloorUVs()!");
        }
    }

    /// <summary>
    /// Order is (topleft, topright, botleft, botright)
    /// </summary> 
    private Vector2[] GetRandomConcreteUVs(int numRotations)
    {
        var uvs = new Vector2[4];
        
        var rotation = UnityEngine.Random.Range(0, 4);
        var indices = new int[4] 
        { 
            rotationIndices[rotation],
            rotationIndices[(rotation + 3) % 4],    // top-right is 3-offset from top-left
            rotationIndices[(rotation + 1) % 4],    // top-right is 1-offset from top-left 
            rotationIndices[(rotation + 2) % 4]     // top-right is 2-offset from top-left
        }; 

        switch (numRotations)
        {
            case 0:
                uvs[indices[0]] = new Vector2(0, 1);
                uvs[indices[1]] = new Vector2(0.125f, 1);
                uvs[indices[2]] = new Vector2(0, 0.75f);
                uvs[indices[3]] = new Vector2(0.125f, 0.75f);
                break;
            case 1:
                uvs[indices[0]] = new Vector2(0.125f, 1);
                uvs[indices[1]] = new Vector2(0.25f, 1);
                uvs[indices[2]] = new Vector2(0.125f, 0.75f);
                uvs[indices[3]] = new Vector2(0.25f, 0.75f);
                break;
            case 2:
                uvs[indices[0]] = new Vector2(0, 0.75f);
                uvs[indices[1]] = new Vector2(0.125f, 0.75f);
                uvs[indices[2]] = new Vector2(0, 0.5f);
                uvs[indices[3]] = new Vector2(0.125f, 0.5f);
                break;
            case 3:
                uvs[indices[0]] = new Vector2(0.125f, 0.75f);
                uvs[indices[1]] = new Vector2(0.25f, 0.75f);
                uvs[indices[2]] = new Vector2(0.125f, 0.5f);
                uvs[indices[3]] = new Vector2(0.25f, 0.5f);
                break;
            default:
                throw new ArgumentException($"Bad concrete UV specifier: {numRotations}");
        }

        return uvs;
    }
     
    private Vector2[] GetRandomAsphaltUVs(int numRotations)
    {
        var uvs = new Vector2[4];

        var rotation = UnityEngine.Random.Range(0, 4);
        var indices = new int[4]
        {
            rotationIndices[rotation],
            rotationIndices[(rotation + 3) % 4],    // top-right is 3-offset from top-left
            rotationIndices[(rotation + 1) % 4],    // top-right is 1-offset from top-left 
            rotationIndices[(rotation + 2) % 4]     // top-right is 2-offset from top-left
        };

        switch (numRotations)
        {
            case 0:
                uvs[indices[0]] = new Vector2(0.5f, 0.25f);
                uvs[indices[1]] = new Vector2(0.625f, 0.25f);
                uvs[indices[2]] = new Vector2(0.5f, 0);
                uvs[indices[3]] = new Vector2(0.625f, 0);
                break;
            case 1:
                uvs[indices[0]] = new Vector2(0.5f, 0.5f);
                uvs[indices[1]] = new Vector2(0.625f, 0.5f);
                uvs[indices[2]] = new Vector2(0.5f, 0.25f);
                uvs[indices[3]] = new Vector2(0.625f, 0.25f);
                break;
            case 2:
                uvs[indices[0]] = new Vector2(0.625f, 0.25f);
                uvs[indices[1]] = new Vector2(0.75f, 0.25f);
                uvs[indices[2]] = new Vector2(0.625f, 0);
                uvs[indices[3]] = new Vector2(0.75f, 0);
                break;
            case 3:
                uvs[indices[0]] = new Vector2(0.625f, 0.5f);
                uvs[indices[1]] = new Vector2(0.75f, 0.5f);
                uvs[indices[2]] = new Vector2(0.625f, 0.25f);
                uvs[indices[3]] = new Vector2(0.75f, 0.25f);
                break;
            default:
                throw new ArgumentException($"Bad concrete UV specifier: {numRotations}");
        }

        return uvs;
    }

    private Vector2[] GetPortalUVs()
    {
        return new Vector2[]
        {
            new Vector2(.25f, .75f),
            new Vector2(.375f, .75f),
            new Vector2(.25f, .5f),
            new Vector2(.375f, .5f),
        };
    }

    private Vector2[] GetLavaUVs()
    {
        return new Vector2[]
        {
            new Vector2(.375f, .75f),
            new Vector2(.5f, .75f),
            new Vector2(.375f, .5f),
            new Vector2(.5f, .5f),
        };
    }

    private Vector2[] GetNoneUVs()
    {
        return new Vector2[]
        {
            new Vector2(1, 1),
            new Vector2(1, 1),
            new Vector2(1, 1),
            new Vector2(1, 1)
        };
    }

    private void AddWall(Vector3 position, GameObject wallPrefab)
    {
        // Instantiate a wall instance, randomly rotated about the y axis
        var wall = Instantiate(wallPrefab, position, Quaternion.Euler(0, UnityEngine.Random.Range(0, 4) * 90f, 0), wallAndDoorParent.transform);
        // var wall = Instantiate(wallPrefab, position, Quaternion.Euler(0, 0f, 0), wallAndDoorParent.transform);

        walls.Add(wall);
    } 

    private void AddDoor(Vector3 position, GameObject doorPrefab, Direction direction)
    {
        // Instantiate and rotate a door instance
        var door = Instantiate(doorPrefab, position, Quaternion.Euler(0, (int)direction * 90f, 0), wallAndDoorParent.transform); 

        doors.Add(door);
    }
     
    private void AddQuad(Vector3 topLeft, Vector3 topRight, Vector3 botLeft, Vector3 botRight, Vector2[] uvs)
    {
        int vertexIndex = mVertices.Count;


        mVertices.Add(topLeft);
        mVertices.Add(topRight);
        mVertices.Add(botLeft);
        mVertices.Add(botRight);

        mTriangles.Add(vertexIndex);
        mTriangles.Add(vertexIndex + 1);
        mTriangles.Add(vertexIndex + 2);

        mTriangles.Add(vertexIndex + 1);
        mTriangles.Add(vertexIndex + 3);
        mTriangles.Add(vertexIndex + 2);

        foreach (var uv in uvs)
            mUVs.Add(uv);
    }
     
    private void ClearMeshData()
    {
        mMesh.Clear();
        mVertices.Clear();
        mTriangles.Clear(); 
        mUVs.Clear();

        foreach (var wall in walls)
        {
            Destroy(wall);
        }
        foreach (var door in doors)
        {
            Destroy(door);
        } 

        walls.Clear();
        doors.Clear();
    }

    private void SetMeshData()
    {
        mMesh.vertices = mVertices.ToArray();
        mMesh.triangles = mTriangles.ToArray(); 
        mMesh.uv = mUVs.ToArray();

        mMesh.RecalculateNormals();

        mMeshCollider.sharedMesh = mMesh;
    }
}
