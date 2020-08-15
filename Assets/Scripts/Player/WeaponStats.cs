using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/**
 * the stats of a weapon
 */
[System.Serializable]
public class WeaponStats
{
    public string attackName;

    public int dmg;

    public int cooldown;

    public Color color;

    [NonSerialized]
    public Sprite sprite;

    public WeaponType type;

    public string name;

    public int level;

    public Synergy synergy;

    //public bool isRedBuffed;

    //public bool hasGoldenBowString;

    public int price;

    public bool fromVendor;

    static string[] adjectives = new string[10] { "Awesome", "Terrible", "Peaceful", "Harrowing", "Tropical",
                                            "Lazy", "Gigantic", "Useful", "Reliable", "Gruesome"};

    static string[] nouns = new string[10] { "Reckoning", "Vengeance", "Laziness", "Yesterday", "Pity",
                                        "Jealousy", "The Damned", "Oscillation", "Christmas", "Zealots"};

    /**
     *
     */
    internal static WeaponStats RandomStats(int level)
    {
        int dps = level;
        int gold;
        switch (level)
        {
            case 1:
                gold = 50;
                break;
            case 2:
                gold = 100;
                break;
            case 3:
                gold = 200;
                break;
            case 4:
                gold = 300;
                break;
            case 5:
                gold = 400;
                break;
            default:
                throw new Exception();
        }

        WeaponType weaponType;
        string attack;
        int time;
        int damage;
        switch (UnityEngine.Random.Range(0, 6))
        {
            case 0:
                weaponType = WeaponType.Sword;
                attack = "Slash";
                time = UnityEngine.Random.Range(2, 5);
                damage = dps * time;
                break;
            case 1:
                weaponType = WeaponType.Whip;
                attack = "Crack";
                time = UnityEngine.Random.Range(2, 4);
                damage = dps * time;
                break;
            case 2:
                weaponType = WeaponType.Staff;
                attack = "Bonk";
                time = UnityEngine.Random.Range(3, 6);
                damage = dps * time;
                break;
            case 3:
                weaponType = WeaponType.Hammer;
                attack = "Bang";
                time = UnityEngine.Random.Range(4, 6);
                damage = dps * time;
                break;
            case 4:
                weaponType = WeaponType.Bow;
                attack = "Woosh";
                time = UnityEngine.Random.Range(3, 5);
                damage = dps * time;
                break;
            case 5:
                weaponType = WeaponType.Dagger;
                attack = "Stab";
                time = UnityEngine.Random.Range(1, 3);
                damage = dps * time;
                break;
            default:
                throw new System.Exception();
        }

        Color weaponColor;
        Synergy synergy;
        switch (UnityEngine.Random.Range(0, 3))
        {
            case 0:
                weaponColor = Color.red;
                synergy = (Synergy)UnityEngine.Random.Range(0, 2);
                break;
            case 1:
                weaponColor = Color.blue;
                synergy = (Synergy)UnityEngine.Random.Range(2, 4);
                break;
            case 2:
                weaponColor = Color.green;
                synergy = (Synergy)UnityEngine.Random.Range(4, 6);
                break;
            default:
                throw new System.Exception();
        }

        string weaponName
            = adjectives[UnityEngine.Random.Range(0, 10)] + " " + weaponType.ToString() + " of " + nouns[UnityEngine.Random.Range(0, 10)];

        return new WeaponStats()
        {
            attackName = attack,
            type = weaponType,
            dmg = damage,
            cooldown = time,
            color = weaponColor,
            sprite = Resources.Load<Sprite>("Sprites/" + weaponType.ToString()),
            name = weaponName,
            synergy = synergy,
            level = level,
            price = gold,
            fromVendor = false,
        };
    }

    internal static WeaponStats FromSerializable(SerializableWeapon weapon)
    {
        var stats = weapon.weaponStats;

        stats.sprite = Resources.Load<Sprite>("Sprites/" + stats.type.ToString()); 

        return stats;
    }

