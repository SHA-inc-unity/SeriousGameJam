using UnityEngine;

[System.Serializable]
public class Combatant
{
    public string displayName;
    public int maxHP;
    public int currentHP;
    public int overhealth;

    [Tooltip("Sprite shown for this combatant in the battle scene.")]
    public Sprite battleSprite;

    [Tooltip("The wheel this combatant spins.")]
    public Wheel wheel;

    public Combatant(string name, int maxHP, Sprite battleSprite = null, Wheel wheel = null)
    {
        this.displayName  = name;
        this.maxHP        = maxHP;
        this.currentHP    = maxHP;
        this.overhealth   = 0;
        this.battleSprite = battleSprite;
        this.wheel        = wheel;
    }

    public bool IsDefeated => currentHP <= 0 && overhealth <= 0;

    public void TakeDamage(int amount)
    {
        // Drain overhealth first
        if (overhealth > 0)
        {
            int absorbed = Mathf.Min(overhealth, amount);
            overhealth  -= absorbed;
            amount      -= absorbed;
        }

        if (amount > 0)
            currentHP = Mathf.Max(0, currentHP - amount);
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }

    public void HealWithOverhealth(int amount)
    {
        int hpNeeded = maxHP - currentHP;

        if (amount <= hpNeeded)
        {
            currentHP += amount;
        }
        else
        {
            currentHP   = maxHP;
            int excess  = amount - hpNeeded;
            overhealth  = Mathf.Min(overhealth + excess, maxHP); // never exceeds maxHP
        }
    }
}