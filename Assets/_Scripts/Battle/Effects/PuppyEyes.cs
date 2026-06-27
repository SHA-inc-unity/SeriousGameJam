using UnityEngine;

[CreateAssetMenu(fileName = "VulnerableSlot", menuName = "Battle/Effects/Vulnerable")]
public class VulnerableSlotEffect : WheelSlotEffect
{
    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle, SpinResult spinResult)
    {
        // Vulnerable is self-applied and only relevant if the opponent is attacking this spin.
        WheelSlotEffect opponentEffect = spinResult.GetOpponentEffect(attacker, battle.GetPlayer());
        if (opponentEffect == null || opponentEffect is MissEffect || opponentEffect is DefendEffect
            || opponentEffect is DeflectEffect || opponentEffect is EvadeEffect)
        {
            battle.Announce($"{attacker.displayName} is exposed, but the opponent isn't attacking!");
            return;
        }

        BattleAudio battleAudio = FindAnyObjectByType<BattleAudio>();
        if (battleAudio && effectSounds.Count > 0)
            battleAudio.PlayClip(effectSounds[Random.Range(0, effectSounds.Count)]);

        battle.StatusEffects.TryApply(attacker, new VulnerableStatus());
        battle.Announce($"{attacker.displayName} is Vulnerable this spin! Incoming hit deals double damage.");
    }
}