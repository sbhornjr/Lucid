using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    /// <summary>
    /// The speed of the car.
    /// </summary>
    [SerializeField]
    private float movementSpeed = 2f;

    /// <summary>
    /// The speed at which the car turns.
    /// </summary>
    [SerializeField]
    private float turnSpeed = 20f;

    /// <summary>
    /// The sound to play when this car hits the player.
    /// </summary>
    [SerializeField]
    private AudioClip honkSFX;

    /// <summary>
    /// The current index of the car.
    /// </summary>
    public uint Index { get; private set; }

    /// <summary>
    /// The direction that the car is facing.
    /// </summary>                                      (the + 2 is because up is south)
    public Direction Facing { get { return (Direction)(((rotationLeftwardsOffset + 2) % 4 + 4) % 4); } }

    /// <summary>
    /// The next index; used while lerping.
    /// </summary>
    private uint nextIndex;

    /// <summary>
    /// The destination of the next tile; used while lerping.
    /// </summary>
    private Vector3 destination;

    /// <summary>
    /// The current turn orientation of the car. Incremented each time the car turns
    /// left and decremented each time the car turns right. Used to offset user input to move
    /// in the right direction.
    /// </summary>
    private int rotationLeftwardsOffset;

    /// <summary>
    /// The start and end rotations for a turn lerp.
    /// </summary>
    private Quaternion rotationDestination, rotationStart;

    /// <summary>
    /// The current interpolant of the turning car.
    /// </summary>
    private float rotationInterpolant;

    private Vector3 movementStart;
    private float movementInterpolant;

    public Direction CurrentDirection { get; private set; }

    private LevelManager levelManager;

    private void Awake()
    {
        levelManager = FindObjectOfType<LevelManager>();
    }

    /// <summary>
    /// Sets the index and position of the player.
    /// </summary>
    public void InitPosition(uint index)
    {
        Index = index;
        transform.position = GameUtils.GetPosition(index);

        var dirs = Direction.N.GetValues();
        CurrentDirection = dirs[UnityEngine.Random.Range(0, dirs.Length)];
        transform.rotation = Quaternion.Euler(0, (int)CurrentDirection * 90, 0);
    }

    /// <summary>
    /// Accepts the next index and calculates the next position.
    /// </summary>
    internal void AcceptNextIndex(uint index)
    {
        nextIndex = index;
        destination = GameUtils.GetPosition(nextIndex);

        movementInterpolant = 0;
        movementStart = transform.position;
    }

    public uint? ComputeNextPosition(ISet<uint> indices)
    {
        // Default to no motion and override if appropriate
        movementInterpolant = 1;
        movementStart = transform.position;

        // Move in one direction until a wall is hit
        if (levelManager.EntityCanMoveTo(Index, CurrentDirection, out var maybeNextIndex)
            && !indices.Contains(maybeNextIndex))
        {
            nextIndex = maybeNextIndex;
            movementStart = transform.position;
            movementInterpolant = 0;
            destination = GameUtils.GetPosition(nextIndex);
            return nextIndex;
        }
        else
        { 
            // Find a random valid direction in which to move
            var randomDirs = CurrentDirection.GetValues().OrderBy(d => UnityEngine.Random.value);
            foreach (var dir in randomDirs)
            {
                if (levelManager.EntityCanMoveTo(Index, dir, out maybeNextIndex)
                    && !indices.Contains(maybeNextIndex))
                {
                    CurrentDirection = dir;
                    nextIndex = maybeNextIndex;
                    movementStart = transform.position;
                    movementInterpolant = 0;
                    destination = GameUtils.GetPosition(nextIndex);
                    return nextIndex;
                }
            }
        } 

        // Impossible
        return null;
    }

    /// <summary>
    /// Moves the player one tick.
    /// </summary>
    internal bool Move()
    {
        transform.LookAt(destination);
        // Lerp one tick
        movementInterpolant += movementSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(movementStart, destination, movementInterpolant);

        bool isDone = movementInterpolant >= 1;

        if (isDone)
        {
            // Update the index when the movement is done.
            Index = nextIndex;
        }

        return isDone;
    }

    /// <summary>
    /// Accept a rotation instruction and calculate the rotation quaternion.
    /// </summary>
    internal void AcceptRotationDirection(RotateDirection direction)
    {
        switch (direction)
        {
            case RotateDirection.Left:
                rotationDestination = transform.rotation * Quaternion.Euler(0, -90, 0);
                ++rotationLeftwardsOffset;
                break;
            case RotateDirection.Right:
                rotationDestination = transform.rotation * Quaternion.Euler(0, 90, 0);
                --rotationLeftwardsOffset;
                break;
            default:
                throw new ArgumentException($"Unimplemented direction: {direction}");
        }

        rotationInterpolant = 0;
        rotationStart = transform.rotation;
    }

    /// <summary>
    /// Rotate the player one tick.
    /// </summary>
    internal bool Rotate()
    {
        rotationInterpolant += Time.deltaTime * turnSpeed;
        transform.rotation = Quaternion.Lerp(rotationStart, rotationDestination, rotationInterpolant);

        return rotationInterpolant >= 1;
    }

    internal void Honk()
    {
        AudioSource.PlayClipAtPoint(honkSFX, transform.position);
    }
}
