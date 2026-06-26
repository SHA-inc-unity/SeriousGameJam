using UnityEngine;

/// <summary>
/// Applies a chosen upgrade permanently to a CombatantData asset's wheel - the same
/// in-place slot replacement BombEffect already does on a runtime wheel clone, just
/// targeting the source asset so the change survives into the next battle.
/// </summary>
public static class WheelUpgradeManager
{
    /// <summary>
    /// Replaces the effect in playerData.wheel.slots[slotIndex] with that effect's
    /// upgradedVersion, keeping the slot's existing weight. No-op (with a warning) if the
    /// slot has no upgrade or the index is out of range.
    /// </summary>
    public static bool ApplyUpgrade(CombatantData playerData, int slotIndex)
    {
        if (playerData == null || playerData.wheel == null)
        {
            Debug.LogWarning("WheelUpgradeManager: no CombatantData/wheel to upgrade.");
            return false;
        }

        Wheel wheel = playerData.wheel;

        if (slotIndex < 0 || slotIndex >= wheel.slots.Length)
        {
            Debug.LogWarning($"WheelUpgradeManager: slot index {slotIndex} out of range.");
            return false;
        }

        WheelSlotEffect current = wheel.slots[slotIndex].effect;
        if (current == null || !current.HasUpgrade)
        {
            Debug.LogWarning($"WheelUpgradeManager: slot {slotIndex} has no upgrade available.");
            return false;
        }

        wheel.slots[slotIndex] = new Wheel.WheelSlot
        {
            effect      = current.upgradedVersion,
            weight      = wheel.slots[slotIndex].weight,      // keep existing odds
            sliceSprite = wheel.slots[slotIndex].sliceSprite   // wedge background slice unchanged;
            // the icon itself comes from effect.sliceSprite
        };

        Debug.Log($"{playerData.displayName}'s slot {slotIndex} upgraded: " +
                  $"{current.name} -> {current.upgradedVersion.name}");
        return true;
    }
}