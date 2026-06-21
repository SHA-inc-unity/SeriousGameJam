using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    private NPCInteract npcInteract;

    private void Update()
    {
        if (!IsActive.isActive || IsActive.dialogueCooldown) return;

        if (Keyboard.current.eKey.wasPressedThisFrame && npcInteract != null)
        {
            npcInteract.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var npc = other.GetComponent<NPCInteract>();
        if (npc != null)
        {
            npcInteract = npc;
            Debug.Log("Interacted with NPC: " + npc.NpcName);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var npc = other.GetComponent<NPCInteract>();
        if (npc != null && npc == npcInteract)
        {
            npcInteract = null;
            Debug.Log("Stopped interacting with NPC: " + npc.NpcName);
        }
    }
}
