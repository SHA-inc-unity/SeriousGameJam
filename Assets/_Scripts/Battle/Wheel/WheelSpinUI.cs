using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spins the wheel, then determines the winning slot by finding which icon is physically
/// closest to the pointer GameObject once the spin settles. The pointer is a real Transform
/// placed in the scene; icons are real RectTransforms already being rendered. Both are
/// measured directly (Vector3.Distance in world space) - there's no angle formula and no
/// duplicate geometry that has to be kept in sync with the art.
/// </summary>
public class WheelSpinUI : MonoBehaviour
{
    [Range(0.5f, 5f)] public float spinDuration = 2.5f;
    [Range(1, 8)] public int extraFullSpins = 4;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float slotZeroOffsetDegrees = 0f;
    public float scale = 2.4f;

    [Tooltip("Angle in degrees used only to choose which way to spin toward at the start " +
             "(0 = right, 90 = top, 180 = left, 270 = bottom). The actual winner is read " +
             "from the pointer Transform below, not from this angle.")]
    public float pointerAngle = 90f;

    [Tooltip("The pointer marker in the scene. Whichever icon ends up closest to this " +
             "Transform when the spin stops is the winning slot.")]
    public Transform pointerTransform;
    public List<AudioClip> pegSounds;
    public BattleAudio battleAudio;

    private RectTransform rect;
    private bool isSpinning;
    private List<RectTransform> slotIcons = new List<RectTransform>();

    // Track true accumulated angle ourselves - never read back from Unity since
    // localEulerAngles.z gets normalized to 0-360 and breaks multi-spin math.
    private float currentAngle = 0f;

    public bool IsSpinning => isSpinning;

    public void Init(RectTransform rootRect, Transform pointer)
    {
        rect = rootRect;
        pointerTransform = pointer;
        currentAngle = 0f;
        gameObject.transform.localScale = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// Registers the icon RectTransforms in slot order (index 0 = slot 0, etc). Call once
    /// right after the icons are created. The winner readout measures these directly, so
    /// whatever is actually on screen is exactly what gets reported.
    /// </summary>
    public void RegisterSlotIcons(List<RectTransform> icons)
    {
        slotIcons = icons;
    }

    /// <summary>
    /// Re-skins the already-built icons to match wheel.slots[i].sliceSprite, in place.
    /// Does not touch rotation, spin state, or icon layout/parenting - only swaps each
    /// icon's displayed sprite. Safe to call mid-battle (e.g. when a status effect
    /// temporarily reskins a wheel) since it never rebuilds or destroys anything, and
    /// has no effect on IsSpinning or any in-flight spin coroutine.
    /// </summary>
    public void RefreshSlotSprites(Wheel wheel)
    {
        if (wheel == null || wheel.slots == null) return;

        for (int i = 0; i < slotIcons.Count && i < wheel.slots.Length; i++)
        {
            if (slotIcons[i] == null) continue;

            Image img = slotIcons[i].GetComponent<Image>();
            if (img == null) img = slotIcons[i].GetComponentInChildren<Image>();

            if (img != null)
            {
                var slot = wheel.slots[i];

                Sprite chosenSprite;
                float chosenScale;

                if (slot.sliceSprite != null)
                {
                    chosenSprite = slot.sliceSprite;
                    chosenScale = 1f;
                }
                else if (slot.effect != null)
                {
                    var (effScale, effSprite) = slot.effect.SliceSprite;
                    chosenSprite = effSprite;
                    chosenScale = effScale;
                }
                else
                {
                    chosenSprite = null;
                    chosenScale = 1f;
                }

                img.sprite = chosenSprite;
            }
            else
                Debug.LogWarning($"WheelSpinUI: slot icon {i} has no Image component to refresh.");
        }
    }

    public void PlaySpin(int winningIndex, int slotCount, System.Action<int> onComplete = null, float durationOverride = -1f)
    {
        if (isSpinning)
        {
            Debug.LogWarning("WheelSpinUI: already spinning, ignoring.");
            return;
        }
        StartCoroutine(SpinRoutine(winningIndex, slotCount, onComplete, durationOverride));
    }

    private IEnumerator SpinRoutine(int winningIndex, int slotCount, System.Action<int> onComplete, float durationOverride)
    {
        isSpinning = true;

        float duration = durationOverride > 0f ? durationOverride : spinDuration;
        float degreesPerSlot = 360f / slotCount;

        // This is only the spin's AIM, to pick a direction/target to rotate toward. The
        // actual reported winner is measured below from real icon positions, so the result
        // always matches what's drawn on screen regardless of how accurate this aim is.
        float slotCurrentAngle = 90f + slotZeroOffsetDegrees - (winningIndex * degreesPerSlot);
        float targetAngleWithinOneRotation = (slotCurrentAngle - pointerAngle + 720f) % 360f;

        float startAngle = currentAngle;
        float totalRotation = (extraFullSpins * 360f) + targetAngleWithinOneRotation;
        float endAngle = startAngle + totalRotation;

        float elapsed = 0f;

        // Track how many slot boundaries we've crossed
        int lastSlotIndex = Mathf.FloorToInt(startAngle / degreesPerSlot);
        int pegSoundsIndex = 0; // alternates between 0 and 1

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = easeCurve.Evaluate(t);
            currentAngle = Mathf.Lerp(startAngle, endAngle, easedT);
            rect.localEulerAngles = new Vector3(0f, 0f, -currentAngle);
            int currentSlotIndex = Mathf.FloorToInt(currentAngle / degreesPerSlot);
            if (currentSlotIndex != lastSlotIndex)
            {
                battleAudio.PlayClip(pegSounds[pegSoundsIndex]);
                pegSoundsIndex = 1 - pegSoundsIndex;
                lastSlotIndex = currentSlotIndex;
            }
            yield return null;
        }

        currentAngle = endAngle;
        rect.localEulerAngles = new Vector3(0f, 0f, -currentAngle);
        isSpinning = false;

        int actualWinningIndex = ReadWinnerFromClosestIcon(winningIndex);
        onComplete?.Invoke(actualWinningIndex);
    }

    /// <summary>
    /// Finds whichever registered icon is currently closest to the pointer Transform in
    /// world space. Falls back to the intended winningIndex (with a warning) only if no
    /// icons were registered, which should not happen in normal operation.
    /// </summary>
    private int ReadWinnerFromClosestIcon(int fallbackIndex)
    {
        if (pointerTransform == null)
        {
            Debug.LogWarning("WheelSpinUI: no pointerTransform assigned. Falling back to intended winningIndex.");
            return fallbackIndex;
        }

        if (slotIcons == null || slotIcons.Count == 0)
        {
            Debug.LogWarning("WheelSpinUI: no slot icons registered. Falling back to intended winningIndex.");
            return fallbackIndex;
        }

        int closestIndex = fallbackIndex;
        float closestDist = float.MaxValue;

        for (int i = 0; i < slotIcons.Count; i++)
        {
            if (slotIcons[i] == null) continue;

            float dist = Vector3.Distance(slotIcons[i].position, pointerTransform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        return closestIndex;
    }
}