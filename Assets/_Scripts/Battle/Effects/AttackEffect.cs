using UnityEngine;

[CreateAssetMenu(fileName = "AttackEffect", menuName = "Battle/Effects/Attack")]
public class AttackEffect : WheelSlotEffect
{
    [Tooltip("Flat damage this slot deals.")]
    public int damage = 1;

    public override void Execute(Combatant attacker, Combatant defender, BattleManager battleManager)
    {
        battleManager.ApplyDamage(attacker, defender, damage);

        BattleAudio battleAudio = FindAnyObjectByType<BattleAudio>();
        if (battleAudio && effectSounds.Count > 0) battleAudio.PlayClip(effectSounds[UnityEngine.Random.Range(0, effectSounds.Count)]);
    }
}