using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the visual layout of a wheel (background, slice overlay, icons) under a given
/// parent transform. Pulled out as a shared helper so BattleCanvas (the spinning battle
/// wheel) and the post-battle upgrade picker draw from the exact same icon-placement
/// code - one formula, used everywhere a wheel needs to be drawn, instead of two copies
/// that could quietly drift apart.
/// </summary>
public static class WheelLayout
{
    /// <summary>
    /// Creates the base/overlay/icon visuals as children of parent. Returns the icon
    /// RectTransforms in slot order (index i = wheel.slots[i]); entries are null for
    /// slots with no effect or no sliceSprite assigned, same convention BattleCanvas used.
    /// </summary>
    public static List<RectTransform> BuildWheelVisual(
    Transform parent, Wheel wheel, float wheelSize,
    float iconDistanceFromCenter, float iconSizeRatio, float slotZeroOffsetDegrees)
    {
        CreateImage("Base", parent, wheel.backgroundSprite, wheelSize);

        float iconDist = wheelSize * iconDistanceFromCenter;
        float iconSizePx = wheelSize * iconSizeRatio;
        int slotCount = wheel.slots.Length;

        var slotIcons = new List<RectTransform>(new RectTransform[slotCount]);

        for (int i = 0; i < slotCount; i++)
        {
            WheelSlotEffect effect = wheel.slots[i].effect;
            if (effect == null) continue;

            var (sliceScale, sliceSprite) = effect.SliceSprite;
            if (sliceSprite == null) continue;

            GameObject iconGO = new GameObject($"Icon_{i}", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(parent, worldPositionStays: false);

            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(iconSizePx * sliceScale, iconSizePx * sliceScale);
            iconRect.pivot = new Vector2(0.5f, 0.5f);

            float angleDeg = 90f + slotZeroOffsetDegrees - (i * (360f / slotCount));
            float angleRad = angleDeg * Mathf.Deg2Rad;

            iconRect.anchoredPosition = new Vector2(
                Mathf.Cos(angleRad) * iconDist,
                Mathf.Sin(angleRad) * iconDist
            );

            iconRect.localEulerAngles = new Vector3(0f, 0f, angleDeg - 90f);
            iconGO.GetComponent<Image>().sprite = sliceSprite;

            slotIcons[i] = iconRect;
        }

        return slotIcons;
    }

    private static Image CreateImage(string goName, Transform parent, Sprite sprite, float size)
    {
        GameObject go       = new GameObject(goName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, worldPositionStays: false);

        RectTransform rt    = go.GetComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(size, size);
        rt.anchoredPosition = Vector2.zero;
        rt.pivot            = new Vector2(0.5f, 0.5f);

        Image img           = go.GetComponent<Image>();
        img.sprite          = sprite;
        return img;
    }
}