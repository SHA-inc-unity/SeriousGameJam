using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRandomEffect", menuName = "Battle/Effects/RandomEffect")]
public class RandomEffect : WheelSlotEffect
{
    [Serializable]
    public struct WeightedEffect
    {
        public WheelSlotEffect effect;
        [Min(0f)] public float weight;
    }

    [Tooltip("Effects to randomly pick from, each with its own weight.")]
    public WeightedEffect[] effects;

    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle)
    {
        if (effects == null || effects.Length == 0)
        {
            Debug.LogWarning($"RandomEffect '{name}': no effects configured.");
            return;
        }

        float totalWeight = 0f;
        foreach (var e in effects)
            totalWeight += e.weight;

        if (totalWeight <= 0f)
        {
            Debug.LogWarning($"RandomEffect '{name}': total weight is zero.");
            return;
        }

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var e in effects)
        {
            cumulative += e.weight;
            if (roll <= cumulative)
            {
                e.effect?.Execute(attacker, defender, battle);
                return;
            }
        }

        // Fallback (floating point edge case)
        effects[effects.Length - 1].effect?.Execute(attacker, defender, battle);
    }
}