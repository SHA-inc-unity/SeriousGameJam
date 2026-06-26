using UnityEngine;

public enum StatusDurationType { Seconds, Spins }

public abstract class StatusEffect
{
    public string displayName;
    public StatusDurationType durationType;

    // For seconds-based
    public float durationSeconds;

    // For spins-based
    public int durationSpins;
    public int spinsRemaining;

    // Called once when the status is first applied
    public virtual void OnApply(Combatant owner, BattleManager battle) { }

    // Called once when the status expires or is removed
    public virtual void OnExpire(Combatant owner, BattleManager battle) { }

    // Called each time the owner completes a spin (spins-based statuses tick here)
    public virtual void OnSpinCompleted(Combatant owner, BattleManager battle) { }
}