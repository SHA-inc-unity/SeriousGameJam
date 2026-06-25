using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    private ObjectInteract interactable;

    private void Update()
    {
        if (!IsActive.isActive || IsActive.dialogueCooldown) return;

        if (Keyboard.current.eKey.wasPressedThisFrame && interactable != null)
            interactable.Interact();
    }

    private void OnTriggerEnter(Collider other)
    {
        var target = other.GetComponent<ObjectInteract>();
        if (target != null)
            interactable = target;
    }

    private void OnTriggerExit(Collider other)
    {
        var target = other.GetComponent<ObjectInteract>();
        if (target != null && target == interactable)
            interactable = null;
    }
}