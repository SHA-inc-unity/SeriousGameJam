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

[Serializable]
public struct BattleDialogueLine
{
    public string who;
    public string text;

    [SerializeField]
    public BattleState nextBattleState;
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