using UnityEngine;

// Sets attacker.isDefending = true, which Combatant.TakeDamage() already
// reads to halve incoming damage on the NEXT hit this combatant takes.
[CreateAssetMenu(fileName = "DefendEffect", menuName = "Battle/Effects/Defend")]
public class DefendEffect : WheelSlotEffect
{
    public override void Execute(Combatant attacker, Combatant defender, BattleManager battleManager)
    {
        attacker.isDefending = true;
        Debug.Log($"{attacker.displayName} braces to defend (reduced damage next hit).");
    }
}