using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCInteract : MonoBehaviour
{
    [SerializeField]
    private string npcName;
    [SerializeField]
    private DialogueHolder holder;

    public string NpcName { get => npcName; }

    public void Interact()
    {
        //Debug.Log("NPC was touched: " + npcName);

        //SceneManager.LoadScene("BattleScene");

        //DialogueSystem.Instance.StartDialogue(DialogueHolder.GetDialogue(npcName));
        DialogueSystem.Instance.StartDialogue(holder);
    }

}
