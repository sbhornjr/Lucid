using System;
using System.Collections.Generic;
using UnityEngine;

/**
  * Represents a collectable object that a player can have that provides
  * passive benefit for them.
  */
public class Item
{
    // immediate / persistent effects
    // enums
    // loop thru weapons for buffs when new weapons are acquired
    // when combat ends, x percentage change of getting items/weapons
    // vendors might sell items/weapons, might find these in chests

    PlayerStats playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();

    public Sprite sprite;

    public string description;
    public string name;
    public bool fromVendor;
    public int price = 100;
    public static List<string> exists;

    public enum ItemEffect { redRune, goldenBowString, syringe, magnet };

    public ItemEffect itemEffect;

    public static Item RandomItem()
    {
        Item item = new Item();
        if (exists == null) exists = new List<string>();
        int n;
        while (true)
        {
            n = UnityEngine.Random.Range(0, 4);
            foreach (string s in exists)
                if (s == ((ItemEffect)n).ToString()) continue;
            break;
        }
        item.itemEffect = (ItemEffect)n;
        item.sprite = Resources.Load<Sprite>("Sprites/" + item.itemEffect.ToString());

        switch(n)
        {
            case 0:
                item.description = "Damage boost (+1) for each red colored weapon";
                item.name = "Red Rune";
                break;
            case 1:
                item.description = "Increases damage with bows by 3, but each arrow costs 1 gold";
                item.name = "Golden Bow String";
                break;
            case 2:
                item.description = "Heals you for 5 hp after each combat";
                item.name = "Syringe";
                break;
            case 3:
                item.description = "You gain an additional 5 gold after each combat";
                item.name = "Magnet";
                break;
            default:
                throw new System.Exception();
        }
        exists.Add(item.itemEffect.ToString());

        return item;
    }

    public static Item FromSerializable(SerializableItem item)
    {
        return new Item
        {
            fromVendor = item.fromVendor,
            itemEffect = item.itemEffect,
            price = item.price,
            name = item.name,
            description = item.description,
            sprite = Resources.Load<Sprite>("Sprites/" + item.itemEffect.ToString())
        };
    }

    /**
      * Applies an item's effect persistently.
      */
    public void applyItemEffect(Weapon weapon)
    {
      switch(itemEffect)
      {
        case ItemEffect.redRune:
          redRuneEffect(weapon);
          break;
        case ItemEffect.goldenBowString:
          goldenBowStringEffect(weapon);
          break;
      }
    }

  /**
    * Applies a flat 1 damage bonus to all red weapons.
    */
    public void redRuneEffect(Weapon weapon)
    {
      //if(weapon.Stats.color == Color.red)
      //  {
      //    weapon.Stats.isRedBuffed = true;
      //  }

    }

    /**
      * Makes every bow weapon takes 1 gold per attack, but adds 3 damage.
      */
      public void goldenBowStringEffect(Weapon weapon)
      {
        //if(weapon.Stats.type == WeaponStats.WeaponType.Bow)
        //{
        //  weapon.Stats.hasGoldenBowString = true;
        //}
      }

    /**
      * Heals 5 health at the end of a combat.
      */
      public void syringeEffect()
      {
        playerStats.GainHealth(5);
      }

    /**
      * Gain an additional 5 gold at the end of combat.
      */
      public void magnetEffect()
      {
        playerStats.GainGold(5);
      }

    internal SerializableItem ToSerializable()
    {
        return new SerializableItem
        {
            itemEffect = itemEffect,
            price = price,
            fromVendor = fromVendor, 
            name = name, 
            description = description
        };
    }
}

[System.Serializable]
public class SerializableItem
{
    public Item.ItemEffect itemEffect;
    public int price;
    public bool fromVendor;
    public string name, description;
}
