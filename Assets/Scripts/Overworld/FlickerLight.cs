using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerLight : MonoBehaviour
{ 
    public enum FlickerType { Sinusoidal, Perlin }

    [SerializeField]
    private FlickerType flickerType = FlickerType.Sinusoidal;

    [SerializeField]
    private float flickerSpeed = 2, flickerAmplitude = 10, flickerOffset = 0.5f;

    private Light lightToFlicker = default;

    private bool _Active;
    public bool Active { get { return _Active; } set { _Active = value; if (!value) lightToFlicker.intensity = 0; } }

    // Start is called before the first frame update
    void Awake()
    {
        lightToFlicker = GetComponent<Light>();
        Active = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Active)
        {
            switch (flickerType)
            {
                case FlickerType.Sinusoidal:
                    lightToFlicker.intensity = flickerAmplitude * Mathf.Sin(Time.time * flickerSpeed) + flickerOffset;
                    break;
                case FlickerType.Perlin:
                    lightToFlicker.intensity = flickerAmplitude * Mathf.PerlinNoise(Time.time * flickerSpeed, 0) + flickerOffset;
                    break;
                default:
                    break;
            }
        } 
    }
}
