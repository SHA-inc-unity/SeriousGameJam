using UnityEngine;
using UnityEngine.UI;

public class HPDisplay : MonoBehaviour
{
    [Header("Layout")]
    [Tooltip("Diameter of each HP light icon.")]
    public float lightSize = 30f;

    [Tooltip("Vertical gap between lights (from centre to centre).")]
    public float verticalSpacing = 40f;

    [Header("Hero Sprites")]
    [SerializeField] private Sprite heroOnSprite;
    [SerializeField] private Sprite heroOffSprite;
    [SerializeField] private Sprite heroOverhealthSprite;

    [Header("Enemy Sprites")]
    [SerializeField] private Sprite enemyOnSprite;
    [SerializeField] private Sprite enemyOffSprite;
    [SerializeField] private Sprite enemyOverhealthSprite;

    // ── internals ──────────────────────────────────────────────────────────────
    private Image[] lights;
    private bool isHeroDisplay;

    // ── public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Spawns <paramref name="slotCount"/> lights (max 4) stacked top-to-bottom
    /// under <paramref name="parent"/>.
    /// </summary>
    public void Init(int slotCount, Transform parent, bool isHero)
    {
        isHeroDisplay = isHero;
        transform.SetParent(parent, worldPositionStays: false);

        slotCount = Mathf.Clamp(slotCount, 1, 4);
        lights = new Image[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            GameObject go = new GameObject($"Light_{i}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, worldPositionStays: false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(lightSize, lightSize);
            // Slot 0 is at the top; each subsequent slot moves down.
            rt.anchoredPosition = new Vector2(0f, -i * verticalSpacing);

            Image img = go.GetComponent<Image>();
            img.sprite = isHero ? heroOnSprite : enemyOnSprite;
            lights[i] = img;
        }
    }

    /// <summary>Normal HP update (no overhealth).</summary>
    public void UpdateHP(int currentHP)
    {
        Sprite on  = isHeroDisplay ? heroOnSprite  : enemyOnSprite;
        Sprite off = isHeroDisplay ? heroOffSprite : enemyOffSprite;

        for (int i = 0; i < lights.Length; i++)
            lights[i].sprite = i < currentHP ? on : off;
    }

    /// <summary>
    /// HP update with overhealth. Overhealth slots fill from the top (index 0),
    /// then normal HP, then empty.
    /// e.g. slotCount=4, currentHP=2, overhealth=1 → [overhealth, on, on, off]
    /// </summary>
    public void UpdateHPWithOverhealth(int currentHP, int overhealth)
    {
        Sprite on         = isHeroDisplay ? heroOnSprite         : enemyOnSprite;
        Sprite off        = isHeroDisplay ? heroOffSprite        : enemyOffSprite;
        Sprite oversprite = isHeroDisplay ? heroOverhealthSprite : enemyOverhealthSprite;

        for (int i = 0; i < lights.Length; i++)
        {
            if      (i < overhealth)                lights[i].sprite = oversprite;
            else if (i < overhealth + currentHP)    lights[i].sprite = on;
            else                                    lights[i].sprite = off;
        }
    }
}