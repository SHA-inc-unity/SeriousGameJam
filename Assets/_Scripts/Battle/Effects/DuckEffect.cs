using UnityEngine;

// A completely inert effect. Used to temporarily skin every slot on a
// wheel during DuckedStatus - landing on it does nothing at all.
[CreateAssetMenu(fileName = "DuckEffect", menuName = "Battle/Effects/Duck")]
public class DuckEffect : WheelSlotEffect
{
    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle, SpinResult spinResult)
    {
        battle.Announce($"{attacker.displayName}'s wheel lands on a duck. ...Nothing happens.");
        BattleAudio battleAudio = FindAnyObjectByType<BattleAudio>();
        if (battleAudio && effectSounds.Count > 0) battleAudio.PlayClip(effectSounds[UnityEngine.Random.Range(0, effectSounds.Count)]);
    }
}