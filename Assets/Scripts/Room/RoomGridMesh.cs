using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

public class RoomGridMesh : MonoBehaviour
{
    private Mesh mMesh;
    private MeshCollider mMeshCollider;

    private List<Vector3> mVertices;
    private List<int> mTriangles;
    private List<Color> mColors;

    private void Awake()
    {
        // Assign members
        GetComponent<MeshFilter>().mesh = mMesh = new Mesh();
        mMeshCollider = gameObject.AddComponent<MeshCollider>();

        mVertices = new List<Vector3>();
        mTriangles = new List<int>();
        mColors = new List<Color>();
    }

    public void Triangulate(Tile[] tiles)
    {
        ClearMeshData();

        foreach (var tile in tiles)
        {
            Triangulate(tile);
        }

        SetMeshData();
    }

    private void Triangulate(Tile tile)
    {
        // Add a quad of the tile color at the tile location
        AddTile(tile);
    }


    private void AddTile(Tile tile)
    {
        var color = tile.IsWall ? Color.black : Color.gray;

        // Flip right and left to render through the bottom of the map; this is required to interact with the map from above (clicking, etc)
        AddQuad(tile.TopRight, tile.TopLeft, tile.BotRight, tile.BotLeft, color);
    }

    private void AddQuad(Vector3 topLeft, Vector3 topRight, Vector3 botLeft, Vector3 botRight, Color color)
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

        mColors.Add(color);
        mColors.Add(color);
        mColors.Add(color);
        mColors.Add(color);
    }

    private void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
    {
        int vertexIndex = mVertices.Count;

        mVertices.Add(a);
        mVertices.Add(b);
        mVertices.Add(c);

        mTriangles.Add(vertexIndex);
        mTriangles.Add(vertexIndex + 1);
        mTriangles.Add(vertexIndex + 2);

        mColors.Add(color);
        mColors.Add(color);
        mColors.Add(color);
    }

    private void ClearMeshData()
    {
        mMesh.Clear();
        mVertices.Clear();
        mTriangles.Clear();
        mColors.Clear();
    }

    private void SetMeshData()
    {
        mMesh.vertices = mVertices.ToArray();
        mMesh.triangles = mTriangles.ToArray();
        mMesh.colors = mColors.ToArray();

        mMesh.RecalculateNormals();

        mMeshCollider.sharedMesh = mMesh;
    }
}
