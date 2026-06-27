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
    [SerializeField] private float wheelSize = 200f;
    [SerializeField] private float iconDistanceFromCenter = 0.55f;
    [SerializeField] private float iconSize = 0.5f;
    [SerializeField] private float slotZeroOffsetDegrees = 0f;
    [SerializeField] private float spinDuration = 2.5f;
    [SerializeField] private int extraFullSpins = 4;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private List<AudioClip> pegSounds;

    [Tooltip("Used only to pick which direction the wheel spins toward initially. Left = 180.")]
    [SerializeField] private float playerPointerAngle = 180f;

    [Tooltip("Used only to pick which direction the wheel spins toward initially. Right = 0.")]
    [SerializeField] private float enemyPointerAngle = 0f;

    // ── HP Display ─────────────────────────────────────────────────────────────
    [Header("HP Display")]
    [SerializeField] private HPDisplay hpDisplayPrefab;

    [Tooltip("Left/only column for the player (slots 1–4).")]
    [SerializeField] private Transform playerHPAnchorCol1;

    [Tooltip("Right column for the player (slots 5–8). Only used when player HP > 4.")]
    [SerializeField] private Transform playerHPAnchorCol2;

    [Tooltip("Left/only column for the enemy (slots 1–4).")]
    [SerializeField] private Transform enemyHPAnchorCol1;

    [Tooltip("Right column for the enemy (slots 5–8). Only used when enemy HP > 4.")]
    [SerializeField] private Transform enemyHPAnchorCol2;

    // ── BG ─────────────────────────────────────────────────────────────────────
    [Header("BG")]
    [SerializeField] private Image background;

    // ── private state ──────────────────────────────────────────────────────────
    private HPDisplay playerHPCol1;
    private HPDisplay playerHPCol2; // null when playerMaxHP <= 4
    private int       playerMaxHP;

    private HPDisplay enemyHPCol1;
    private HPDisplay enemyHPCol2;  // null when enemyMaxHP <= 4
    private int       enemyMaxHP;

    private WheelSpinUI playerWheel;
    private WheelSpinUI enemyWheel;

    // ── wheel accessors (unchanged) ────────────────────────────────────────────
    public WheelSpinUI PlayerWheelUI => playerWheel;
    public WheelSpinUI EnemyWheelUI => enemyWheel;

    public float WheelSize               => wheelSize;
    public float IconDistanceFromCenter  => iconDistanceFromCenter;
    public float IconSizeRatio           => iconSize;
    public float SlotZeroOffsetDegrees   => slotZeroOffsetDegrees;

    // ── HP init ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates one or two HPDisplay columns for the player.
    /// Column 1 holds slots 1–4; column 2 holds slots 5–8 (if maxHP > 4).
    /// </summary>
    public void InitPlayerHP(int maxHP)
    {
        playerMaxHP = Mathf.Clamp(maxHP, 1, 8);
        (playerHPCol1, playerHPCol2) = InitHPColumns(
            playerMaxHP, playerHPAnchorCol1, playerHPAnchorCol2, isHero: true);
        UpdatePlayerHP(playerMaxHP);
    }

    /// <summary>
    /// Creates one or two HPDisplay columns for the enemy.
    /// Column 1 holds slots 1–4; column 2 holds slots 5–8 (if maxHP > 4).
    /// </summary>
    public void InitEnemyHP(int maxHP)
    {
        enemyMaxHP = Mathf.Clamp(maxHP, 1, 8);
        (enemyHPCol1, enemyHPCol2) = InitHPColumns(
            enemyMaxHP, enemyHPAnchorCol1, enemyHPAnchorCol2, isHero: false);
        UpdateEnemyHP(enemyMaxHP);
    }

    // ── HP update ──────────────────────────────────────────────────────────────

    public void UpdatePlayerHP(int currentHP, int overhealth = 0)
    {
        ApplyHP(currentHP, overhealth, playerMaxHP, playerHPCol1, playerHPCol2);
    }

    public void UpdateEnemyHP(int currentHP, int overhealth = 0)
    {
        ApplyHP(currentHP, overhealth, enemyMaxHP, enemyHPCol1, enemyHPCol2);
    }

    // ── sprite / background setters (unchanged) ────────────────────────────────
    public void SetPlayerSprite(Sprite sprite) => playerSprite.sprite = sprite;
    public void SetEnemySprite(Sprite sprite)  => enemySprite.sprite  = sprite;
    public void SetBackground(Sprite sprite)   => background.sprite   = sprite;

    // ── wheel builders (unchanged) ─────────────────────────────────────────────
    public void BuildPlayerWheel(Wheel wheel)
        => playerWheel = BuildWheel(wheel, playerWheelAnchor, playerPointerAngle, playerPointer);

    public void BuildEnemyWheel(Wheel wheel)
        => enemyWheel = BuildWheel(wheel, enemyWheelAnchor, enemyPointerAngle, enemyPointer);

    public void PlayPlayerWheelSpin(int winningIndex, int slotCount, System.Action<int> onComplete, float durationOverride = -1f)
        => playerWheel.PlaySpin(winningIndex, slotCount, onComplete, durationOverride);

    public void PlayEnemyWheelSpin(int winningIndex, int slotCount, System.Action<int> onComplete, float durationOverride = -1f)
        => enemyWheel.PlaySpin(winningIndex, slotCount, onComplete, durationOverride);

    public void RefreshPlayerWheelSprites(Wheel wheel) => playerWheel?.RefreshSlotSprites(wheel);
    public void RefreshEnemyWheelSprites(Wheel wheel)  => enemyWheel?.RefreshSlotSprites(wheel);

    // ── private helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Instantiates the necessary HPDisplay column(s).
    /// Returns (col1, col2). col2 is null when maxHP ≤ 4.
    /// </summary>
    private (HPDisplay col1, HPDisplay col2) InitHPColumns(
        int maxHP, Transform anchor1, Transform anchor2, bool isHero)
    {
        int col1Slots = Mathf.Min(maxHP, 4);          // 1–4
        int col2Slots = Mathf.Max(0, maxHP - 4);      // 0–4

        HPDisplay col1 = Instantiate(hpDisplayPrefab);
        col1.Init(col1Slots, anchor1, isHero);

        HPDisplay col2 = null;
        if (col2Slots > 0)
        {
            col2 = Instantiate(hpDisplayPrefab);
            col2.Init(col2Slots, anchor2, isHero);
        }

        return (col1, col2);
    }

    /// <summary>
    /// Splits currentHP / overhealth across the two columns and calls the
    /// appropriate Update method on each.
    ///
    /// Column 1 owns slots 1–4 (indices 0–3).
    /// Column 2 owns slots 5–8 (indices 4–7).
    ///
    /// Overhealth fills from the top of column 1 first, then column 2.
    /// </summary>
    private static void ApplyHP(
        int currentHP, int overhealth,
        int maxHP,
        HPDisplay col1, HPDisplay col2)
    {
        int col1Max = Mathf.Min(maxHP, 4);
        int col2Max = Mathf.Max(0, maxHP - 4);

        // ── overhealth split ──────────────────────────────────────────────────
        // Overhealth fills col1 first, then spills into col2.
        int oh1 = Mathf.Clamp(overhealth, 0, col1Max);
        int oh2 = col2 != null ? Mathf.Clamp(overhealth - col1Max, 0, col2Max) : 0;

        // ── normal HP split ───────────────────────────────────────────────────
        // HP fills the remainder of col1 after overhealth, then col2.
        int col1Remaining = col1Max - oh1;
        int hp1 = Mathf.Clamp(currentHP, 0, col1Remaining);
        int hp2 = col2 != null ? Mathf.Clamp(currentHP - col1Remaining, 0, col2Max - oh2) : 0;

        // ── apply to col1 ─────────────────────────────────────────────────────
        if (oh1 > 0) col1.UpdateHPWithOverhealth(hp1, oh1);
        else         col1.UpdateHP(hp1);

        // ── apply to col2 (if it exists) ──────────────────────────────────────
        if (col2 == null) return;

        if (oh2 > 0) col2.UpdateHPWithOverhealth(hp2, oh2);
        else         col2.UpdateHP(hp2);
    }

    private WheelSpinUI BuildWheel(Wheel wheel, Transform anchor, float pointerAngle, Transform pointer)
    {
        if (pointer == null)
            Debug.LogWarning($"BattleCanvas: no pointer Transform assigned for wheel on '{anchor.name}'. " +
                              "Spin results will fall back to the intended winningIndex.");

        GameObject root = new GameObject("WheelRoot", typeof(RectTransform));
        root.transform.SetParent(anchor, worldPositionStays: false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(wheelSize, wheelSize);
        rootRect.anchoredPosition = Vector2.zero;

        List<RectTransform> slotIcons = WheelLayout.BuildWheelVisual(
            root.transform, wheel, wheelSize, iconDistanceFromCenter, iconSize, slotZeroOffsetDegrees);

        WheelSpinUI spinner = root.AddComponent<WheelSpinUI>();
        spinner.spinDuration        = spinDuration;
        spinner.extraFullSpins      = extraFullSpins;
        spinner.easeCurve           = easeCurve;
        spinner.slotZeroOffsetDegrees = slotZeroOffsetDegrees;
        spinner.pointerAngle        = pointerAngle;
        spinner.Init(rootRect, pointer);
        spinner.RegisterSlotIcons(slotIcons);
        spinner.pegSounds           = pegSounds;
        spinner.battleAudio         = FindAnyObjectByType<BattleAudio>();

        return spinner;
    }
}