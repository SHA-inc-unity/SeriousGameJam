using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackManager : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private DialogueHolder dialogueHolder;

    public bool isFading;

    private DialogueSystem dialogueSystem;
    private Image fadeImage;

    public IEnumerator FadeAndTeleport(PlayerMove player)
    {
        isFading = true;

        yield return StartCoroutine(Fade(0f, 1f));

        player.BackToStart();
        yield return new WaitForSeconds(0.1f);
        if (dialogueSystem == null) dialogueSystem = FindAnyObjectByType<DialogueSystem>();
        dialogueSystem.StartDialogue(dialogueHolder);

        yield return StartCoroutine(Fade(1f, 0f));

        isFading = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        if (fadeImage == null)
        {
            var x = FindAnyObjectByType<BackImage>();
            fadeImage = x.gameObject.GetComponent<Image>();
        }
        Color c = fadeImage.color;

        c.a = from;
        fadeImage.color = c;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }
}