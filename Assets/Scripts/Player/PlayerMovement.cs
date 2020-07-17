using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private uint startingIndex;

    private TileMap mTileMap;

    private uint mIndex;

    private EnemyMovement[] mEnemyMovements;

    [SerializeField]
    private StartEncounter startEncounter;

    private void Awake()
    {
        mTileMap = FindObjectOfType<TileMap>();
        mEnemyMovements = FindObjectsOfType<EnemyMovement>();
    }
     
    public void InitPosition()
    {
        mIndex = startingIndex;

        transform.position = mTileMap.GetCenterPositionOfTileAt(mIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.W))
        {
            HandleKey(Direction.S);
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            HandleKey(Direction.W);
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            HandleKey(Direction.N);
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            HandleKey(Direction.E);
        } 
    }

    private void HandleKey(Direction direction)
    {
        if (mTileMap.TryMoveToNeighbor(mIndex, direction, out var nextIndex))
        {
            mIndex = nextIndex; 
            transform.position = mTileMap.GetCenterPositionOfTileAt(mIndex);
            foreach(EnemyMovement em in mEnemyMovements)
            {
                if (em != null)
                {
                    var enIndex = em.getTile();
                    if (enIndex == mIndex) startEncounter.PlayerStartEncounter(em);
                    else if (em.MoveEnemy() == mIndex) startEncounter.EnemyStartEncounter(em);
                }
            }
        }
    }

    public uint getTile()
    {
        return mIndex;
    }
}
