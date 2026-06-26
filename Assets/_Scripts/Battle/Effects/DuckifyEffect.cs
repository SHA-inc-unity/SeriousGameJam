using UnityEngine;

[CreateAssetMenu(fileName = "DuckifyEffect", menuName = "Battle/Effects/Duckify")]
public class DuckifyEffect : WheelSlotEffect
{
    [Tooltip("The Duck effect asset to fill every defender slot with.")]
    public DuckEffect duckEffect;

    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle)
    {
        if (duckEffect == null)
        {
            Debug.LogWarning("DuckifyEffect has no duckEffect assigned.");
            return;
        }

        battle.Announce($"{attacker.displayName} turns {defender.displayName}'s wheel into ducks!");

        bool defenderIsPlayer = defender == battle.GetPlayer();
        battle.StatusEffects.TryApply(
            defender,
            new DuckedStatus(defender.wheel, duckEffect, battle.BattleCanvasRef, defenderIsPlayer));
    }
}