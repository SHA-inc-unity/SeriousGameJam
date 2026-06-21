using UnityEngine;

// The AUTHORED data for a combatant - one asset per character (the Knight,
// Hook-a-Duck Guy, any future enemy). This is what you'd tweak in the
// Inspector. It is NOT the same as the Combatant class in Combatant.cs -
// that's the RUNTIME object (tracks current HP, active statuses, etc) that
// gets built FROM this data when a battle actually starts.
//
// Create via: right-click in Project window -> Create -> Battle -> Combatant Data
[CreateAssetMenu(fileName = "NewCombatantData", menuName = "Battle/Combatant Data")]
public class CombatantData : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    [Tooltip("Shown in the battle scene for this combatant.")]
    public Sprite battleSprite;

    [Header("Stats")]
    public int maxHP = 50;
    public int attackPower = 10;

    [Header("Wheel")]
    [Tooltip("The wheel this combatant spins in battle.")]
    public Wheel wheel;

    /// <summary>
    /// Builds a fresh runtime Combatant from this data. Call this when a
    /// battle actually starts - never mutate the asset's own fields at runtime.
    /// </summary>
    public Combatant CreateRuntimeCombatant()
    {
        return new Combatant(displayName, maxHP, attackPower, battleSprite, wheel);
    }
}