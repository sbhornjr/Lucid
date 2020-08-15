using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/**
 * represents a weapon including stats and the combat UI element.
 * this is what tells the combat manager that the weapon is attacking
 */
public class Weapon : MonoBehaviour
{
    [SerializeField]
    private GameObject weaponUIPrefab;

    [SerializeField]
    private AudioClip[] weaponSFX;

    [SerializeField]
    GameObject equipmentBar;

    public AudioClip SFX;
    public string weaponAnimTrigger;

    public WeaponStats Stats { get; private set; }
    public WeaponRenderer Renderer { get; private set; }

    public int Damage { get { return Stats.dmg; } }

    public int DoDamage { get { return DamageToDo(); } }

    private float cooldown;
    int numHalvedFor;

    public float Multiplier;
    public List<float> Multipliers;
    public List<float> MultiplierCooldowns;

    PlayerStats playerStats;

    /**
     * create a random stat line and get necessary references
     */
    public void Awake()
    {
        //equipmentBar = GameObject.FindGameObjectWithTag("EquipmentBar");
        transform.parent = GameObject.FindGameObjectWithTag("WeaponParent").transform;
    }

    internal bool Upgrade()
    {
        if (Stats.level == 5) return false;
        else
        {
            Stats.level++;
            Stats.dmg = Stats.level * Stats.cooldown;
            return true;
        }
    }

    public void InitStats(int level)
    {
        if (level == 0) Stats = WeaponStats.RandomStats();
        else Stats = WeaponStats.RandomStats(level);
        AssignAudioAndAnimation();
    }

    public void FromSerializable(SerializableWeapon weapon, PlayerStats parent)
    {
        Stats = WeaponStats.FromSerializable(weapon);
        AssignAudioAndAnimation();

        playerStats = parent;

        var index = weapon.index;
        var ui = Instantiate(weaponUIPrefab);
        Renderer = new WeaponRenderer(ui, Stats, weapon.keyCode, index);
        //equipmentBar.transform.GetChild(index + 2).GetComponent<Image>().sprite = Stats.sprite;

        numHalvedFor = 0;
        Multipliers = new List<float>(2);
        MultiplierCooldowns = new List<float>(2);

        for (int itemIndex = 0; itemIndex < playerStats.items.Count; ++itemIndex)
        {
            playerStats.items[itemIndex].applyItemEffect(this);
        }
    }

    /**
     * different weapon types have different sound effects and animations -> find this one's sound
     */
    private void AssignAudioAndAnimation()
    {
        switch(Stats.type)
        {
            case WeaponStats.WeaponType.Bow:
                SFX = weaponSFX[1];
                weaponAnimTrigger = "animBow";
                break;
            case WeaponStats.WeaponType.Sword:
                SFX = weaponSFX[0];
                weaponAnimTrigger = "animOneHand";
                break;
            case WeaponStats.WeaponType.Dagger:
                SFX = weaponSFX[0];
                weaponAnimTrigger = "animStab";
                break;
            default:
                SFX = weaponSFX[0];
                weaponAnimTrigger = "animTwoHand";
                break;
        }
    }

    /**
     * the player picked me up! -> create a combat UI element for this weapon and update the equipment bar
     * keyCode is the key that this weapon will use in combat
     * index is the # weapon we are in the player's list
     * also checks for items that modify the weapon.
     */
    public void WeaponPickedUp(KeyCode keyCode, int index)
    {
        var ui = Instantiate(weaponUIPrefab);
        Renderer = new WeaponRenderer(ui, Stats, keyCode, index);
        //equipmentBar.transform.GetChild(index + 2).GetComponent<Image>().sprite = Stats.sprite;
        playerStats = FindObjectOfType<PlayerStats>();
        numHalvedFor = 0;
        Multipliers = new List<float>();
        MultiplierCooldowns = new List<float>();

        //for(int itemIndex = 0; itemIndex < playerStats.items.Count; ++itemIndex)
        //{
        //  playerStats.items[itemIndex].applyItemEffect(this);
        //}
    }

    /**
     * we are starting combat -> reset UI values
     */
    public void StartCombat()
    {
        cooldown = 0;
        Renderer.timeText.text = Stats.cooldown.ToString();
        Renderer.slider.maxValue = Stats.cooldown;
        Renderer.slider.value = cooldown;
        Renderer.multiplierText.enabled = false;
        Renderer.timeShortenImage.enabled = false;
        Renderer.timeShortenImage.GetComponentInChildren<TextMeshProUGUI>().enabled = false;

        Multipliers.Clear();
        MultiplierCooldowns.Clear();
        numHalvedFor = 0;
        Multiplier = 0;
    }

