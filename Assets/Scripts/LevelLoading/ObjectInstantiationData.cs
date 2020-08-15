using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct ObjectInstantiationData
{
    public GameObject GameObject { get; set; }
    public uint Index { get; set; }
    public uint ObjectPrefabInstantiationIndex { get; set; }
    public ObjectInstantiationType Type { get; set; }
    public Quaternion Rotation { get; set; } 
    public bool InteractionComplete { get; set; }
    public POIType POIType { get; set; }
}

public enum ObjectInstantiationType
{
    Enemy, Treasure, POI,
    Car,
    LavaParticleSystem,
    Prop,
    PortalParticleSystem
}
