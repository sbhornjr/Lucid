using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

/**
 * stores all of the player's stats.
 * this script contains all info that will possible persist between levels.
 */

public class PlayerStats : MonoBehaviour
{
    public int maxHealth = 100;

    [SerializeField]
    private GameObject weaponPrefab;

    [SerializeField]
    GameObject weaponUIPrefab, itemUIPrefab, itemBar;

    public int CurrentHealth { get; private set; }

    public int EnemiesKilled { get; private set; }

    public int TilNextSlot { get; private set; }

    public Weapon[] weapons;
    public int WeaponSlots;

    public List<Item> items;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private TextMeshProUGUI healthText;

    [SerializeField]
    private TextMeshProUGUI goldText;

    [SerializeField]
    AudioClip gotGoldSFX, gotHealthSFX, boughtSomethingSFX;

    [SerializeField]
    Camera combatCamera;

    [SerializeField]
    Image shieldImage, shieldFill;

    public Image deflectImage;

    public int shield;
    public int gold;
    public List<float> deflects;
    float totalDeflected;

    Weapon possibleNewWeapon;
    Item possibleNewItem;
    KeyCode[] keyCodes = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L };

    int[] toUnlock = { 1, 10, 30, 75 };

    /**
     * we must have died (or otherwise starting a new game).
     * wipe all of our info and set necessary base values.
     */
    public void Reset()
    {
        CurrentHealth = maxHealth;

        // destroy all weapons
        for (var i = 0; i < weapons.Length; ++i)
        {
            if (weapons[i] != null)
            {
                weapons[i].DestroyUI();
                Destroy(weapons[i]);
            }
        }

        items = new List<Item>();

        // create started weapon and pick it up
        weapons = new Weapon[1] { gameObject.Instantiate(weaponPrefab, 1).GetComponent<Weapon>() };
        weapons[0].WeaponPickedUp(keyCodes[0], 0);

        // reset base values
        EnemiesKilled = 0;
        TilNextSlot = 1;
        WeaponSlots = 1;
        gold = 0;
        shield = 0;
        deflects = new List<float>();
        totalDeflected = 0;

        // reset UI values
        healthText.text = CurrentHealth.ToString() + "/" + maxHealth.ToString();
        slider.maxValue = maxHealth;
        slider.value = CurrentHealth;
        goldText.text = "0";
    }

    /**
     * an enemy hit us -> take the specified amount of damage
     */
    public void TakeDamage(int dmg)
    {
        if (deflects.Count != 0)
        {
            dmg -= (int)Mathf.Floor(dmg * (totalDeflected / 100));
            deflects.Clear();
            deflectImage.enabled = false;
            deflectImage.gameObject.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
            totalDeflected = 1f;
        }

        if (shield == 0)
        {
            CurrentHealth -= dmg;
        }
        else
        {
            if (dmg >= shield)
            {
                dmg -= shield;
                shield = 0;
                ToggleShield(false);
                CurrentHealth -= dmg;
            }
            else
            {
                shield -= dmg;
                ToggleShield(true);
            }

        }
        healthText.text = CurrentHealth.ToString() + "/" + maxHealth.ToString();
        slider.value = CurrentHealth;
    }

    /**
     * we gain the amount of health specified
     */
    public void GainHealth(int health)
    {
        if (CombatManager.inCombat) AudioSource.PlayClipAtPoint(gotHealthSFX, combatCamera.transform.position);
        else AudioSource.PlayClipAtPoint(gotHealthSFX, Camera.main.transform.position);
        CurrentHealth += (health * maxHealth) / 100;
        if (CurrentHealth > maxHealth) CurrentHealth = maxHealth;
        healthText.text = CurrentHealth.ToString() + "/" + maxHealth.ToString();
        slider.value = CurrentHealth;
        
    }

    /**
     * our max health increases by the amount specified
     */
    public void IncreaseMaxHealth(int health)
    {
        CurrentHealth += (health * maxHealth) / 100;
        maxHealth += (health * maxHealth) / 100;

        slider.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(maxHealth, 20);
        healthText.text = CurrentHealth.ToString() + "/" + maxHealth.ToString();
        slider.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(maxHealth * 2, 80);
        slider.maxValue = maxHealth;
        slider.value = CurrentHealth;
        AudioSource.PlayClipAtPoint(gotHealthSFX, Camera.main.transform.position);
    }

    public void GainGold(int gain)
    {
        Camera cam;
        if (CombatManager.inCombat)
            cam = combatCamera;
        else cam = Camera.main;

        gold += gain;
        goldText.text = gold.ToString();
        AudioSource.PlayClipAtPoint(gotGoldSFX, cam.transform.position);
    }

    public void LoseGold(int loss)
    {
        Camera cam;
        if (CombatManager.inCombat)
            cam = combatCamera;
        else cam = Camera.main;

        gold -= loss;
        gold = Mathf.Clamp(gold, 0, 99999);
        goldText.text = gold.ToString();
        AudioSource.PlayClipAtPoint(boughtSomethingSFX, cam.transform.position);
    }

    public void GainShield(int gain)
    {
        shield += gain;
        ToggleShield(true);
    }

    public void ToggleShield(bool which)
    {
        shieldFill.GetComponent<Image>().enabled = which;
        shieldImage.GetComponent<Image>().enabled = which;
        var shieldText = shieldImage.GetComponentInChildren<TextMeshProUGUI>();
        shieldText.enabled = which;
        shieldText.text = shield.ToString();
    }

    public void Deflect(float amt)
    {
        if (deflects.Count == 0)
        {
            deflectImage.enabled = true;
        }
        deflects.Add(amt);
        float theoreticalDmg = 100f;
        foreach (var deflect in deflects) theoreticalDmg = theoreticalDmg - deflect * theoreticalDmg;
        totalDeflected = 100 - theoreticalDmg;
        deflectImage.gameObject.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
        deflectImage.gameObject.GetComponentInChildren<TextMeshProUGUI>().text
            = Math.Floor(totalDeflected).ToString() + "%";
    }

    /**
     * we killed an enemy -> increment that value
     */
    public void KilledEnemy()
    {
        EnemiesKilled += 1;
    }

    /**
     * we killed enough enemies to unlock a weapon slot -> make room in our list
     */
    public void UnlockedSlot()
    {
        WeaponSlots += 1;

        if (WeaponSlots == 5)
        {
            TilNextSlot = 10000;
        }
        else
        {
            TilNextSlot = toUnlock[WeaponSlots - 1];   // side note we may want to change this system of progression
        }

        var temp = weapons;
        weapons = new Weapon[WeaponSlots];
        for (var i = 0; i < temp.Length; ++i)
            weapons[i] = temp[i];
    }

    /**
     * set up a weapon UI for each of our current weapons for the post-combat weapon list
     */
    internal void SetupWeaponUIs(GameObject playerWeapons)
    {
        var list = playerWeapons.transform.GetChild(0).GetChild(0);
        // make sure that no weapon list items exist right now
        for (var i = 0; i < weapons.Length; ++i)
        {
            // try catch because if it's a new slot or the first time it won't be there
            try
            {
                Destroy(list.GetChild(i).gameObject);
            }
            catch
            {

            }
        }

        // loop through each weapon to create the UI
        for (var i = 0; i < weapons.Length; ++i)
        {
            // create a UI object and get the before and after hover objects
            var weaponUI = Instantiate(weaponUIPrefab);

            // tell the object to set its UI elements
            OldWeaponClick owc = weaponUI.GetComponent<OldWeaponClick>();
            owc.SetUpUI(weapons[i]);
            weaponUI.GetComponent<OldWeaponUpgrade>().enabled = false;

            // set the weapon UI element's index, a reference to its weapon, and its parent
            owc.index = i;
            owc.weapon = weapons[i];
            owc.playerWeapons = playerWeapons;
            weaponUI.transform.SetParent(list);
        }
    }

    /**
     * set up a weapon UI for each of our current weapons for the post-combat weapon list
     */
    public void SetupUpgradeUIs(GameObject playerWeapons)
    {
        playerWeapons.SetActive(true); 
        var list = playerWeapons.transform.GetChild(0).GetChild(0);
        // make sure that no weapon list items exist right now
        for (var i = 0; i < weapons.Length; ++i)
        {
            // try catch because if it's a new slot or the first time it won't be there
            try
            {
                Destroy(list.GetChild(i).gameObject);
            }
            catch
            {

            }
        }

        // loop through each weapon to create the UI
        for (var i = 0; i < weapons.Length; ++i)
        {
            // create a UI object and get the before and after hover objects
            var weaponUI = Instantiate(weaponUIPrefab);

            // tell the object to set its UI elements
            OldWeaponUpgrade owu = weaponUI.GetComponent<OldWeaponUpgrade>();
            owu.SetUpUI(weapons[i]);
            weaponUI.GetComponent<OldWeaponClick>().enabled = false;

            // set the weapon UI element's index, a reference to its weapon, and its parent
            owu.index = i;
            owu.weapon = weapons[i];
            owu.playerWeapons = playerWeapons;
            weaponUI.transform.SetParent(list);
        }
    }

    /**
     * set up a item UI for each of our current items for the post-combat item list
     */
    internal void SetupItemUIs(GameObject playerItems)
    {
        var list = playerItems.transform.GetChild(0).GetChild(0);
        // make sure that no weapon list items exist right now
        for (var i = 0; i < 4; ++i)
        {
            // try catch because if it's a new slot or the first time it won't be there
            try
            {
                Destroy(list.GetChild(i).gameObject);
            }
            catch
            {

            }
        }

        // loop through each weapon to create the UI
        for (var i = 0; i < 4; ++i)
        {
            // create a UI object and get the before and after hover objects
            var itemUI = Instantiate(itemUIPrefab);

            // tell the object to set its UI elements
            OldItemClick oic = itemUI.GetComponent<OldItemClick>();
            if (items.Count > i)
            {
                oic.SetUpUI(items[i]);

                // set the weapon UI element's index, a reference to its weapon, and its parent
                oic.index = i;
                oic.item = items[i];
            }
            else
            {
                oic.SetUpUI(null);
                oic.index = i;
            }
            
            oic.playerItems = playerItems;
            itemUI.transform.SetParent(list);
        }
    }

    /**
     * we have an opportunity to pick up a weapon - store that possible new weapon
     */
    public void PossibleNewWeapon(Weapon weapon)
    {
        possibleNewWeapon = weapon;
    }

    /**
     * we are picking up that possible new weapon -> so swap out the current weapon
     */
    public void SwapWeapons(int index)
    {
        if (weapons[index] != null)
            weapons[index].DestroyUI();
        if (possibleNewWeapon.Stats.fromVendor)
        {
            LoseGold(possibleNewWeapon.Stats.price);
            possibleNewWeapon.Stats.fromVendor = false;
            if (weapons[index] != null) weapons[index].Stats.fromVendor = true;
        }
        weapons[index] = possibleNewWeapon;
        weapons[index].WeaponPickedUp(keyCodes[index], index);
        possibleNewWeapon = null;
    }

    /**
     * we are picking up that possible new item -> so swap out the current item
     */
    public void SwapItems(int index)
    {
        if (possibleNewItem.fromVendor)
        {
            LoseGold(possibleNewItem.price);
            possibleNewItem.fromVendor = false;
            if (index < items.Count && items[index] != null) items[index].fromVendor = true;
        }
        if (index >= items.Count)
        {
            index = items.Count;
            items.Add(possibleNewItem);
        }
        else items[index] = possibleNewItem;
        itemBar.transform.GetChild(index).GetComponent<Image>().color = new Color(1, 1, 1, 1);
        itemBar.transform.GetChild(index).GetComponent<Image>().sprite = possibleNewItem.sprite;
        foreach (Weapon weapon in weapons)
            possibleNewItem.applyItemEffect(weapon);
        possibleNewItem = null;
    }

    /**
     * we did not pick up the new weapon -> destroy it
     */
    public void KillPossibleNewWeapon()
    {
        if (possibleNewWeapon != null)
            Destroy(possibleNewWeapon.gameObject);
    }

    /**
     * we have an opportunity to pick up a item - store that possible new item
     */
    public void PossibleNewItem(Item item)
    {
        possibleNewItem = item;
    }

    public SerializablePlayerStats ToSerializable()
    {
        var serializableWeapons = new SerializableWeapon[weapons.Length];
        for (int i = 0; i < serializableWeapons.Length; i++)
        {
            serializableWeapons[i] = weapons[i].ToSerializable(i);
        } 

        return new SerializablePlayerStats
        {
            currentHealth = CurrentHealth,
            maxHealth = maxHealth,
            enemiesKilled = EnemiesKilled,
            tilNextSlot = TilNextSlot,
            weapons = serializableWeapons,
            weaponSlots = WeaponSlots,
            items = items.Select(i => i.ToSerializable()).ToList(),
            gold = gold,
            currentLevel = GameStateMachine.CurrentLevel
        };
    }

    public GameStateMachine.Level FromSerializable(SerializablePlayerStats stats)
    {
        CurrentHealth = stats.currentHealth;
        maxHealth = stats.maxHealth;
        EnemiesKilled = stats.enemiesKilled;
        TilNextSlot = stats.tilNextSlot;

        slider.maxValue = maxHealth;
        slider.value = CurrentHealth;
        healthText.text = $"{CurrentHealth}/{maxHealth}";

        gold = stats.gold; 
        goldText.text = gold.ToString();
        
        items = new List<Item>(stats.items.Count);

        foreach (var item in stats.items)
        {
            items.Add(Item.FromSerializable(item));
        }

        WeaponSlots = stats.weaponSlots;
        weapons = new Weapon[WeaponSlots];

        for (int i = 0; i < stats.weapons.Length; i++)
        {
            var weaponStat = stats.weapons[i];
            weapons[i] = gameObject.Instantiate(weaponPrefab, weaponStat.weaponStats.level).GetComponent<Weapon>();
            weapons[i].FromSerializable(weaponStat, this);
        }

        return stats.currentLevel;
    }
}

[Serializable]
public class SerializablePlayerStats
{
    public int currentHealth, maxHealth, enemiesKilled, tilNextSlot;
    
    public SerializableWeapon[] weapons;
    public int weaponSlots;

    public List<SerializableItem> items;

    public int gold;

    public GameStateMachine.Level currentLevel;
}

