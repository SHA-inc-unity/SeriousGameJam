using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [SerializeField] private float fadeDuration = 0.5f;

    private Image fadeImage;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build the canvas and image in code so you don't need a prefab
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        GameObject imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(transform, false);
        fadeImage = imgGO.AddComponent<Image>();
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = false;

        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        SetAlpha(0f);
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeRoutine(sceneName));
    }

    private IEnumerator FadeRoutine(string sceneName)
    {
        // Fade to black
        yield return StartCoroutine(Fade(0f, 1f));

        // Load scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);

        // Wait a frame for the scene to load
        yield return null;

        // Fade back in
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}