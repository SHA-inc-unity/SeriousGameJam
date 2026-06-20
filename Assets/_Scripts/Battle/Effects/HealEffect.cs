using UnityEngine;

// Heals the attacker (the one who spun this) for a percentage of their max HP.
[CreateAssetMenu(fileName = "HealEffect", menuName = "Battle/Effects/Heal")]
public class HealEffect : WheelSlotEffect
{
    [Tooltip("Percentage of attacker's max HP to restore. 0.2 = heal 20% of max HP.")]
    [Range(0f, 1f)]
    public float healPercent = 0.2f;

    public override void Execute(Combatant attacker, Combatant defender)
    {
        int healAmount = Mathf.RoundToInt(attacker.maxHP * healPercent);
        attacker.Heal(healAmount);
        Debug.Log($"{attacker.displayName} heals for {healAmount}! " +
                  $"HP: {attacker.currentHP}/{attacker.maxHP}");
    }
}