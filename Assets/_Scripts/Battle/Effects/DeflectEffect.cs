using UnityEngine;

[CreateAssetMenu(fileName = "DeflectEffect", menuName = "Battle/Effects/Deflect")]
public class DeflectEffect : WheelSlotEffect
{
    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle)
    {
        if (battle.HasPendingDeflect(attacker))
        {
            battle.Announce($"{attacker.displayName} is already deflecting! Deflect wasted.");
            return;
        }

        battle.SetPendingDeflect(attacker);
        battle.Announce($"{attacker.displayName} readies a deflect! Next hit gets sent back.");
    }
}