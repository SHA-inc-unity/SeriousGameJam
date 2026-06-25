using System;
using UnityEngine;

public class DialogueStarter : MonoBehaviour
{
    [SerializeField]
    private DialogueHolder dialogue;


    private void Start()
    {
        StartDialogue();
    }

    public void StartDialogue()
    {
        DialogueSystem.Instance.StartDialogue(dialogue);
    }
}