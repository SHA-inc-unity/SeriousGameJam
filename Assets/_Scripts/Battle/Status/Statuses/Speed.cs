// SpeedStatus.cs
using UnityEngine;

public class SpeedStatus : StatusEffect
{
    public float speedMultiplier;

    public SpeedStatus(float seconds, float speedMultiplier = 2f)
    {
        displayName     = "Speed";
        durationType    = StatusDurationType.Seconds;
        durationSeconds = seconds;
        this.speedMultiplier = speedMultiplier;
    }

    public override void OnApply(Combatant owner, BattleManager battle)
        => battle.MultiplySpinDuration(owner, 1f / speedMultiplier);

    public override void OnExpire(Combatant owner, BattleManager battle)
        => battle.MultiplySpinDuration(owner, speedMultiplier);
}