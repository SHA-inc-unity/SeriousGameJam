using System;
using UnityEngine;

// A "Wheel" is a fixed number of pie slices ("slots"), each holding a
// WheelSlotEffect (Attack, Heal, Defend, Miss, or any custom effect you make)
// plus a weight (not a percentage - weights are relative, so [3, 1] means
// the first slot is 3x more likely to be landed on than the second).
//
// slotCount controls how many wedges the wheel is divided into (e.g. 3, 6, 8).
// This matters for two things: (1) it's what you'll use to actually draw the
// wheel later (each slot = 360/slotCount degrees), and (2) it's a sanity check
// that the slots array below matches what you intend to display.
//
// Create new wheels via: right-click in Project window -> Create -> Battle -> Wheel
// This lets you make a "Starter Wheel", "Hook-a-Duck Wheel", "Lucky Wheel" etc.
// entirely in the Inspector - mixing and matching effect assets - with zero new code.
[CreateAssetMenu(fileName = "NewWheel", menuName = "Battle/Wheel")]
public class Wheel : ScriptableObject
{
    [Serializable]
    public struct WheelSlot
    {
        [Tooltip("The effect that runs when the wheel lands on this slot. " +
                 "Drag in an AttackEffect, HealEffect, DefendEffect, MissEffect, or any custom effect asset.")]
        public WheelSlotEffect effect;
        [Min(0f)] public float weight;
    }

    [Header("Visual")]
    [Tooltip("The actual wheel graphic shown in battle (the pie-chart image with " +
             "this wheel's slots drawn on it). Set dynamically onto WheelSpinUI " +
             "when the battle starts - see BattleManager.Start().")]
    public Sprite wheelSprite;

    [Tooltip("How many wedges this wheel is divided into (e.g. 3, 6, 8). " +
             "The 'slots' list below should have exactly this many entries.")]
    [Min(1)]
    public int slotCount = 6;

    [Tooltip("One entry per wedge on the wheel. Length should match slotCount.")]
    public WheelSlot[] slots;
    
    [Header("Timing")]
    [Tooltip("Seconds the owner must wait after this wheel finishes spinning before they can spin again.")]
    [Min(0f)]
    public float spinCooldown = 2f;

    /// <summary>
    /// Spins the wheel and returns the chosen slot's effect, based on weights.
    /// Returns null if the wheel has no valid slots (caller should handle this).
    /// </summary>
    public WheelSlotEffect Spin()
    {
        return SpinWithIndex(out _);
    }

    /// <summary>
    /// Same as Spin(), but also outputs which slot index won. Use this when
    /// you need to animate a wheel visual to a specific slot (see WheelSpinUI) -
    /// the result is decided here FIRST, then the animation is just made to
    /// visually match it.
    /// </summary>
    public WheelSlotEffect SpinWithIndex(out int winningIndex)
    {
        winningIndex = -1;

        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning($"Wheel '{name}' has no slots configured.");
            return null;
        }

        float totalWeight = 0f;
        foreach (var slot in slots)
            totalWeight += slot.weight;

        if (totalWeight <= 0f)
        {
            Debug.LogWarning($"Wheel '{name}' has zero total weight.");
            return null;
        }

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < slots.Length; i++)
        {
            cumulative += slots[i].weight;
            if (roll <= cumulative)
            {
                winningIndex = i;
                return slots[i].effect;
            }
        }

        // Fallback in case of floating point edge cases
        winningIndex = slots.Length - 1;
        return slots[winningIndex].effect;
    }

#if UNITY_EDITOR
    // Editor-only sanity checks: catches mismatches early instead of finding
    // out at runtime that a slot was left empty or the count doesn't match.
    private void OnValidate()
    {
        if (slots != null && slots.Length != slotCount)
        {
            Debug.LogWarning($"Wheel '{name}': slotCount is {slotCount} but slots array has {slots.Length} entries. " +
                              $"These should match.");
        }

        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].effect == null)
                    Debug.LogWarning($"Wheel '{name}': slot {i} has no effect assigned.");
            }
        }
    }
#endif
}