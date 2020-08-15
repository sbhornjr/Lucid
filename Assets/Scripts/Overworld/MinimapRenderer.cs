using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Xml;
using UnityEngine;

public class MinimapRenderer : MonoBehaviour
{
    [SerializeField]
    private float MaxLevelWidth = 13, MaxLevelHeight = 13;

    [SerializeField]
    private GameObject orientationObject;
    private MeshRenderer orientationObjectRenderer;

    [SerializeField]
    AudioClip mapOpenSFX;

    [SerializeField]
    private float minimapSize = 5;

    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;

    private Mesh mesh;

    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Color> colors;

    private float widthPerRoom, heightPerRoom; 

    private Vector2 currentPlayerRoom;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();

        orientationObjectRenderer = orientationObject.GetComponent<MeshRenderer>();

        vertices = new List<Vector3>();
        triangles = new List<int>();
        colors = new List<Color>(); 
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) 
        {
            meshRenderer.enabled = !meshRenderer.enabled;
            orientationObjectRenderer.enabled = !orientationObjectRenderer.enabled;
            AudioSource.PlayClipAtPoint(mapOpenSFX, Camera.main.transform.position);
        }
    }

    internal void RenderLevel(Level level, Vector2 currentRoom)
    {
        var numCols = (level.Width + 0) * 2; var numRows = (level.Height + 0) * 2;
        var widthPerRoomUnsquared = minimapSize / numCols;
        var heightPerRoomUnsquared = minimapSize / numRows;

        widthPerRoom = Math.Min(widthPerRoomUnsquared, heightPerRoomUnsquared);
        heightPerRoom = widthPerRoom;

        orientationObject.transform.localScale = Vector3.one * widthPerRoom * .08f;

        ClearMeshData(); 

        AddBounds();

        foreach (var room in level.RoomLocations)
        {
            var color = room.Equals(currentRoom) ? Color.green : Color.blue;
            AddRoom(room, color);
        }
        
        foreach (var hallway in level.Hallways)
        {
            AddHallway(hallway);
        }
         
        SetMeshData();

        // Cache the current room for later overwrite
        currentPlayerRoom = currentRoom;

        MoveOrientationObject(currentRoom);
    }

    private void ClearMeshData()
    {
        mesh.Clear();

        vertices.Clear();
        triangles.Clear();
        colors.Clear();
    }

    private void SetMeshData()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();

        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh; 
    }

    private void AddBounds()
    {
        var index = vertices.Count;
        vertices.Add(new Vector3(-minimapSize, 0, -minimapSize));
        vertices.Add(new Vector3(minimapSize, 0, -minimapSize));
        vertices.Add(new Vector3(minimapSize, 0, minimapSize));
        vertices.Add(new Vector3(-minimapSize, 0, minimapSize));

        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 1);
        triangles.Add(index);
        triangles.Add(index + 3);
        triangles.Add(index + 2);

        colors.Add(Color.gray);
        colors.Add(Color.gray);
        colors.Add(Color.gray);
        colors.Add(Color.gray);
    }
     
    private void AddRoom(Vector2 location, Color color)
    { 
        var x = (location.x * widthPerRoom * 2) - widthPerRoom * .5f;
        var z = (-location.y * heightPerRoom * 2) - heightPerRoom * .5f;

        var index = vertices.Count;
        vertices.Add(new Vector3(x, 0, z));
        vertices.Add(new Vector3(x + widthPerRoom, 0, z));
        vertices.Add(new Vector3(x + widthPerRoom, 0, z + heightPerRoom));
        vertices.Add(new Vector3(x, 0, z + heightPerRoom));

        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 1);
        triangles.Add(index);
        triangles.Add(index + 3);
        triangles.Add(index + 2);

        colors.Add(color); 
        colors.Add(color); 
        colors.Add(color); 
        colors.Add(color); 
    }

    private void AddHallway(Level.Hallway hallway)
    {
        var locA = hallway.LocationOfRoomA * widthPerRoom * 2;
        var locB = hallway.LocationOfRoomB * widthPerRoom * 2;
         
        Vector3 topLeft, topRight, botRight, botLeft;

        if (locA.x == locB.x) // vertically aligned
        {
            if (locA.y < locB.y)    // Hallway points down
            { 
                topLeft = new Vector3(locA.x - widthPerRoom / 4, 0, -(locA.y + widthPerRoom / 2));
                topRight = new Vector3(locA.x + widthPerRoom / 4, 0, -(locA.y + widthPerRoom / 2));
                botRight = new Vector3(locA.x + widthPerRoom / 4, 0, -(locB.y - widthPerRoom / 2));
                botLeft = new Vector3(locA.x - widthPerRoom / 4, 0, -(locB.y - widthPerRoom / 2));
            }
            else        // Hallway points up
            {
                topLeft = new Vector3(locB.x - widthPerRoom / 4, 0, -(locB.y + widthPerRoom / 2));
                topRight = new Vector3(locB.x + widthPerRoom / 4, 0, -(locB.y + widthPerRoom / 2));
                botRight = new Vector3(locB.x + widthPerRoom / 4, 0, -(locA.y - widthPerRoom / 2));
                botLeft = new Vector3(locB.x - widthPerRoom / 4, 0, -(locA.y - widthPerRoom / 2)); 
            }
        }
        else    // locA.y == locB.y
        {
            if (locA.x < locB.x)    // Hallway points right
            {
                topLeft = new Vector3(locA.x + widthPerRoom / 2, 0, -(locA.y - widthPerRoom / 4));
                topRight = new Vector3(locB.x - widthPerRoom / 2, 0, -(locA.y - widthPerRoom / 4));
                botRight = new Vector3(locB.x - widthPerRoom / 2, 0, -(locA.y + widthPerRoom / 4));
                botLeft = new Vector3(locA.x + widthPerRoom / 2, 0, -(locA.y + widthPerRoom / 4)); 
            }
            else    // Hallway points left
            {
                topLeft = new Vector3(locB.x + widthPerRoom / 2, 0, -(locB.y - widthPerRoom / 4));
                topRight = new Vector3(locA.x - widthPerRoom / 2, 0, -(locB.y - widthPerRoom / 4));
                botRight = new Vector3(locA.x - widthPerRoom / 2, 0, -(locB.y + widthPerRoom / 4));
                botLeft = new Vector3(locB.x + widthPerRoom / 2, 0, -(locB.y + widthPerRoom / 4));
            }
        }

        var index = vertices.Count;
        vertices.Add(topLeft);
        vertices.Add(topRight);
        vertices.Add(botRight);
        vertices.Add(botLeft);

        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 1);
        triangles.Add(index);
        triangles.Add(index + 3);
        triangles.Add(index + 2);

        colors.Add(Color.black);
        colors.Add(Color.black);
        colors.Add(Color.black);
        colors.Add(Color.black);
    }

    internal void UpdateCurrentRoom(Vector2 newLocation)
    {
        // Just overwrite the one room
        AddRoom(currentPlayerRoom, Color.blue);
        AddRoom(newLocation, Color.green);

        currentPlayerRoom = newLocation;

        MoveOrientationObject(newLocation);

        SetMeshData();
    }

    private void MoveOrientationObject(Vector2 coordinates)
    { 
        // Local space: put the arrow in the right place (within the room)
        var localPointFlatOnMinimap = new Vector3(
            coordinates.x * widthPerRoom * 2, 0.05f, -coordinates.y * widthPerRoom * 2);

        // Move it closer to the camera (this is required, but I don't know why)
        var cameraPosition = transform.InverseTransformPoint(Camera.main.transform.position);
        var distance = Vector3.Distance(localPointFlatOnMinimap, cameraPosition);
        var point = Vector3.MoveTowards(
            localPointFlatOnMinimap, cameraPosition, distance / 8);
         
        orientationObject.transform.position = transform.TransformPoint(point); 
    }
}
