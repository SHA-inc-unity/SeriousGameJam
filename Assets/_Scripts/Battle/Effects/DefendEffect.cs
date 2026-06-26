using UnityEngine;

[CreateAssetMenu(fileName = "DefendEffect", menuName = "Battle/Effects/Defend")]
public class DefendEffect : WheelSlotEffect
{
    public override void Execute(Combatant attacker, Combatant defender, BattleManager battleManager)
    {
        if (battleManager.HasPendingDamageReduction(attacker))
        {
            battleManager.Announce($"{attacker.displayName} is already braced! Defend wasted.");
            return;
        }

        battleManager.SetPendingDamageReduction(attacker, 1);
        battleManager.Announce($"{attacker.displayName} braces! Next hit reduced by 1.");
    }
}