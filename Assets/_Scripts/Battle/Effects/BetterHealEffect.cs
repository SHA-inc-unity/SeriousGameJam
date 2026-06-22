using UnityEngine;

[CreateAssetMenu(fileName = "BigHealEffect", menuName = "Battle/Effects/BigHeal")]
public class BigHealEffect : WheelSlotEffect
{
    [Tooltip("Total HP restored. Anything beyond max HP becomes overhealth.")]
    public int healAmount = 2;

    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle)
    {
        attacker.HealWithOverhealth(healAmount);
        battle.NotifyHPChanged(attacker);
        battle.Announce($"{attacker.displayName} heals for {healAmount}! " +
                        $"HP: {attacker.currentHP}/{attacker.maxHP} " +
                        $"(+{attacker.overhealth} overhealth)");
    }
}