using UnityEngine;

// Minimal combatant data. Used for both the player knight and enemies
// (the duck-stall guy, etc). Keep this lean for jam purposes -
// add fields only when a system actually needs them.
[System.Serializable]
public class Combatant
{
    public string displayName;
    public int maxHP;
    public int currentHP;
    public int attackPower = 10;
    public bool isDefending;

    [Tooltip("Sprite shown for this combatant in the battle scene.")]
    public Sprite battleSprite;

    [Tooltip("The wheel this combatant spins. Each combatant owns their own wheel " +
             "reference so the player's equipped wheel and an enemy's wheel can " +
             "differ and be swapped independently (e.g. wheel unlocks/upgrades).")]
    public Wheel wheel;

    public Combatant(string name, int maxHP, int attackPower, Sprite battleSprite = null, Wheel wheel = null)
    {
        this.displayName = name;
        this.maxHP = maxHP;
        this.currentHP = maxHP;
        this.attackPower = attackPower;
        this.battleSprite = battleSprite;
        this.wheel = wheel;
    }

    public bool IsDefeated => currentHP <= 0;

    public void TakeDamage(int amount)
    {
        int finalDamage = isDefending ? Mathf.RoundToInt(amount * 0.5f) : amount;
        currentHP = Mathf.Max(0, currentHP - finalDamage);
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }
}