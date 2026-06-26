// RiggedStatus.cs
using UnityEngine;

public class RiggedStatus : StatusEffect
{
    public int riggedSlotIndex;

    public RiggedStatus(int slotIndex, int spins)
    {
        displayName   = "Rigged";
        durationType  = StatusDurationType.Spins;
        durationSpins = spins;
        riggedSlotIndex = slotIndex;
    }
}