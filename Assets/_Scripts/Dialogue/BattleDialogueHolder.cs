using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BattleDialogueLine
{
    public string who;
    public string text;

    public AudioClip voice;

    [Header("Chouse Only one or null")]
    [SerializeField]
    public BattleState nextBattleState;
    [SerializeField]
    public WheelSlotEffect nextWheelEffect;
}

[CreateAssetMenu(fileName = "BattleDialogue", menuName = "Dialogue/BattleDialogueHolder")]
public class BattleDialogueHolder : ScriptableObject
{
    [SerializeField]
    private List<BattleDialogueLine> dialogue;

    public List<BattleDialogueLine> GetDialogueLines()
    {
        return dialogue;
    }
}