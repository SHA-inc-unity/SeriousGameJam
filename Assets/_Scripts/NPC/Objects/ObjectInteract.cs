using UnityEngine;

public class ObjectInteract : MonoBehaviour
{
    [SerializeField]
    protected DialogueHolder holder;

    public virtual void Interact()
    {
        if (DialogueSystem.Instance == null)
        {
            Debug.LogError("DialogueSystem.Instance is null");
            return;
        }

        DialogueSystem.Instance.StartDialogue(holder);
    }
}