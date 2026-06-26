using UnityEngine;

[CreateAssetMenu(fileName = "NewCombatantData", menuName = "Battle/Combatant Data")]
public class CombatantData : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    public Sprite battleSprite;

    [Header("Stats")]
    public int maxHP = 3;

    [Header("Wheel")]
    public Wheel wheel;

    [Header("Audio")]
    public BattleSoundSet soundSet;

    /// <summary>
    /// Builds a fresh runtime Combatant from this data. Call this when a
    /// battle actually starts - never mutate the asset's own fields at runtime.
    /// </summary>
    public Combatant CreateRuntimeCombatant()
    {
        // Clone the wheel so any runtime mutations (e.g. BombEffect replacing
        // a slot) affect only this battle's copy, never the original asset.
        Wheel runtimeWheel = Instantiate(wheel);

        return new Combatant(displayName, maxHP, battleSprite, runtimeWheel);
    }
}