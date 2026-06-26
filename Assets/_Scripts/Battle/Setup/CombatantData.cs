using UnityEngine;

[CreateAssetMenu(fileName = "NewCombatantData", menuName = "Battle/Combatant Data")]
public class CombatantData : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    public Sprite battleSprite;

    [Header("Stats")]
    public int maxHP = 3;

    [Header("Permanent Progress (runtime only - resets on game restart)")]
    [Tooltip("Permanent HP earned from past battle wins this session. Added on top of maxHP.")]
    public int bonusMaxHP = 0;

    [Header("Wheel")]
    public Wheel wheel;

    [Header("Audio")]
    public BattleSoundSet soundSet;

    /// <summary>
    /// Builds a fresh runtime Combatant from this data. Call this when a
    /// battle actually starts - never mutate the asset's own fields at runtime,
    /// except via GrantPermanentHP, which is the one intentional exception.
    /// </summary>
    public Combatant CreateRuntimeCombatant()
    {
        // Clone the wheel so any runtime mutations (e.g. BombEffect replacing
        // a slot) affect only this battle's copy, never the original asset.
        Wheel runtimeWheel = Instantiate(wheel);

        int effectiveMaxHP = maxHP + bonusMaxHP;

        return new Combatant(displayName, effectiveMaxHP, battleSprite, runtimeWheel);
    }

    /// <summary>
    /// Permanently (for this play session) increases this combatant's max HP
    /// by the given amount. Intended to be called on the player's CombatantData
    /// when a battle is won.
    /// </summary>
    public void GrantPermanentHP(int amount)
    {
        bonusMaxHP += amount;
    }
}