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

    public Combatant CreateRuntimeCombatant()
    {
        // Clone the wheel so any runtime mutations (e.g. BombEffect replacing
        // a slot) affect only this battle's copy, never the original asset.
        Wheel runtimeWheel = Instantiate(wheel);

        return new Combatant(displayName, maxHP, battleSprite, runtimeWheel);
    }
}