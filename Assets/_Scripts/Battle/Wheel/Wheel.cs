using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWheel", menuName = "Battle/Wheel")]
public class Wheel : ScriptableObject
{
    [Serializable]
    public struct WheelSlot
    {
        [Tooltip("The effect that runs when the wheel lands on this slot.")]
        public WheelSlotEffect effect;
        [Min(0f)] public float weight;
        [Tooltip("The wedge sprite for this slot.")]
        public Sprite sliceSprite;
    }

    [Header("Visual")]
    [Tooltip("The base wheel background. Spins together with the slices.")]
    public Sprite backgroundSprite;

    public int slotCount => slots != null ? slots.Length : 0;

    [Tooltip("One entry per wedge. Should always have 6 entries.")]
    public WheelSlot[] slots;

    [Header("Timing")]
    [Tooltip("Seconds the owner must wait after this wheel finishes spinning before they can spin again.")]
    [Min(0f)]
    public float spinCooldown = 2f;

    public WheelSlotEffect Spin()
    {
        return SpinWithIndex(out _);
    }

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

        winningIndex = slots.Length - 1;
        return slots[winningIndex].effect;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (slots != null && slots.Length != slotCount)
            Debug.LogWarning($"Wheel '{name}': slotCount is {slotCount} but slots array has {slots.Length} entries.");

        if (slots != null)
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].effect == null)
                    Debug.LogWarning($"Wheel '{name}': slot {i} has no effect assigned.");
    }
#endif
}