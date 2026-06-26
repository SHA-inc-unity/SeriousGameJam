using UnityEngine;

[CreateAssetMenu(fileName = "EvadeEffect", menuName = "Battle/Effects/Evade")]
public class EvadeEffect : WheelSlotEffect
{
    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle)
    {
        battle.StatusEffects.TryApply(attacker, new EvadeStatus());
    }
}