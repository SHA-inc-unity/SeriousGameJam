using UnityEngine;

[CreateAssetMenu(fileName = "NewSelfDamageEffect", menuName = "Battle/Effects/SelfDamage")]
public class SelfDamageEffect : WheelSlotEffect
{
    [Min(0)] public int damage = 1;

    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle, SpinResult spinResult)
    {
        battle.ApplyDamage(attacker, attacker, damage);
        BattleAudio battleAudio = FindAnyObjectByType<BattleAudio>();
        if (battleAudio && effectSounds.Count > 0) battleAudio.PlayClip(effectSounds[UnityEngine.Random.Range(0, effectSounds.Count)]);
    }
}