using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCInteract : MonoBehaviour
{
    [SerializeField]
    private string npcName;
    [SerializeField]
    private DialogueHolder holder;

    public string sceneName;

    public string NpcName { get => npcName; }

    public void Interact()
    {
        //Debug.Log("NPC was touched: " + npcName);

        DialogueSystem.Instance.StartDialogue(holder);
    }

}