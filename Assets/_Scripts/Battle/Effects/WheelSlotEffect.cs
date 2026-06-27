using System.Collections.Generic;
using UnityEngine;

// Base class for "what happens when a wheel lands on this slot".
// Every concrete effect (Attack, Heal, Defend, Miss, and anything custom
// you add later) inherits from this and implements its own Execute()
// with its own tunable fields.
//
// This is a ScriptableObject (not a plain class) so you can create effect
// assets in the Project window and drag them straight into a Wheel's slots,
// tuning values per-instance in the Inspector with zero code changes.
//
// To add a new effect type: create a new script that inherits from
// WheelSlotEffect, override Execute(), then right-click in the Project
// window -> Create -> Battle -> Effects -> [YourEffectName].
public abstract class WheelSlotEffect : ScriptableObject
{
    /// <summary>
    /// Runs this effect's logic. "attacker" is whoever owns the wheel that
    /// just spun (the one taking their turn); "defender" is the other combatant.
    /// "battle" gives access to Announce(), ApplyEffect(), EndBattleImmediately(),
    /// and GetPlayer()/GetEnemy() for anything fancier.
    /// </summary>
    ///
    [Tooltip("The wedge sprite shown on the wheel for this effect slot.")]
    public Sprite sliceSprite;

    [Header("Upgrade")]
    [Tooltip("The effect this slot upgrades into, if the player picks it on the post-battle " +
             "upgrade screen. Leave empty if this effect has no upgrade (e.g. Miss). " +
             "Same pattern as BombEffect.revealedEffect - just generalized to every effect.")]
    public WheelSlotEffect upgradedVersion;

    public List<AudioClip> effectSounds;

    public bool HasUpgrade => upgradedVersion != null;

    public abstract void Execute(Combatant attacker, Combatant defender, BattleManager battle);
}