using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    /// <summary>
    /// zOffset is the distance away from the player on the z-axis<br/>
    /// xTiltAngle is the downwards tilt of the camera relative to the y-z plane<br/>
    /// yOffset is the distance away from the player on the y-axis
    /// </summary>
    [SerializeField]
    private float zOffset = 2, xTiltAngle = 20, yOffset = 2;

    private Transform player; 

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; 
    }

    private void LateUpdate()
    {
        // Apply the offsets and the tilt; the order of these operations matters
        transform.position = player.transform.position - player.transform.forward * zOffset;
        transform.LookAt(player.transform.position);
        transform.position = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);
        transform.Rotate(Vector3.right, xTiltAngle); 
    }
}
