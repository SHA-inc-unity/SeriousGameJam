using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    private ObjectInteract interactable;

    private ObjectInteract Interactable { get => interactable; set { interactable = value; InteractHelper.Instance.SetObj(value); } }

    private void Update()
    {
        if (!IsActive.isActive || IsActive.dialogueCooldown) return;

        if (Keyboard.current.eKey.wasPressedThisFrame && Interactable != null)
            Interactable.Interact();
    }

    private void OnTriggerEnter(Collider other)
    {
        var target = other.GetComponent<ObjectInteract>();
        if (target != null)
            Interactable = target;
    }

    private void OnTriggerExit(Collider other)
    {
        var target = other.GetComponent<ObjectInteract>();
        if (target != null && target == Interactable)
            Interactable = null;
    }
}