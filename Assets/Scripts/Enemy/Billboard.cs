using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * this class is attached to the enemy's canvas so that it is always facing the active camera
 */ 
public class Billboard : MonoBehaviour
{
    public Transform cam;

    private void Awake()
    {
        cam = Camera.main.transform;
    }

    /**
     * update the UI so it is facing whichever camera is active right now
     */
    private void LateUpdate()
    {
        transform.LookAt(transform.position + cam.position);
    }
}
