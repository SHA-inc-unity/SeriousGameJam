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

    [Header("HP Display")]
    [SerializeField] private Transform playerHPAnchor;
    [SerializeField] private Transform enemyHPAnchor;
    [SerializeField] private HPDisplay hpDisplayPrefab;

    [Header("BG")]
    [SerializeField] private Image background;

    private HPDisplay playerHP;
    private HPDisplay enemyHP;
    private WheelSpinUI playerWheel;
    private WheelSpinUI enemyWheel;

    public WheelSpinUI PlayerWheelUI => playerWheel;
    public WheelSpinUI EnemyWheelUI => enemyWheel;

    // Exposed so other screens (e.g. the upgrade picker) can build a wheel with the exact
    // same visual layout without duplicating the icon-placement math.
    public float WheelSize => wheelSize;
    public float IconDistanceFromCenter => iconDistanceFromCenter;
    public float IconSizeRatio => iconSize;
    public float SlotZeroOffsetDegrees => slotZeroOffsetDegrees;

    public void InitPlayerHP(int maxHP)
    {
        playerHP = Instantiate(hpDisplayPrefab);
        playerHP.Init(maxHP, playerHPAnchor, isHero: true);
        playerHP.UpdateHP(maxHP);
    }

    public void InitEnemyHP(int maxHP)
    {
        enemyHP = Instantiate(hpDisplayPrefab);
        enemyHP.Init(maxHP, enemyHPAnchor, isHero: false);
        enemyHP.UpdateHP(maxHP);
    }

    public void UpdatePlayerHP(int currentHP, int overhealth = 0)
    {
        if (overhealth > 0) playerHP.UpdateHPWithOverhealth(currentHP, overhealth);
        else playerHP.UpdateHP(currentHP);
    }

    public void UpdateEnemyHP(int currentHP, int overhealth = 0)
    {
        if (overhealth > 0) enemyHP.UpdateHPWithOverhealth(currentHP, overhealth);
        else enemyHP.UpdateHP(currentHP);
    }

    public void SetPlayerSprite(Sprite sprite) => playerSprite.sprite = sprite;
    public void SetEnemySprite(Sprite sprite) => enemySprite.sprite = sprite;
    public void SetBackground(Sprite sprite) => background.sprite = sprite;

    public void BuildPlayerWheel(Wheel wheel)
        => playerWheel = BuildWheel(wheel, playerWheelAnchor, playerPointerAngle, playerPointer);

    public void BuildEnemyWheel(Wheel wheel)
        => enemyWheel = BuildWheel(wheel, enemyWheelAnchor, enemyPointerAngle, enemyPointer);

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
        spinner.spinDuration = spinDuration;
        spinner.extraFullSpins = extraFullSpins;
        spinner.easeCurve = easeCurve;
        spinner.slotZeroOffsetDegrees = slotZeroOffsetDegrees;
        spinner.pointerAngle = pointerAngle;
        spinner.Init(rootRect, pointer);
        spinner.RegisterSlotIcons(slotIcons);
        spinner.pegSounds = pegSounds;
        spinner.battleAudio = FindAnyObjectByType<BattleAudio>();

        return spinner;
    }

    public void PlayPlayerWheelSpin(int winningIndex, int slotCount, System.Action<int> onComplete, float durationOverride = -1f)
        => playerWheel.PlaySpin(winningIndex, slotCount, onComplete, durationOverride);

    public void PlayEnemyWheelSpin(int winningIndex, int slotCount, System.Action<int> onComplete, float durationOverride = -1f)
        => enemyWheel.PlaySpin(winningIndex, slotCount, onComplete, durationOverride);

    // Re-skins a wheel's icons in place to match its current slots (e.g. after a status
    // effect mutates the underlying Wheel asset's slots, like DuckedStatus). Does not
    // rebuild, reposition, or reparent anything, so it's safe to call mid-battle.
    public void RefreshPlayerWheelSprites(Wheel wheel) => playerWheel?.RefreshSlotSprites(wheel);
    public void RefreshEnemyWheelSprites(Wheel wheel) => enemyWheel?.RefreshSlotSprites(wheel);
}