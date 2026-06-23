using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BattleStateSound
{
    public BattleState state;
    public WheelSlotEffect effect;
    public AudioClip clip;
}

[CreateAssetMenu(fileName = "BattleSoundSet", menuName = "Battle/Battle Sound Set")]
public class BattleSoundSet : ScriptableObject
{
    [SerializeField] private List<BattleStateSound> sounds;
    [SerializeField] private AudioClip wheelSpinClip;

    public AudioClip GetClip(BattleState state)
    {
        foreach (var s in sounds)
        {
            if (s.state == state && s.state != BattleState.Null)
                return s.clip;
        }
        return null;
    }

    public AudioClip GetClip(WheelSlotEffect effect)
    {
        foreach (var s in sounds)
        {
            if (s.state == BattleState.Null && s.effect == effect)
                return s.clip;
        }
        return null;
    }

    public AudioClip GetWheelSpinClip()
    {
        return wheelSpinClip;
    }
}