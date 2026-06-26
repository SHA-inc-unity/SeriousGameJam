using UnityEngine;

// Applied to a combatant when they land on the Evade slot.
// Completely negates the next instance of damage they would take, then expires.
public class EvadeStatus : StatusEffect
{
    public EvadeStatus()
    {
        displayName  = "Evading";
        durationType = StatusDurationType.Spins;
        durationSpins = 0; // never expires from spins - consumed by interception in ApplyDamage
    }

    public override void OnApply(Combatant owner, BattleManager battle)
    {
        battle.Announce($"{owner.displayName} is ready to evade the next attack!");
    }

    public override void OnExpire(Combatant owner, BattleManager battle)
    {
        battle.Announce($"{owner.displayName}'s evade wore off.");
    }
}