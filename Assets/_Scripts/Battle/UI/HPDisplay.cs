using UnityEngine;
using UnityEngine.UI;

public class HPDisplay : MonoBehaviour
{
    [Header("Layout")]
    public float horizontalSpacing = 50f;
    public float verticalSpacing = 50f;
    public float lightSize = 30f;

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

    public void Init(int maxHP, Transform parent, bool isHero)
    {
        isHeroDisplay = isHero;
        transform.SetParent(parent, worldPositionStays: false);
        lights = new Image[maxHP];
        Vector2[] positions = GetPositions(maxHP);

        for (int i = 0; i < maxHP; i++)
        {
            GameObject go = new GameObject($"Light_{i}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, worldPositionStays: false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta        = new Vector2(lightSize, lightSize);
            rt.anchoredPosition = positions[i];

            float percent = (180f / 1920f) * -i;
            if (!isHero) percent = -percent;

            rt.anchorMin = new Vector2(0f + percent, 0f);
            rt.anchorMax = new Vector2(1f + percent, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

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
    public void UpdateHPWithOverhealth(int currentHP, int overhealth)
    {
        Sprite onSprite          = isHeroDisplay ? heroOnSprite          : enemyOnSprite;
        Sprite offSprite         = isHeroDisplay ? heroOffSprite         : enemyOffSprite;
        Sprite overhealthSprite  = isHeroDisplay ? heroOverhealthSprite  : enemyOverhealthSprite;

        for (int i = 0; i < lights.Length; i++)
        {
            if (i < overhealth)
                lights[i].sprite = overhealthSprite;
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
                return RowPositions(3, offsetY: 0f);
            case 5:
            {
                Vector2[] top    = RowPositions(3, offsetY:  verticalSpacing * 0.5f);
                Vector2[] bottom = RowPositions(2, offsetY: -verticalSpacing * 0.5f);
                Vector2[] all    = new Vector2[5];
                top.CopyTo(all, 0);
                bottom.CopyTo(all, 3);
                return all;
            }
            case 7:
            {
                Vector2[] top    = RowPositions(4, offsetY:  verticalSpacing * 0.5f);
                Vector2[] bottom = RowPositions(3, offsetY: -verticalSpacing * 0.5f);
                Vector2[] all    = new Vector2[7];
                top.CopyTo(all, 0);
                bottom.CopyTo(all, 4);
                return all;
            }
            default:
                Debug.LogWarning($"HPDisplay: no layout defined for {count} HP. Falling back to single row.");
                return RowPositions(count, offsetY: 0f);
        }
    }

    private Vector2[] RowPositions(int count, float offsetY)
    {
        Vector2[] positions = new Vector2[count];
        float totalWidth    = (count - 1) * horizontalSpacing;
        float startX        = -totalWidth * 0.5f;
        for (int i = 0; i < count; i++)
            positions[i] = new Vector2(startX + i * horizontalSpacing, offsetY);
        return positions;
    }
}