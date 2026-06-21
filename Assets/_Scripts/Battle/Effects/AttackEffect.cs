using UnityEngine;

// Deals damage to the defender based on the attacker's attackPower,
// scaled by an optional multiplier (1.0 = normal hit, 2.0 = double damage, etc).
// Reusing this same class with a higher multiplier IS your "Crit" slot -
// no separate CritEffect needed unless you want crit-specific extras (VFX flag, etc).
[CreateAssetMenu(fileName = "AttackEffect", menuName = "Battle/Effects/Attack")]
public class AttackEffect : WheelSlotEffect
{
    [Tooltip("Multiplier applied to attacker.attackPower. 1 = normal hit, 2 = crit-style double damage.")]
    public float damageMultiplier = 1f;

    public override void Execute(Combatant attacker, Combatant defender, BattleManager battleManager)
    {
        int damage = Mathf.RoundToInt(attacker.attackPower * damageMultiplier);
        defender.TakeDamage(damage);
        Debug.Log($"{attacker.displayName} attacks for {damage}! " +
                  $"{defender.displayName} HP: {defender.currentHP}/{defender.maxHP}");
    }
}