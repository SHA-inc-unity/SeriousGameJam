using UnityEngine;

[CreateAssetMenu(fileName = "DeflectEffect", menuName = "Battle/Effects/Deflect")]
public class DeflectEffect : WheelSlotEffect
{
    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle, SpinResult spinResult)
    {
        // Only deflect if the opponent actually landed a damaging effect this spin.
        WheelSlotEffect opponentEffect = spinResult.GetOpponentEffect(attacker, battle.GetPlayer());
        if (opponentEffect == null || opponentEffect is MissEffect || opponentEffect is DefendEffect
            || opponentEffect is DeflectEffect || opponentEffect is EvadeEffect)
        {
            battle.Announce($"{attacker.displayName} readies a deflect, but there's nothing to send back!");
            return;
        }

        BattleAudio battleAudio = FindAnyObjectByType<BattleAudio>();
        if (battleAudio && effectSounds.Count > 0)
            battleAudio.PlayClip(effectSounds[Random.Range(0, effectSounds.Count)]);

        battle.SetPendingDeflect(attacker);
        battle.Announce($"{attacker.displayName} deflects! The next hit gets sent back.");
    }
}