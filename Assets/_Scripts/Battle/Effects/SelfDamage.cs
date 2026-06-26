using UnityEngine;

[CreateAssetMenu(fileName = "NewSelfDamageEffect", menuName = "Battle/Effects/SelfDamage")]
public class SelfDamageEffect : WheelSlotEffect
{
    [Min(0)] public int damage = 1;

    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle)
    {
        battle.ApplyDamage(attacker, attacker, damage);
    }
}