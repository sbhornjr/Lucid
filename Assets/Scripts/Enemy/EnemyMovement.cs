using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField]
    private uint startingIndex = 67;

    [SerializeField]
    private uint lineOfSight = 4;

    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private Canvas exclCanvas;

    private string moveType = "random";
    private TileMap mTileMap;
    private uint mIndex;
    private System.Random rand;
    private Direction facing;

    private void Awake()
    {
        mTileMap = FindObjectOfType<TileMap>();
        rand = new System.Random();
    }

    public void InitPosition()
    {
        mIndex = startingIndex;

        transform.position = mTileMap.GetCenterPositionOfTileAt(mIndex);
    }

    public uint MoveEnemy()
    {
        if (CheckLoS())
        {
            moveType = "follow";
            exclCanvas.gameObject.SetActive(true);
        }

        if (moveType == "random") MoveRandom();
        if (moveType == "follow") FollowPlayer();

        return mIndex;
    }

    private void FollowPlayer()
    {
        var playerTile = mTileMap.GetCenterPositionOfTileAt(playerMovement.getTile());
        var moveToIndex = mIndex;
        var direction = facing;
        var minDistance = Mathf.Infinity;

        Direction[] dirs = new Direction[] { Direction.N, Direction.E, Direction.S, Direction.W };

        foreach(Direction dir in dirs)
        {
            if (mTileMap.TryMoveToNeighbor(mIndex, dir, out var nextIndex))
            {
                var distance = Vector3.Distance(playerTile, mTileMap.GetCenterPositionOfTileAt(nextIndex));
                if (distance < minDistance)
                {
                    direction = dir;
                    minDistance = distance;
                    moveToIndex = nextIndex;
                }
            }
        }

        if (minDistance != Mathf.Infinity)
        {
            facing = direction;
            mIndex = moveToIndex;
            transform.LookAt(mTileMap.GetCenterPositionOfTileAt(mIndex));
            transform.position = mTileMap.GetCenterPositionOfTileAt(mIndex);
        }
    }

    private void MoveRandom()
    {
        int dir = rand.Next(4);

        Direction direction;
        bool stay = false;

        switch (dir)
        {
            case 0:
                direction = Direction.N;
                break;
            case 1:
                direction = Direction.E;
                break;
            case 2:
                direction = Direction.S;
                break;
            case 3:
                direction = Direction.W;
                break;
            default:
                stay = true;
                direction = Direction.N;
                break;
        }

        if (!stay)
        {
            if (mTileMap.TryMoveToNeighbor(mIndex, direction, out var nextIndex))
            {
                facing = direction;
                mIndex = nextIndex;
                transform.LookAt(mTileMap.GetCenterPositionOfTileAt(mIndex));
                transform.position = mTileMap.GetCenterPositionOfTileAt(mIndex);

                if (CheckLoS())
                {
                    moveType = "follow";
                }
            }
        }
    }

    private bool CheckLoS()
    {
        var checkIndex = mIndex;
        for (int i = 0; i < lineOfSight; ++i)
        {
            if (mTileMap.TryMoveToNeighbor(checkIndex, facing, out var nextIndex))
            {
                if (playerMovement.getTile() == nextIndex)
                {
                    return true;
                }
                else
                {
                    checkIndex = nextIndex;
                }
            }
        }
        return false;
    }

    public uint getTile()
    {
        return mIndex;
    }

    public void ToggleExcl(bool which)
    {
        exclCanvas.gameObject.SetActive(which);
    }
}
