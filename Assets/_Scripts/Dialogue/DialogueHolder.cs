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

    public List<DialogueLine> GetDialogueLines()
    {
        return dialogue;
    }
}