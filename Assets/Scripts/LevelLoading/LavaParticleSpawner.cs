using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawner
{
    public static List<ObjectInstantiationData> SpawnLavaParticles(RoomTemplate.Lava[] lavaTempalates, GameObject particles)
    {
        var lavas = new List<ObjectInstantiationData>();

        if (lavaTempalates != null)
        {
            foreach (var lava in lavaTempalates)
            {
                lavas.Add(new ObjectInstantiationData
                {
                    Index = lava.index,
                    GameObject = particles,
                    Type = ObjectInstantiationType.LavaParticleSystem,
                    Rotation = Quaternion.Euler(-90, 0, 0)
                });
            }
        }

        return lavas;
    }

    public static List<ObjectInstantiationData> SpawnPortalParticles(RoomTemplate.Portal portal, GameObject particles)
    {
        var portals = new List<ObjectInstantiationData>(1);
        
        if (portal != null)
        {
            portals.Add(new ObjectInstantiationData
            {
                Index = portal.index,
                GameObject = particles,
                Type = ObjectInstantiationType.PortalParticleSystem,
                Rotation = Quaternion.Euler(-90, 0, 0)
            });
        }

        return portals;
    }
}
