// StunnedStatus.cs
using UnityEngine;

public class StunnedStatus : StatusEffect
{
    public StunnedStatus(float seconds)
    {
        displayName    = "Stunned";
        durationType   = StatusDurationType.Seconds;
        durationSeconds = seconds;
    }

    public override void OnApply(Combatant owner, BattleManager battle)
        => battle.SetStunned(owner, true);

    public override void OnExpire(Combatant owner, BattleManager battle)
        => battle.SetStunned(owner, false);
}