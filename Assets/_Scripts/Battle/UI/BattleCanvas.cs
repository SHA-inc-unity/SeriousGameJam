using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleCanvas : MonoBehaviour
{
    [SerializeField] private Image playerSprite;
    [SerializeField] private Image enemySprite;

    [Header("Wheel Anchors")]
    [SerializeField] private Transform playerWheelAnchor;
    [SerializeField] private Transform enemyWheelAnchor;

    [Header("Wheel Pointers")]
    [Tooltip("Pointer marker for the player wheel. A plain Transform/GameObject placed " +
             "wherever the pointer visually sits. The winning slot is whichever icon ends " +
             "up closest to this when the wheel stops.")]
    [SerializeField] private Transform playerPointer;

    [Tooltip("Pointer marker for the enemy wheel.")]
    [SerializeField] private Transform enemyPointer;

    [Header("Wheel Settings")]
    [SerializeField] private float wheelSize              = 200f;
    [SerializeField] private float iconDistanceFromCenter = 0.55f;
    [SerializeField] private float iconSize               = 0.5f;
    [SerializeField] private float slotZeroOffsetDegrees  = 0f;
    [SerializeField] private float spinDuration           = 2.5f;
    [SerializeField] private int   extraFullSpins         = 4;
    [SerializeField] private AnimationCurve easeCurve     = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("Used only to pick which direction the wheel spins toward initially. Left = 180.")]
    [SerializeField] private float playerPointerAngle = 180f;

    [Tooltip("Used only to pick which direction the wheel spins toward initially. Right = 0.")]
    [SerializeField] private float enemyPointerAngle  = 0f;

    [Header("HP Display")]
    [SerializeField] private Transform playerHPAnchor;
    [SerializeField] private Transform enemyHPAnchor;
    [SerializeField] private HPDisplay hpDisplayPrefab;

    private HPDisplay  playerHP;
    private HPDisplay  enemyHP;
    private WheelSpinUI playerWheel;
    private WheelSpinUI enemyWheel;

    public WheelSpinUI PlayerWheelUI => playerWheel;
    public WheelSpinUI EnemyWheelUI  => enemyWheel;

    public void InitPlayerHP(int maxHP)
    {
        playerHP = Instantiate(hpDisplayPrefab);
        playerHP.Init(maxHP, playerHPAnchor);
        playerHP.UpdateHP(maxHP);
    }

    public void InitEnemyHP(int maxHP)
    {
        enemyHP = Instantiate(hpDisplayPrefab);
        enemyHP.Init(maxHP, enemyHPAnchor);
        enemyHP.UpdateHP(maxHP);
    }

    public void UpdatePlayerHP(int currentHP, int overhealth = 0)
    {
        if (overhealth > 0) playerHP.UpdateHPWithOverhealth(currentHP, overhealth);
        else                playerHP.UpdateHP(currentHP);
    }

    public void UpdateEnemyHP(int currentHP, int overhealth = 0)
    {
        if (overhealth > 0) enemyHP.UpdateHPWithOverhealth(currentHP, overhealth);
        else                enemyHP.UpdateHP(currentHP);
    }

    public void SetPlayerSprite(Sprite sprite) => playerSprite.sprite = sprite;
    public void SetEnemySprite(Sprite sprite)  => enemySprite.sprite  = sprite;

    public void BuildPlayerWheel(Wheel wheel)
        => playerWheel = BuildWheel(wheel, playerWheelAnchor, playerPointerAngle, playerPointer);

    public void BuildEnemyWheel(Wheel wheel)
        => enemyWheel = BuildWheel(wheel, enemyWheelAnchor, enemyPointerAngle, enemyPointer);

    private WheelSpinUI BuildWheel(Wheel wheel, Transform anchor, float pointerAngle, Transform pointer)
    {
        if (pointer == null)
            Debug.LogWarning($"BattleCanvas: no pointer Transform assigned for wheel on '{anchor.name}'. " +
                              "Spin results will fall back to the intended winningIndex.");

        // Root — everything is a child of this so it all spins together
        GameObject root        = new GameObject("WheelRoot", typeof(RectTransform));
        root.transform.SetParent(anchor, worldPositionStays: false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta        = new Vector2(wheelSize, wheelSize);
        rootRect.anchoredPosition = Vector2.zero;

        // Layer 1: red base circle
        CreateImage("Base", root.transform, wheel.backgroundSprite, wheelSize);

        // Layer 2: black and white slice divider overlay
        CreateImage("SliceOverlay", root.transform, wheel.overlaySprite, wheelSize);

        // Layer 3: icons, positioned and rotated per slot
        float iconDist   = wheelSize * iconDistanceFromCenter;
        float iconSizePx = wheelSize * iconSize;
        int   slotCount  = wheel.slots.Length;

        // One entry per slot index, null where there's no icon (e.g. no effect assigned).
        // Keeping nulls in place (instead of skipping) means slot index i in this list
        // always corresponds to wheel.slots[i] - no renumbering for callers to get wrong.
        var slotIcons = new List<RectTransform>(new RectTransform[slotCount]);

        for (int i = 0; i < slotCount; i++)
        {
            WheelSlotEffect effect = wheel.slots[i].effect;
            if (effect == null || effect.sliceSprite == null) continue;

            GameObject iconGO  = new GameObject($"Icon_{i}", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(root.transform, worldPositionStays: false);

            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.sizeDelta     = new Vector2(iconSizePx, iconSizePx);
            iconRect.pivot         = new Vector2(0.5f, 0.5f);

            float angleDeg = 90f + slotZeroOffsetDegrees - (i * (360f / slotCount));
            float angleRad = angleDeg * Mathf.Deg2Rad;

            iconRect.anchoredPosition = new Vector2(
                Mathf.Cos(angleRad) * iconDist,
                Mathf.Sin(angleRad) * iconDist
            );

            iconRect.localEulerAngles = new Vector3(0f, 0f, angleDeg - 90f);
            iconGO.GetComponent<Image>().sprite = effect.sliceSprite;

            slotIcons[i] = iconRect;
        }

        // Spinner lives on the root
        WheelSpinUI spinner           = root.AddComponent<WheelSpinUI>();
        spinner.spinDuration          = spinDuration;
        spinner.extraFullSpins        = extraFullSpins;
        spinner.easeCurve             = easeCurve;
        spinner.slotZeroOffsetDegrees = slotZeroOffsetDegrees;
        spinner.pointerAngle          = pointerAngle;
        spinner.Init(rootRect, pointer);
        spinner.RegisterSlotIcons(slotIcons);

        return spinner;
    }

    private Image CreateImage(string goName, Transform parent, Sprite sprite, float size)
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

    /// <summary>
    /// onComplete receives the slot actually closest to the pointer when the spin stops -
    /// callers should use that index, not the one they passed in, to resolve gameplay effects.
    /// </summary>
    public void PlayPlayerWheelSpin(int winningIndex, int slotCount, System.Action<int> onComplete, float durationOverride = -1f)
        => playerWheel.PlaySpin(winningIndex, slotCount, onComplete, durationOverride);

    public void PlayEnemyWheelSpin(int winningIndex, int slotCount, System.Action<int> onComplete, float durationOverride = -1f)
        => enemyWheel.PlaySpin(winningIndex, slotCount, onComplete, durationOverride);
}