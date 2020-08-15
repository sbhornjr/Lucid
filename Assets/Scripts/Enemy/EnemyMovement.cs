using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField]
    private uint lineOfSight;

    [SerializeField]
    private Image excl;

    [SerializeField]
    private float movementSpeed = 2f;

    [SerializeField]
    private uint turnsInterestedInPlayer = 5;

    [SerializeField]
    private Transform eyes;

    [SerializeField]
    private uint fieldOfView;

    private uint turnsFollowedPlayer = 0;
    private bool followingPlayer = false;

    public uint Index { get; private set; }

    public bool IsEnemyAlive { get; set; }

    private float halfHeight;

    private LevelManager levelManager;

    private uint nextIndex;
    private Vector3 destination;

    private Vector3 movementStart;
    private float movementInterpolant;

    private GameObject player;

    private Animator anim;
    private void Awake()
    {
        levelManager = FindObjectOfType<LevelManager>();

        halfHeight = GetComponent<MeshRenderer>().bounds.extents.y;

        player = GameObject.FindGameObjectWithTag("Player");

        anim = GetComponent<Animator>();

        IsEnemyAlive = true;
    } 

    public void InitPosition(uint index)
    { 
        Index = index;

        transform.position = GameUtils.GetPosition(this.Index) + (Vector3.up * halfHeight);
    }

    public uint? ComputeNextPosition(ISet<uint> indices)
    {
        // Default to no motion and override if appropriate
        movementInterpolant = 1;
        movementStart = transform.position;

        if (!followingPlayer)
        {
            FollowPlayerIfVisible();
        }

        // Sometimes, stay where we are
        if (UnityEngine.Random.value < 0.2f && !indices.Contains(Index) && !followingPlayer)
        {
            nextIndex = Index; 
            destination = transform.position;
            return nextIndex;
        }
        else
        {
            // If following player, order directions towards players location. Otherwise, move randomly
            var dirs = (Direction[])Enum.GetValues(typeof(Direction));
            List<Direction> orderedDirs = GetDirectionOrder(dirs, followingPlayer);

            foreach (var dir in orderedDirs)
            {
                if (levelManager.EntityCanMoveTo(Index, dir, out var maybeNextIndex) && !indices.Contains(maybeNextIndex))
                {
                    nextIndex = maybeNextIndex;
                    movementStart = transform.position;
                    movementInterpolant = 0;
                    destination = GameUtils.GetPosition(nextIndex);
                    return nextIndex;
                }
            }
            if (!indices.Contains(Index))
            {
                nextIndex = Index;
                destination = transform.position;
                return nextIndex;
            }
            return null;
        }
    }

    private void FollowPlayerIfVisible()
    {
        if (PlayerVisible())
        {
            turnsFollowedPlayer = 0;
            followingPlayer = true;
            ToggleExcl(true);
        }
        else
        {
            followingPlayer = false;
            ToggleExcl(false);
        }
    }

    private bool PlayerVisible()
    {
        RaycastHit hit;
        Vector3 directionToPlayer = player.transform.position - eyes.position;

        if (Vector3.Angle(directionToPlayer, eyes.forward) <= fieldOfView)
        {
            if (Physics.Raycast(eyes.position, directionToPlayer, out hit, lineOfSight))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        return false;
    }

    private List<Direction> GetDirectionOrder(Direction[] dirs, bool followingPlayer)
    {
        List<Direction> output = new List<Direction>();
        List<float> outputAdjacency = new List<float>();

        // Order directions based on which direction gets to player sooner
        if (followingPlayer)
        {
            foreach (Direction dir in dirs)
            {
                if (levelManager.EntityCanMoveTo(Index, dir, out var nextIndex))
                {
                    var distance = Vector3.Distance(GameUtils.GetPosition(nextIndex), player.transform.position);

                    if (outputAdjacency.Count == 0)
                    {
                        outputAdjacency.Add(distance);
                        output.Add(dir);
                    }
                    else
                    {
                        int placement = outputAdjacency.Count;

                        for (int i = 0; i < outputAdjacency.Count; i++)
                        {
                            if (distance < outputAdjacency[i])
                            {
                                placement = i;
                                break;
                            }
                        }
                        outputAdjacency.Insert(placement, distance);
                        output.Insert(placement, dir);
                    }
                }
            }
            return output;
        }
        // if isnt chasing player, move randomly
        else
        {
            return dirs.OrderBy(d => UnityEngine.Random.value).ToList<Direction>();
        }
    }

    public bool Move()
    {
        anim.SetBool("animMoving", true);
        transform.LookAt(destination);
        movementInterpolant += movementSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(movementStart, destination, movementInterpolant);

        bool isDone = Vector3.Distance(transform.position, destination) < 0.05f;

        if (isDone)
        {
            // Incrememnt turns followed player if following player, and stop following if becomes disinterested
            if (followingPlayer)
            {
                turnsFollowedPlayer += 1;
            }
            if (turnsFollowedPlayer >= turnsInterestedInPlayer)
            {
                followingPlayer = false;
                ToggleExcl(false);
                turnsFollowedPlayer = 0;

            }
            Index = nextIndex;

            anim.SetBool("animMoving", false);

            if (!followingPlayer)
                FollowPlayerIfVisible();
        }

        return isDone;
    }

    public uint GetTile()
    {
        return Index;
    }

    public void ToggleExcl(bool which)
    {
        excl.enabled = which;
    }
}