    /**
     * used to randomize the stats of a weapon
     */
    internal static WeaponStats RandomStats()
    {
        switch(GameStateMachine.CurrentLevel)
        {
            case GameStateMachine.Level.Dungeon:
                var num = UnityEngine.Random.Range(0, 100);
                if (num < 70) return RandomStats(1);
                else if (num < 95) return RandomStats(2);
                else return RandomStats(3);
            case GameStateMachine.Level.City:
                num = UnityEngine.Random.Range(0, 100);
                if (num < 70) return RandomStats(2);
                else if (num < 95) return RandomStats(3);
                else return RandomStats(4);
            case GameStateMachine.Level.Hell:
                num = UnityEngine.Random.Range(0, 100);
                if (num < 70) return RandomStats(3);
                else if (num < 95) return RandomStats(4);
                else return RandomStats(5);
            default:
                throw new Exception();
        }
    }

    /**
     * gets the weapon's color as a string
     */
    public string ColorToString()
    {
        string colorString;
        if (color == Color.red)
            colorString = "Red";
        else if (color == Color.blue)
            colorString = "Blue";
        else colorString = "Green";

        return colorString;
    }

    /**
     * gets the weapon's synergy as a string
     */
    public string SynergyToString()
    {
        string synergyString;
        switch(synergy)
        {
            case Synergy.MoreDmg:
                synergyString = "Extra Damage (Attack Based)";
                break;
            case Synergy.DmgBuffOverTime:
                synergyString = "Extra Damage (Time Based)";
                break;
            case Synergy.GainShield:
                synergyString = "Gain Shield";
                break;
            case Synergy.Deflect:
                synergyString = "Deflect Enemy Attacks";
                break;
            case Synergy.PlayerTimeShorten:
                synergyString = "Shorten Cooldowns";
                break;
            case Synergy.EnemyTimeLengthen:
                synergyString = "Lengthen Enemy Cooldowns";
                break;
            default:
                throw new System.Exception();
        }

        return synergyString;
    }

    // there are 6 different weapon types
    public enum WeaponType { Sword, Whip, Staff, Hammer, Bow, Dagger };

    /**
     * special synergy abilities for each weapon color
     * MoreDmg: 1 - x1.5 dmg for this attack
     *          2 - x2 dmg for this attack
     *          3 - x1.5 dmg for this attack and the attack(s) synergized with
     *          4 - x2 dmg for this attack and the attack(s) synergized with
     *          5 - x3 dmg for this attack and the attack(s) synergized with
     *
     * DmgBuffOverTime: 1 - x1.5 dmg on all attacks for 3 seconds
     *                  2 - x1.5 dmg on all attacks for 5 seconds
     *                  3 - x2 dmg on all attacks for 3 seconds
     *                  4 - x2 dmg on all attacks for 5 seconds
     *                  5 - x3 dmg on all attacks for 5 seconds
     *
     * GainShield: 1 - gain 3 shield
     *             2 - gain 5 shield
     *             3 - gain 8 shield
     *             4 - gain 10 shield
     *             5 - gain 15 shield
     *
     * Deflect: 1 - block 20% of the enemy's next attack
     *          2 - block 40% of the enemy's next attack
     *          3 - block 50% of the enemy's next attack
     *          4 - block 65% of the enemy's next attack
     *          5 - block 80% of the enemy's next attack
     *
     * PlayerTimeShorten: 1 - this weapon's timer is halved until its next attack
     *                    2 - this weapon's timer and that of the weapon(s) synergized with are halved until next attacks
     *                    3 - this weapon's timer is halved for its next 2 attacks
     *                    4 - this weapon's timer and that of the weapon(s) synergized with are halved for their next 2 attacks
     *                    5 - this weapon's timer is halved for the rest of the combat
     *
     * EnemyTimeLengthen: 1 - the enemy's timer is doubled until its next attack
     *                    2 - the enemy's timer is doubled for 2 attacks
     *                    3 - the enemy's timer is doubled for 3 attacks
     *                    4 - the enemy's timer is doubled for [# of weapons synergized] attacks
     *                    5 - the enemy's timer is doubled for the rest of the combat
     */
    public enum Synergy { MoreDmg, DmgBuffOverTime, GainShield, Deflect, PlayerTimeShorten, EnemyTimeLengthen, };
}
