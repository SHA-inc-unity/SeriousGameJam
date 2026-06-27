using UnityEngine;

[CreateAssetMenu(fileName = "EvadeEffect", menuName = "Battle/Effects/Evade")]
public class EvadeEffect : WheelSlotEffect
{
    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle, SpinResult spinResult)
    {
        // Only evade if the opponent actually landed a damaging effect this spin.
        WheelSlotEffect opponentEffect = spinResult.GetOpponentEffect(attacker, battle.GetPlayer());
        if (opponentEffect == null || opponentEffect is MissEffect || opponentEffect is DefendEffect
            || opponentEffect is DeflectEffect || opponentEffect is EvadeEffect)
        {
            battle.Announce($"{attacker.displayName} dodges, but there was nothing to dodge!");
            return;
        }

        BattleAudio battleAudio = FindAnyObjectByType<BattleAudio>();
        if (battleAudio && effectSounds.Count > 0)
            battleAudio.PlayClip(effectSounds[Random.Range(0, effectSounds.Count)]);

        // Apply evade status so ApplyDamage() consumes it when the attack resolves
        battle.StatusEffects.TryApply(attacker, new EvadeStatus());
        battle.Announce($"{attacker.displayName} evades the incoming attack!");
    }
}