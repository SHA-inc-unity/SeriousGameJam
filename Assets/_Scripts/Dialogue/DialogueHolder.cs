// DialogueHolder.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DialogueAnswer
{
    public string text;
    public DialogueHolder nextDialogue;
    public bool goToBattle;
}

[Serializable]
public struct DialogueLine
{
    public string who;
    public string text;
    public AudioClip voice;
    public List<DialogueAnswer> answers;
}

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/DialogueHolder")]
public class DialogueHolder : ScriptableObject
{
    [SerializeField]
    private List<DialogueLine> dialogue;

    [Tooltip("If true, a battle is triggered as soon as this dialogue ends.")]
    public bool triggerBattleOnEnd;

    [Tooltip("If false, this dialogue will only play once and be skipped on subsequent attempts.")]
    public bool canRepeat = true;

    // Runtime-only flag, resets on play. Make it persistent across scenes
    // by saving to PlayerPrefs if you need it to survive session reloads.
    [NonSerialized]
    public bool hasPlayed = false;

    public List<DialogueLine> GetDialogueLines() => dialogue;
}