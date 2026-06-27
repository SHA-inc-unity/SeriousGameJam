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
        if (battleAudio && effectSounds.Count > 0) battleAudio.PlayClip(effectSounds[UnityEngine.Random.Range(0, effectSounds.Count)]);

        // Replace this slot in the wheel with the revealed effect
        Wheel wheel = attacker.wheel;
        for (int i = 0; i < wheel.slots.Length; i++)
        {
            if (wheel.slots[i].effect == this)
            {
                wheel.slots[i] = new Wheel.WheelSlot
                {
                    effect = revealedEffect,
                    weight = wheel.slots[i].weight  // keep the same weight
                };
                battle.Announce($"The bomb reveals a new slot: {(revealedEffect != null ? revealedEffect.name : "nothing")}!");
                break;
            }
        }

        // Check if the self-damage finished the attacker
        if (attacker.IsDefeated)
            battle.EndBattleImmediately(playerWon: attacker == battle.GetEnemy());
    }
}