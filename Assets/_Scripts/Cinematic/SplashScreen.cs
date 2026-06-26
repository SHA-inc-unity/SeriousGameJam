using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] private Sprite image;
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField] private float displayDuration = 3f;

    void Start()
    {
        // Build a fullscreen canvas + image in code
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameObject.AddComponent<CanvasScaler>();

        GameObject imgGO = new GameObject("SplashImage");
        imgGO.transform.SetParent(transform, false);
        Image img = imgGO.AddComponent<Image>();
        img.sprite = image;

        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        StartCoroutine(SplashRoutine());
    }

    private IEnumerator SplashRoutine()
    {
        yield return new WaitForSeconds(displayDuration);
        FadeManager.Instance.FadeToScene(nextSceneName);
    }
}