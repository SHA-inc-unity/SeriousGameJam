using System.Collections;
using TMPro;
using UnityEngine;

public abstract class DialogueSystemBase : MonoBehaviour
{
    [SerializeField] protected string heroNameStr;

    protected Coroutine typingRoutine;
    protected string currentText;
    protected AudioClip currentClip;
    private TMP_Text activeTextField;

    protected void BeginLine(TMP_Text target, string lineText, AudioClip lineClip)
    {
        activeTextField = target;
        currentText = lineText;
        currentClip = lineClip;

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeText());
    }

    protected IEnumerator TypeText()
    {
        activeTextField.text = "";

        float delay = charDelayValue;

        if (currentClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDialogueClip(currentClip);
            if (currentText.Length > 0)
                delay = currentClip.length / currentText.Length;
        }

        foreach (char c in currentText)
        {
            activeTextField.text += c;
            yield return new WaitForSeconds(delay);
        }
        typingRoutine = null;
    }

    protected void SkipTyping()
    {
        StopCoroutine(typingRoutine);
        typingRoutine = null;
        activeTextField.text = currentText;

        if (AudioManager.Instance != null)
            AudioManager.Instance.StopDialogueClip();
    }

    protected abstract float charDelayValue { get; }
}