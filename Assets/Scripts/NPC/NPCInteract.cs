using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCInteract : MonoBehaviour
{
    [SerializeField]
    private string npcName;

    public string sceneName;

    public bool isDefeated;
    public GameObject NPCBooth;
    public GameObject NPC;
    public List<Sprite> ObjectStates;

    public string NpcName { get => npcName; }

    private void Start()
    {
        NPCBooth.GetComponent<SpriteRenderer>().sprite = ObjectStates[0];
    }

    public void Interact()
    {
        Debug.Log("NPC was touched: " + npcName);

        SceneManager.LoadScene(sceneName);
    }

    private void Update()
    {
        if (isDefeated == true)
        {
            NPCBooth.GetComponent<SpriteRenderer>().sprite = ObjectStates[1];
            NPC.GetComponent<Animator>().SetBool("hasDefeated", true);

        }
    }

}
