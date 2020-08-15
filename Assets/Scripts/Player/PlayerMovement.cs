using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    /// <summary>
    /// The speed of the player.
    /// </summary>
    [SerializeField]
    private float movementSpeed = 2f;

    /// <summary>
    /// The speed at which the player turns.
    /// </summary>
    [SerializeField]
    private float turnSpeed = 20f;

    /// <summary>
    /// The object on the minimap responsible for showing player orientation.
    /// </summary>
    [SerializeField]
    private Transform minimapOrientationObject;

    /// <summary>
    /// The current index of the player.
    /// </summary>
    public uint Index { get; private set; }
     
    /// <summary>
    /// The direction that the player is facing.
    /// </summary>                                      (the + 2 is because up is south)
    public Direction Facing { get { return (Direction)(((rotationLeftwardsOffset + 2) % 4 + 4) % 4); } }

    /// <summary>
    /// Half of the height of the player. Used to center the player vertically.
    /// </summary>
    private float halfHeight;

    /// <summary>
    /// The next index; used while lerping.
    /// </summary>
    private uint nextIndex;

    /// <summary>
    /// The destination of the next tile; used while lerping.
    /// </summary>
    private Vector3 destination;

    /// <summary>
    /// The current turn orientation of the player. Incremented each time the player turns
    /// left and decremented each time the player turns right. Used to offset user input to move
    /// in the right direction.
    /// </summary>
    private int rotationLeftwardsOffset;

    /// <summary>
    /// The start and end rotations for a turn lerp.
    /// </summary>
    private Quaternion rotationDestination, rotationStart;

    /// <summary>
    /// The current interpolant of the turning player.
    /// </summary>
    private float rotationInterpolant;

    private Vector3 movementStart;
    private float movementInteroplant;
    public Animator anim;

    private void Awake()
    {  
        halfHeight = GetComponent<MeshRenderer>().bounds.extents.y;
        rotationLeftwardsOffset = 0;
        anim = GetComponent<Animator>();
    }  

    /// <summary>
    /// Sets the index and position of the player.
    /// </summary>
    public void InitPosition(uint index)
    {
        Index = index; 
        transform.position = GameUtils.GetPosition(this.Index) + (Vector3.up * halfHeight);
    } 
     
    /// <summary>
    /// Accepts the next index and calculates the next position.
    /// </summary>
    internal void AcceptNextIndex(uint index)
    {
        nextIndex = index; 
        destination = GameUtils.GetPosition(nextIndex) + Vector3.up * halfHeight;

        movementInteroplant = 0;
        movementStart = transform.position;
    }
     
    /// <summary>
    /// Moves the player one tick.
    /// </summary>
    internal bool Move()
    {
        anim.SetBool("animMoving", true);

        // Lerp one tick
        movementInteroplant += movementSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(movementStart, destination, movementInteroplant);

        bool isDone = movementInteroplant >= 1;

        if (isDone)
        {
            anim.SetBool("animMoving", false);

            // Update the index when the movement is done.
            Index = nextIndex;
        }

        return isDone;
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
     
    /// <summary>
    /// Accept a rotation instruction and calculate the rotation quaternion.
    /// </summary>
    internal void AcceptRotationDirection(RotateDirection direction)
    {
        switch (direction)
        {
            case RotateDirection.Left: 
                rotationDestination = transform.rotation * Quaternion.Euler(0, -90, 0);
                minimapOrientationObject.transform.Rotate(Vector3.up * -90f, Space.Self);
                ++rotationLeftwardsOffset; 
                break;
            case RotateDirection.Right: 
                rotationDestination = transform.rotation * Quaternion.Euler(0, 90, 0);
                minimapOrientationObject.transform.Rotate(Vector3.up * 90f, Space.Self);
                --rotationLeftwardsOffset;
                break;
            default:
                throw new ArgumentException($"Unimplemented direction: {direction}");
        } 

        rotationInterpolant = 0;
        rotationStart = transform.rotation;
    } 
     
    /// <summary>
    /// Get the input direction offset by the current player rotation.
    /// </summary>
    internal Direction GetInputDirection(Direction direction)
    {
        int directionOffset = rotationLeftwardsOffset + (int)direction;

        // C#'s % operator isn't a modulo, it's a remainder. This crap below works for + and - numbers.
        var index = (directionOffset % 4 + 4) % 4;
        return direction.GetValues()[index];
    } 
}
