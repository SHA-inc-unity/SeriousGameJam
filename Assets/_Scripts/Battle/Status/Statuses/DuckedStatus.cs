using UnityEngine;

// Applied to a combatant's WHEEL to temporarily replace every slot's effect with
// DuckEffect. Lasts exactly one spin, then restores the original slots (data and
// visuals together).
//
// durationSpins = 1 works correctly because BattleManager reads
// wheel.slots[confirmedIndex].effect BEFORE calling NotifySpinCompleted (see
// PlayerSpin/EnemyLoop). The duck effect is captured first, then the status ticks
// and expires, restoring the wheel - all in the right order.
public class DuckedStatus : StatusEffect
{
    private readonly Wheel.WheelSlot[] originalSlots;
    private readonly DuckEffect duckEffect;
    private readonly BattleCanvas canvas;
    private readonly bool ownerIsPlayer;

    public DuckedStatus(Wheel targetWheel, DuckEffect duckEffect, BattleCanvas canvas, bool ownerIsPlayer)
    {
        displayName   = "Ducked";
        durationType  = StatusDurationType.Spins;
        durationSpins = 1;
        this.duckEffect = duckEffect;
        this.canvas = canvas;
        this.ownerIsPlayer = ownerIsPlayer;

        // Deep copy: WheelSlot is a struct, so this clones each entry by value.
        // Cloning the array reference alone would NOT protect against mutation,
        // since OnApply below writes into targetWheel.slots in place.
        originalSlots = new Wheel.WheelSlot[targetWheel.slots.Length];
        System.Array.Copy(targetWheel.slots, originalSlots, targetWheel.slots.Length);
    }

    public override void OnApply(Combatant owner, BattleManager battle)
    {
        Wheel wheel = owner.wheel;

        var (duckScale, duckSprite) = duckEffect.SliceSprite;

        for (int i = 0; i < wheel.slots.Length; i++)
        {
            wheel.slots[i] = new Wheel.WheelSlot
            {
                effect = duckEffect,
                weight = wheel.slots[i].weight,
                sliceSprite = duckSprite
            };
        }

        RefreshVisual(wheel);
    }

    public override void OnExpire(Combatant owner, BattleManager battle)
    {
        Wheel wheel = owner.wheel;
        for (int i = 0; i < wheel.slots.Length && i < originalSlots.Length; i++)
            wheel.slots[i] = originalSlots[i];

        RefreshVisual(wheel);
    }

    private void RefreshVisual(Wheel wheel)
    {
        if (canvas == null) return;

        if (ownerIsPlayer) canvas.RefreshPlayerWheelSprites(wheel);
        else canvas.RefreshEnemyWheelSprites(wheel);
    }
}