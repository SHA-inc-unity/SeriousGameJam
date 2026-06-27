using UnityEngine;

[CreateAssetMenu(fileName = "DefendEffect", menuName = "Battle/Effects/Defend")]
public class DefendEffect : WheelSlotEffect
{
    public int damageReduction = 1;

    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle, SpinResult spinResult)
    {
        // Only block if the opponent actually landed a damaging effect this spin.
        // No persistent state — if they didn't attack, defend does nothing.
        WheelSlotEffect opponentEffect = spinResult.GetOpponentEffect(attacker, battle.GetPlayer());
        if (opponentEffect == null || opponentEffect is MissEffect || opponentEffect is DefendEffect
            || opponentEffect is DeflectEffect || opponentEffect is EvadeEffect)
        {
            battle.Announce($"{attacker.displayName} braces, but there's nothing to block!");
            return;
        }

        BattleAudio battleAudio = FindAnyObjectByType<BattleAudio>();
        if (battleAudio && effectSounds.Count > 0)
            battleAudio.PlayClip(effectSounds[Random.Range(0, effectSounds.Count)]);

        battle.SetPendingDamageReduction(attacker, damageReduction);
        battle.Announce($"{attacker.displayName} braces! Incoming hit reduced by {damageReduction}.");
    }
}