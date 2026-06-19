using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCInteract : MonoBehaviour
{
    [SerializeField]
    private string npcName;

    public string NpcName { get => npcName; }

    public void Interact()
    {
        Debug.Log("NPC was touched: " + npcName);

        SceneManager.LoadScene("BattleScene");
    }
}
