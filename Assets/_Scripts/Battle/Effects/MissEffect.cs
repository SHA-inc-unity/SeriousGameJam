using UnityEngine;

// The "nothing happens" slot. Every wheel should probably have at least
// one of these so the wheel isn't guaranteed-good every spin.
[CreateAssetMenu(fileName = "MissEffect", menuName = "Battle/Effects/Miss")]
public class MissEffect : WheelSlotEffect
{
    public override void Execute(Combatant attacker, Combatant defender, BattleManager battleManager, SpinResult spinResult)
    {
        Debug.Log($"{attacker.displayName}'s spin landed on nothing. No effect.");

        BattleAudio battleAudio = FindAnyObjectByType<BattleAudio>();
        if (battleAudio && effectSounds.Count > 0) battleAudio.PlayClip(effectSounds[UnityEngine.Random.Range(0, effectSounds.Count)]);
    }
}