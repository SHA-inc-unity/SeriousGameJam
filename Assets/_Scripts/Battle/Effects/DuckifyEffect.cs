using UnityEngine;

[CreateAssetMenu(fileName = "DuckifyEffect", menuName = "Battle/Effects/Duckify")]
public class DuckifyEffect : WheelSlotEffect
{
    [Tooltip("The Duck effect asset to fill every defender slot with.")]
    public DuckEffect duckEffect;

    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle, SpinResult spinResult)
    {
        if (duckEffect == null)
        {
            Debug.LogWarning("DuckifyEffect has no duckEffect assigned.");
            return;
        }

        BattleAudio battleAudio = FindAnyObjectByType<BattleAudio>();
        if (battleAudio && effectSounds.Count > 0)
            battleAudio.PlayClip(effectSounds[UnityEngine.Random.Range(0, effectSounds.Count)]);

        battle.Announce($"{attacker.displayName} winds up a duckify — {defender.displayName}'s wheel changes next spin!");

        // Capture locals for the closure
        Combatant capturedDefender = defender;
        bool defenderIsPlayer = defender == battle.GetPlayer();
        Wheel originalWheel = defender.wheel;
        DuckEffect capturedDuckEffect = duckEffect;
        BattleCanvas canvas = battle.BattleCanvasRef;

        battle.QueueNextSpinEffect(() =>
        {
            battle.Announce($"{capturedDefender.displayName}'s wheel is now full of ducks!");
            battle.StatusEffects.TryApply(
                capturedDefender,
                new DuckedStatus(originalWheel, capturedDuckEffect, canvas, defenderIsPlayer));
        });
    }
}