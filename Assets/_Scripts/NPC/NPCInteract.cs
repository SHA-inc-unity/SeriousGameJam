using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    [SerializeField]
    private string npcName;
    [SerializeField]
    private DialogueHolder holder;
    private BattleTrigger battleTrigger;

    public string sceneName;

    public string NpcName { get => npcName; }

    private void Start()
    {
        if (GetComponent<BattleTrigger>())
        {
            battleTrigger = GetComponent<BattleTrigger>();
        }
    }

    public void Interact()
    {
        //Debug.Log("NPC was touched: " + npcName);

        if (DialogueSystem.Instance == null)
        {
            Debug.LogError("DialogueSystem.Instance is null - is the DialogueSystem GameObject in this scene and active?");
            return;
        }

        if (battleTrigger)
        {
            DialogueSystem.Instance.StartDialogue(holder, battleTrigger);
        }
        else
        {
            DialogueSystem.Instance.StartDialogue(holder);
        }
        
    }

}