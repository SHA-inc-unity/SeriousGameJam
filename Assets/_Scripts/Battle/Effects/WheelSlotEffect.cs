using System.Collections.Generic;
using UnityEngine;

public abstract class WheelSlotEffect : ScriptableObject
{
    [Tooltip("The wedge sprite shown on the wheel for this effect slot.")]
    [SerializeField] private Sprite sliceSprite;
    [SerializeField] private float sliceSpriteScale = 1f;

    [Header("Resolution Order")]
    [Tooltip("When both wheels land simultaneously, higher priority resolves first. " +
             "Player wins ties. Defensive effects (e.g. Shield) should be higher than " +
             "offensive ones (e.g. Attack) so shields can go up before damage lands.")]
    public int priority = 0;

    [Header("Upgrade")]
    [Tooltip("The effect this slot upgrades into on the post-battle upgrade screen. " +
             "Leave empty if this effect has no upgrade (e.g. Miss).")]
    public WheelSlotEffect upgradedVersion;

    public List<AudioClip> effectSounds;

    public bool HasUpgrade => upgradedVersion != null;
    public (float, Sprite) SliceSprite => (sliceSpriteScale, sliceSprite);

    /// <summary>
    /// Executes the effect. spinResult contains both effects rolled this spin —
    /// defensive effects use it to react to what the opponent landed right now,
    /// with no persistent state required.
    /// </summary>
    public abstract void Execute(Combatant attacker, Combatant defender, BattleManager battle, SpinResult spinResult);
}