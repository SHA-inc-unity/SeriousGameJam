using UnityEngine;

[CreateAssetMenu(fileName = "VulnerableSlot", menuName = "Battle/Effects/Vulnerable")]
public class VulnerableSlotEffect : WheelSlotEffect
{
    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle)
    {
        // Self-applies to whoever spun the wheel, not the opponent.
        battle.StatusEffects.TryApply(attacker, new VulnerableStatus());

        BattleAudio battleAudio = FindAnyObjectByType<BattleAudio>();
        if (battleAudio && effectSounds.Count > 0) battleAudio.PlayClip(effectSounds[UnityEngine.Random.Range(0, effectSounds.Count)]);
    }
}