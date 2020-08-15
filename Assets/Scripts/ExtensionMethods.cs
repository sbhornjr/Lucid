using UnityEngine;

public static class ExtensionMethod
{
    public static GameObject Instantiate(this Object thisObj, Object prefab, int level)
    {
        GameObject weapon = Object.Instantiate(prefab) as GameObject;
        weapon.GetComponent<Weapon>().InitStats(level);
        return weapon;
    }

    public static GameObject Instantiate(this Object thisObj, Object prefab, int level, bool fromVendor)
    {
        GameObject weapon = Object.Instantiate(prefab) as GameObject;
        weapon.GetComponent<Weapon>().InitStats(level);
        weapon.GetComponent<Weapon>().Stats.fromVendor = true;
        return weapon;
    }
}
