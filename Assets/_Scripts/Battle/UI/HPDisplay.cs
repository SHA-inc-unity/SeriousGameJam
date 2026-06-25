using UnityEngine;
using UnityEngine.UI;

public class HPDisplay : MonoBehaviour
{
    [Header("Layout")]
    [Tooltip("Total horizontal space the widest row of lights is allowed to occupy. " +
             "Spacing between lights is derived from this, not set directly.")]
    public float totalWidth = 200f;

    [Tooltip("Vertical gap between the two rows, for layouts that use 2 rows. Manual, not derived.")]
    public float verticalSpacing = 50f;

    public float lightSize = 30f;

    [Tooltip("Multiplier applied to lightSize for 2-row layouts, so rows fit more comfortably.")]
    public float twoRowSizeMultiplier = 0.8f;

    [Header("Hero Sprites")]
    [SerializeField] private Sprite heroOnSprite;
    [SerializeField] private Sprite heroOffSprite;
    [SerializeField] private Sprite heroOverhealthSprite;

    [Header("Enemy Sprites")]
    [SerializeField] private Sprite enemyOnSprite;
    [SerializeField] private Sprite enemyOffSprite;
    [SerializeField] private Sprite enemyOverhealthSprite;

    private Image[] lights;
    private bool isHeroDisplay;
    private float currentLightSize; // set per-layout by GetPositions; full size for single row, shrunk for 2-row

    public void Init(int maxHP, Transform parent, bool isHero)
    {
        isHeroDisplay = isHero;
        transform.SetParent(parent, worldPositionStays: false);
        lights = new Image[maxHP];
        currentLightSize = lightSize; // GetPositions may override this before returning
        Vector2[] positions = GetPositions(maxHP);

        for (int i = 0; i < maxHP; i++)
        {
            GameObject go = new GameObject($"Light_{i}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, worldPositionStays: false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta        = new Vector2(currentLightSize, currentLightSize);
            rt.anchoredPosition = positions[i];

            Image img = go.GetComponent<Image>();
            img.sprite = isHero ? heroOnSprite : enemyOnSprite; // default "on" state
            lights[i] = img;
        }
    }

    // Normal HP update, no overhealth
    public void UpdateHP(int currentHP)
    {
        Sprite onSprite  = isHeroDisplay ? heroOnSprite  : enemyOnSprite;
        Sprite offSprite = isHeroDisplay ? heroOffSprite : enemyOffSprite;

        for (int i = 0; i < lights.Length; i++)
            lights[i].sprite = i < currentHP ? onSprite : offSprite;
    }

    // Call this when overhealth is involved.
    // overhealth slots use the overhealth sprite, starting from the leftmost light.
    // e.g. maxHP=3, currentHP=2, overhealth=1 -> [overhealth, on, off]
    public void UpdateHPWithOverhealth(int currentHP, int overhealth)
    {
        Sprite onSprite         = isHeroDisplay ? heroOnSprite         : enemyOnSprite;
        Sprite offSprite        = isHeroDisplay ? heroOffSprite        : enemyOffSprite;
        Sprite overhealthSprite = isHeroDisplay ? heroOverhealthSprite : enemyOverhealthSprite;

        for (int i = 0; i < lights.Length; i++)
        {
            if (i < overhealth)
                lights[i].sprite = overhealthSprite; // leftmost first
            else if (i < overhealth + currentHP)
                lights[i].sprite = onSprite;
            else
                lights[i].sprite = offSprite;
        }
    }

    private Vector2[] GetPositions(int count)
    {
        switch (count)
        {
            case 3:
                return RowPositions(3, offsetY: 0f);   // single row of 3
            case 4:
                return RowPositions(4, offsetY: 0f);   // single row of 4
            case 5:
                return TwoRowPositions(topCount: 3, bottomCount: 2); // 3 top, 2 bottom
            case 6:
                return TwoRowPositions(topCount: 3, bottomCount: 3); // 2 rows of 3
            case 7:
                return TwoRowPositions(topCount: 4, bottomCount: 3); // 4 top, 3 bottom
            case 8:
                return TwoRowPositions(topCount: 4, bottomCount: 4); // 2 rows of 4
            case 10:
                return TwoRowPositions(topCount: 5, bottomCount: 5); // 2 rows of 5
            default:
                Debug.LogWarning($"HPDisplay: no layout defined for {count} HP. Falling back to single row.");
                return RowPositions(count, offsetY: 0f);
        }
    }

    // Builds a two-row layout: topCount lights on top, bottomCount lights below,
    // each row independently centered horizontally. Shrinks currentLightSize so
    // rows fit more comfortably within totalWidth.
    private Vector2[] TwoRowPositions(int topCount, int bottomCount)
    {
        currentLightSize = lightSize * twoRowSizeMultiplier;

        int widestRow = Mathf.Max(topCount, bottomCount);
        float spacing = SpacingFor(widestRow);

        Vector2[] top    = RowPositions(topCount,    offsetY:  verticalSpacing * 0.5f, spacing: spacing);
        Vector2[] bottom = RowPositions(bottomCount, offsetY: -verticalSpacing * 0.5f, spacing: spacing);
        Vector2[] all    = new Vector2[topCount + bottomCount];
        top.CopyTo(all, 0);
        bottom.CopyTo(all, topCount);
        return all;
    }

    // Spacing so that `count` lights, evenly spread, span totalWidth.
    // count == 1 has no gaps to space, so just center it (spacing value is unused).
    private float SpacingFor(int count)
    {
        return count > 1 ? totalWidth / (count - 1) : 0f;
    }

    // All single-row layouts (3, 4) share one fixed spacing, derived from the
    // densest single-row case (4) — so 3 HP isn't spread wider than 4 HP.
    private const int SingleRowReferenceCount = 4;

    private Vector2[] RowPositions(int count, float offsetY)
        => RowPositions(count, offsetY, SpacingFor(SingleRowReferenceCount));

    private Vector2[] RowPositions(int count, float offsetY, float spacing)
    {
        Vector2[] positions = new Vector2[count];
        float rowWidth      = (count - 1) * spacing;
        float startX        = -rowWidth * 0.5f;
        for (int i = 0; i < count; i++)
            positions[i] = new Vector2(startX + i * spacing, offsetY);
        return positions;
    }
}