using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    private string nextSceneName;
    [SerializeField]
    private Transform tpPos;
    [SerializeField]
    private List<NPCInteract> doorKeepers;

    private bool isOpen = false;

    public bool IsOpen { get => isOpen; }

    private void OnTriggerStay(Collider other)
    {
        if (!IsOpen)return;
        if (IsActive.ExitDoorCooldown) return;

        if (other.CompareTag("Player"))
        {
            IsActive.StartExitDoorCooldown();
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

    private void Awake()
    {
        if (IsActive.ExitDoorCooldown && PlayerPrefs.GetString("LastScene") == nextSceneName)
        {
            // Fix it later
            FindAnyObjectByType<PlayerMove>().EnterTheDoor(tpPos.position);
        }
    }

    private void Update()
    {
        if(IsOpen) return;

        int x = 0;
        foreach (NPCInteract keeper in doorKeepers)
        {
            if (!keeper.isDefeated)
            {
                x++;
            }
        }

        if(x == 0)
        {
            isOpen = true;
        }
    }
}
