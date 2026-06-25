using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Post-battle upgrade picker. Shows the player's wheel exactly as it looks in battle
/// (via the shared WheelLayout helper), but every icon is a clickable Button instead of
/// part of a spinning wheel. Slots whose effect has no upgradedVersion are dimmed and not
/// clickable. Picking a slot permanently upgrades that slot on the player's CombatantData
/// asset, then closes the screen.
/// </summary>
public class WheelUpgradeScreen : MonoBehaviour
{
    [Header("Layout (match BattleCanvas's wheel settings so it looks identical)")]
    [SerializeField] private Transform wheelAnchor;
    [SerializeField] private float wheelSize              = 200f;
    [SerializeField] private float iconDistanceFromCenter = 0.55f;
    [SerializeField] private float iconSize               = 0.5f;
    [SerializeField] private float slotZeroOffsetDegrees  = 0f;

    [Header("Eligible-slot visuals")]
    [Tooltip("Alpha applied to icons with no upgrade available, so they read as disabled.")]
    [SerializeField] private float ineligibleAlpha = 0.35f;
    [Tooltip("Optional: a highlight/ring Image to flash or scale on hover. Leave empty to skip.")]
    [SerializeField] private GameObject hoverHighlightPrefab;

    private CombatantData playerData;
    private System.Action onUpgradeChosen;
    private GameObject wheelRoot;

    /// <summary>
    /// Builds and shows the picker for playerData's wheel. onComplete is invoked once the
    /// player has picked an upgrade and it's been applied - use it to close this screen and
    /// continue (e.g. return to overworld).
    /// </summary>
    public void Show(CombatantData data, System.Action onComplete)
    {
        playerData      = data;
        onUpgradeChosen = onComplete;

        if (wheelRoot != null) Destroy(wheelRoot);

        wheelRoot = new GameObject("UpgradeWheelRoot", typeof(RectTransform));
        wheelRoot.transform.SetParent(wheelAnchor, worldPositionStays: false);
        RectTransform rootRect = wheelRoot.GetComponent<RectTransform>();
        rootRect.sizeDelta        = new Vector2(wheelSize, wheelSize);
        rootRect.anchoredPosition = Vector2.zero;

        List<RectTransform> icons = WheelLayout.BuildWheelVisual(
            wheelRoot.transform, data.wheel, wheelSize, iconDistanceFromCenter, iconSize, slotZeroOffsetDegrees);

        for (int i = 0; i < icons.Count; i++)
        {
            RectTransform iconRect = icons[i];
            if (iconRect == null) continue; // no icon for this slot at all (e.g. empty slot)

            WheelSlotEffect effect = data.wheel.slots[i].effect;
            bool eligible = effect != null && effect.HasUpgrade;

            Image iconImage = iconRect.GetComponent<Image>();
            Button button    = iconRect.gameObject.AddComponent<Button>();
            button.targetGraphic = iconImage;

            if (eligible)
            {
                int slotIndex = i; // capture for closure
                button.onClick.AddListener(() => OnSlotPicked(slotIndex));
            }
            else
            {
                button.interactable = false;
                Color c = iconImage.color;
                c.a = ineligibleAlpha;
                iconImage.color = c;
            }
        }

        gameObject.SetActive(true);
    }

    private void OnSlotPicked(int slotIndex)
    {
        bool applied = WheelUpgradeManager.ApplyUpgrade(playerData, slotIndex);
        if (!applied)
        {
            Debug.LogWarning($"WheelUpgradeScreen: upgrade failed for slot {slotIndex}, leaving screen open.");
            return;
        }

        gameObject.SetActive(false);
        if (wheelRoot != null) { Destroy(wheelRoot); wheelRoot = null; }

        onUpgradeChosen?.Invoke();
    }

    /// <summary>
    /// True if at least one slot on data's wheel has an upgrade available. Callers (e.g.
    /// BattleManager) should check this before showing the screen at all - if nothing is
    /// upgradeable, skip straight to ReturnToOverworld instead of showing an all-disabled wheel.
    /// </summary>
    public static bool HasAnyUpgradeAvailable(CombatantData data)
    {
        if (data == null || data.wheel == null || data.wheel.slots == null) return false;

        foreach (var slot in data.wheel.slots)
            if (slot.effect != null && slot.effect.HasUpgrade)
                return true;

        return false;
    }
}