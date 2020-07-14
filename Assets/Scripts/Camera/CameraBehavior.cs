using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField]
    private bool followPlayer;

    [SerializeField]
    private Vector3 worldViewPosition, worldViewRotation, followPlayerOffset;

    private Transform mPlayerTransform; 

    // Start is called before the first frame update
    void Start()
    {
        mPlayerTransform = GameObject.FindGameObjectWithTag("Player").transform; 
    }

    // Update is called once per frame
    void Update()
    {
        if (followPlayer)
        {
            transform.position = mPlayerTransform.position + followPlayerOffset;
        }
        else
        {
            // Switch to world view
            transform.position = worldViewPosition;
            transform.rotation = Quaternion.Euler(worldViewRotation);
        }
    }
}
