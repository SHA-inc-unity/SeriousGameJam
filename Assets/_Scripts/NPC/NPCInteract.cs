using System.Collections.Generic;
using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    [SerializeField]
    private string npcName;
    [SerializeField]
    private DialogueHolder holder;

    // What is it and why is it public
    [SerializeField]
    private bool isDefeated;
    [SerializeField]
    private GameObject NPCBooth;
    [SerializeField]
    private GameObject NPC;
    [SerializeField]
    private List<Sprite> ObjectStates;

    private BattleTrigger battleTrigger;

    public string NpcName { get => npcName; }

    private void Start()
    {
        if (GetComponent<BattleTrigger>())
            battleTrigger = GetComponent<BattleTrigger>();

        if (NPCBooth != null && ObjectStates != null && ObjectStates.Count > 0)
            NPCBooth.GetComponent<SpriteRenderer>().sprite = ObjectStates[0];
    }

    public void Interact()
    {
        if (DialogueSystem.Instance == null)
        {
            Debug.LogError("DialogueSystem.Instance is null - is the DialogueSystem GameObject in this scene and active?");
            return;
        }

        if (battleTrigger)
            DialogueSystem.Instance.StartDialogue(holder, battleTrigger);
        else
            DialogueSystem.Instance.StartDialogue(holder);
    }

    private void Update()
    {
        if (isDefeated && NPCBooth != null && ObjectStates != null && ObjectStates.Count > 1)
        {
            NPCBooth.GetComponent<SpriteRenderer>().sprite = ObjectStates[1];
            if (NPC != null)
                NPC.GetComponent<Animator>().SetBool("hasDefeated", true);
        }
    }
}