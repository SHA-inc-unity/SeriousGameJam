using UnityEngine;

[CreateAssetMenu(fileName = "BombEffect", menuName = "Battle/Effects/Bomb")]
public class BombEffect : WheelSlotEffect
{
    [Tooltip("Damage dealt to the spinner when the bomb explodes.")]
    public int selfDamage = 1;

    [Tooltip("The effect that permanently replaces this bomb slot after it explodes.")]
    public WheelSlotEffect revealedEffect;

    public override void Execute(Combatant attacker, Combatant defender, BattleManager battle, SpinResult spinResult)
    {
        // Self-damage
        attacker.TakeDamage(selfDamage);
        battle.NotifyHPChanged(attacker);
        battle.Announce($"{attacker.displayName} triggered a bomb! Takes {selfDamage} self-damage. " +
                        $"HP: {attacker.currentHP}/{attacker.maxHP}");

        BattleAudio battleAudio = FindAnyObjectByType<BattleAudio>();
        if (battleAudio && effectSounds.Count > 0)
            battleAudio.PlayClip(effectSounds[UnityEngine.Random.Range(0, effectSounds.Count)]);

        // Use the exact slot index the wheel landed on, not a search by effect reference.
        // This ensures the correct slot is replaced even when multiple bomb slots exist.
        int landedIndex = (attacker == battle.GetPlayer())
            ? spinResult.playerSlotIndex
            : spinResult.enemySlotIndex;

        Wheel wheel = attacker.wheel;
        if (landedIndex >= 0 && landedIndex < wheel.slots.Length)
        {
            wheel.slots[landedIndex] = new Wheel.WheelSlot
            {
                effect = revealedEffect,
                weight = wheel.slots[landedIndex].weight
            };
            battle.Announce($"The bomb reveals a new slot: {(revealedEffect != null ? revealedEffect.name : "nothing")}!");
        }
        else
        {
            Debug.LogWarning($"BombEffect: landedIndex {landedIndex} is out of range for wheel '{wheel.name}'.");
        }

        // Refresh the wheel visuals so the new sprite shows immediately
        battle.NotifyWheelChanged(attacker);

        // Check if the self-damage finished the attacker
        if (attacker.IsDefeated)
            battle.EndBattleImmediately(playerWon: attacker == battle.GetEnemy());
    }
}