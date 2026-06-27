using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "Battle/Effects/Heal")]
public class HealEffect : WheelSlotEffect
{
    [Tooltip("Flat HP restored.")]
    public int healAmount = 1;

    public override void Execute(Combatant attacker, Combatant defender, BattleManager battleManager)
    {
        attacker.Heal(healAmount);
        battleManager.NotifyHPChanged(attacker);
        battleManager.Announce($"{attacker.displayName} heals for {healAmount}! " +
                               $"HP: {attacker.currentHP}/{attacker.maxHP}");

        BattleAudio battleAudio = FindAnyObjectByType<BattleAudio>();
        if (battleAudio && effectSounds.Count > 0) battleAudio.PlayClip(effectSounds[UnityEngine.Random.Range(0, effectSounds.Count)]);
    }
}