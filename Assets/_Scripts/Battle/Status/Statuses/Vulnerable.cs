// VulnerableStatus.cs
using UnityEngine;

public class VulnerableStatus : StatusEffect
{
    public VulnerableStatus()
    {
        displayName  = "Vulnerable";
        durationType = StatusDurationType.Spins; // consumed by hit, not time
        durationSpins = 999; // effectively permanent until consumed
    }
}