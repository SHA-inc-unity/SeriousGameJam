using UnityEngine;

public class ObjectInteract : MonoBehaviour
{
    [SerializeField]
    protected DialogueHolder holder;
    [SerializeField]
    protected DialogueHolder defeatDialogueHolder;
    [SerializeField]
    protected float forwardHelperDistance = 1.5f;

    public float ForwardHelperDistance { get => forwardHelperDistance; }

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