    /**
     * this is called every frame by the combat manager while we are in combat
     */
    public bool IsWeaponActivated()
    {
        // multiplier countdown stuff
        for (var i = MultiplierCooldowns.Count - 1; i >= 0; i--)
        {
            MultiplierCooldowns[i] -= Time.deltaTime;
            if (MultiplierCooldowns[i] <= 0)
            {
                MultiplierCooldowns.RemoveAt(i);
                Multipliers.RemoveAt(i);
            }
        }

        var oldTime = Stats.cooldown - cooldown;

        // increment the cooldown
        cooldown += Time.deltaTime;
        cooldown = Mathf.Clamp(cooldown, 0, Stats.cooldown);

        // the cooldown is done and the player clicked my key -> attack!
        var attack = false;
        if (cooldown >= Stats.cooldown && Input.GetKeyDown(Renderer.keyCode))
        {
            // show in the UI that we are attacking
            Renderer.attackText.text = Stats.attackName + "!";
            Renderer.attackText.enabled = true;
            Invoke("DisableAttackText", 1);

            if (numHalvedFor > 0)
            {
                cooldown = Mathf.Floor(Stats.cooldown / 2);
                numHalvedFor -= 1;
            }
            if (numHalvedFor == 0)
            {
                Renderer.timeShortenImage.enabled = false;
                Renderer.timeShortenImage.gameObject.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
                cooldown = 0;
            }

            Multiplier = 0;

            attack = true;
        }

        // update the slider value and update the time text if necessary
        Renderer.slider.value = cooldown;
        if (cooldown >= Stats.cooldown) Renderer.timeText.text = "0";
        else if (Mathf.Floor(oldTime) != Mathf.Floor(Stats.cooldown - cooldown))
            Renderer.timeText.text = Mathf.Floor(oldTime).ToString();

        return attack;
    }

    /**
     * disable the attack text -> called 1 second after it is enabled
     */
    void DisableAttackText()
    {
        Renderer.attackText.enabled = false;
    }

    /**
     * disable the multiplier text -> called 1 second after it is enabled
     */
    void DisableMultiplierText()
    {
        Renderer.multiplierText.enabled = false;
    }

    /**
     * i got dropped by the player -> destroy my UI
     */
    public void DestroyUI()
    {
        Destroy(Renderer.slider);
        Destroy(Renderer.ui);
    }

    int DamageToDo()
    {
        //int result = 0;
        float multi = 1;
        if (Multiplier > 0)
        {
            multi = Multiplier;
            Renderer.multiplierText.text = multi.ToString() + "x";
            Renderer.multiplierText.enabled = true;
            Invoke("DisableMultiplierText", 1);
        }

        //result = multi * Stats.dmg;

        //if (Stats.isRedBuffed)
        //{
        //  result += 1;
        //}

        //if (Stats.hasGoldenBowString)
        //{
        //  result += 3;
        //  playerStats.LoseGold(1, true);
        //}

        return (int)Mathf.Floor(multi * Stats.dmg);
    }

    public void Synergy(List<Weapon> othersAttacking)
    {
        switch(Stats.level)
        {
            case 1:
                Level1Synergy(othersAttacking);
                break;
            case 2:
                Level2Synergy(othersAttacking);
                break;
            case 3:
                Level3Synergy(othersAttacking);
                break;
            case 4:
                Level4Synergy(othersAttacking);
                break;
            case 5:
                Level5Synergy(othersAttacking);
                break;
            default:
                throw new Exception();

        }
    }

    private void Level5Synergy(List<Weapon> othersAttacking)
    {
        switch (Stats.synergy)
        {
            case WeaponStats.Synergy.MoreDmg:
                MoreDmg(3f);
                foreach (Weapon weapon in othersAttacking)
                    weapon.MoreDmg(3f);
                break;
            case WeaponStats.Synergy.DmgBuffOverTime:
                DmgBuffOverTime(3f, 5);
                break;
            case WeaponStats.Synergy.GainShield:
                GainShield(15);
                break;
            case WeaponStats.Synergy.Deflect:
                Deflect(.8f);
                break;
            case WeaponStats.Synergy.PlayerTimeShorten:
                PlayerTimeShorten(9999);
                break;
            case WeaponStats.Synergy.EnemyTimeLengthen:
                EnemyTimeLengthen(9999);
                break;
            default:
                break;
        }
    }

