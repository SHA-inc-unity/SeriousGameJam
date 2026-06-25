// FrozenStatus.cs
using UnityEngine;

public class FrozenStatus : StatusEffect
{
    public float slowMultiplier;

    public FrozenStatus(float seconds, float slowMultiplier = 2f)
    {
        displayName     = "Frozen";
        durationType    = StatusDurationType.Seconds;
        durationSeconds = seconds;
        this.slowMultiplier = slowMultiplier;
    }

    public override void OnApply(Combatant owner, BattleManager battle)
        => battle.MultiplySpinDuration(owner, slowMultiplier);

    public override void OnExpire(Combatant owner, BattleManager battle)
        => battle.MultiplySpinDuration(owner, 1f / slowMultiplier);
}