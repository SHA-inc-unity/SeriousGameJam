using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueStarter : MonoBehaviour
{
    [SerializeField]
    private DialogueHolder dialogue;

    public void StartDialogue()
    {
        DialogueSystem.Instance.StartDialogue(dialogue);
    }
}