    private void Level4Synergy(List<Weapon> othersAttacking)
    {
        switch (Stats.synergy)
        {
            case WeaponStats.Synergy.MoreDmg:
                MoreDmg(2);
                foreach (Weapon weapon in othersAttacking)
                    weapon.MoreDmg(2);
                break;
            case WeaponStats.Synergy.DmgBuffOverTime:
                DmgBuffOverTime(2, 5);
                break;
            case WeaponStats.Synergy.GainShield:
                GainShield(10);
                break;
            case WeaponStats.Synergy.Deflect:
                Deflect(.65f);
                break;
            case WeaponStats.Synergy.PlayerTimeShorten:
                PlayerTimeShorten(2);
                foreach (Weapon weapon in othersAttacking)
                    weapon.PlayerTimeShorten(2);
                break;
            case WeaponStats.Synergy.EnemyTimeLengthen:
                EnemyTimeLengthen(othersAttacking.Count + 1);
                break;
            default:
                break;
        }
    }

    private void Level3Synergy(List<Weapon> othersAttacking)
    {
        switch (Stats.synergy)
        {
            case WeaponStats.Synergy.MoreDmg:
                MoreDmg(1.5f);
                foreach (Weapon weapon in othersAttacking)
                    weapon.MoreDmg(1.5f);
                break;
            case WeaponStats.Synergy.DmgBuffOverTime:
                DmgBuffOverTime(2, 3);
                break;
            case WeaponStats.Synergy.GainShield:
                GainShield(8);
                break;
            case WeaponStats.Synergy.Deflect:
                Deflect(.5f);
                break;
            case WeaponStats.Synergy.PlayerTimeShorten:
                PlayerTimeShorten(2);
                break;
            case WeaponStats.Synergy.EnemyTimeLengthen:
                EnemyTimeLengthen(3);
                break;
            default:
                break;
        }
    }

    private void Level2Synergy(List<Weapon> othersAttacking)
    {
        switch (Stats.synergy)
        {
            case WeaponStats.Synergy.MoreDmg:
                MoreDmg(2);
                break;
            case WeaponStats.Synergy.DmgBuffOverTime:
                DmgBuffOverTime(1.5f, 5);
                break;
            case WeaponStats.Synergy.GainShield:
                GainShield(5);
                break;
            case WeaponStats.Synergy.Deflect:
                Deflect(.4f);
                break;
            case WeaponStats.Synergy.PlayerTimeShorten:
                PlayerTimeShorten(1);
                foreach (Weapon weapon in othersAttacking)
                    weapon.PlayerTimeShorten(1);
                break;
            case WeaponStats.Synergy.EnemyTimeLengthen:
                EnemyTimeLengthen(2);
                break;
            default:
                break;
        }
    }

    private void Level1Synergy(List<Weapon> othersAttacking)
    {
       switch(Stats.synergy)
       {
            case WeaponStats.Synergy.MoreDmg:
                MoreDmg(1.5f);
                break;
            case WeaponStats.Synergy.DmgBuffOverTime:
                DmgBuffOverTime(1.5f, 3);
                break;
            case WeaponStats.Synergy.GainShield:
                GainShield(3);
                break;
            case WeaponStats.Synergy.Deflect:
                Deflect(.2f);
                break;
            case WeaponStats.Synergy.PlayerTimeShorten:
                PlayerTimeShorten(1);
                break;
            case WeaponStats.Synergy.EnemyTimeLengthen:
                EnemyTimeLengthen(1);
                break;
            default:
                break;
       }
    }

    internal SerializableWeapon ToSerializable(int weaponIndex)
    {
        return new SerializableWeapon
        {
            weaponStats = Stats,
            keyCode = Renderer.keyCode,
            index = weaponIndex
        };
    } 

    private void EnemyTimeLengthen(int numLengthenedFor)
    {
        CombatManager.enemyStats.LengthenTime(numLengthenedFor);
    }

    private void PlayerTimeShorten(int numHalvedFor)
    {
        this.numHalvedFor += numHalvedFor;

        if (cooldown == 0)
        {
            cooldown += Mathf.Floor(Stats.cooldown / 2);
            this.numHalvedFor -= 1;
        }

        Renderer.timeShortenImage.enabled = true;
        var txt = Renderer.timeShortenImage.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        txt.enabled = true;
        txt.text = numHalvedFor.ToString();
    }

    private void Deflect(float percent)
    {
        playerStats.Deflect(percent);
    }

    private void GainShield(int shield)
    {
        playerStats.GainShield(shield);
    }

    private void DmgBuffOverTime(float multiplier, float time)
    {
        Multipliers.Add(multiplier);
        MultiplierCooldowns.Add(time);
    }

    private void MoreDmg(float multiplier)
    {
        Multiplier += multiplier;
    }
}

[Serializable]
public class SerializableWeapon
{
    public WeaponStats weaponStats;
    public KeyCode keyCode;
    public int index;
}
