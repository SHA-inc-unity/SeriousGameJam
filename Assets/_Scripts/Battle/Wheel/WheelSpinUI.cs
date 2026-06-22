using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Rotates a UI Image to land on a slot that's ALREADY been decided by
// Wheel.SpinWithIndex() - this never decides the outcome itself, it just
// makes the spin look natural while landing exactly where the code says it should.
//
// HOW THE MATH WORKS:
//   - The wheel image is divided into `slotCount` equal wedges.
//   - Slot 0 is assumed to start at the top (12 o'clock) and slots go
//     CLOCKWISE from there. If your wheel art has slot 0 somewhere else,
//     adjust slotZeroOffsetDegrees below to compensate.
//   - To make slot `winningIndex` end up at the top (under the pointer),
//     we rotate the WHEEL backwards by that slot's angle (since spinning
//     the wheel clockwise moves slots IN clockwise, so the slot that ends
//     up at the top after a clockwise spin is found by going counter-clockwise
//     from 0 - in practice this just means negating the angle, handled below).
//   - We add several full 360s on top so it visually spins multiple times.
//   - We add a small random "wobble" within the winning wedge so it doesn't
//     land at the exact same pixel every time (looks more physical).
//
// SETUP:
//   1. Put this on the wheel's UI Image (the part that actually spins -
//      NOT the pointer/indicator, which should stay static).
//   2. Make sure the Image's pivot is centered (0.5, 0.5) in its RectTransform,
//      or the rotation will look like it's orbiting instead of spinning in place.
//   3. Call SpinTo(winningIndex, slotCount) and yield/await until it's done,
//      OR pass a callback - see PlaySpin() below.
public class WheelSpinUI : MonoBehaviour
{
    [Tooltip("How long the spin animation takes, start to finish.")]
    [Range(0.5f, 5f)]
    public float spinDuration = 2.5f;

    [Tooltip("How many extra full rotations happen before landing, on top of " +
             "however many are needed to reach the winning slot. Higher = feels " +
             "more dramatic/longer spin.")]
    [Range(1, 8)]
    public int extraFullSpins = 4;

    [Tooltip("Eases the spin so it starts fast and slows down near the end, " +
             "like real friction. 0 = linear (robotic), higher = more dramatic slowdown.")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("If your wheel art's slot 0 isn't at the top (12 o'clock), adjust this " +
             "to match where slot 0 actually starts. 0 = slot 0 starts at the top.")]
    public float slotZeroOffsetDegrees = 0f;

    [Tooltip("Random wobble within the winning wedge, as a fraction of the wedge's " +
             "width (0 = always lands dead-center, 0.4 = can land up to 40% off-center " +
             "in either direction). Keep this comfortably under 0.5 so it can't visually " +
             "spill into the NEXT slot.")]
    [Range(0f, 0.45f)]
    public float landingWobble = 0.3f;

    private RectTransform rect;
    private Image image;
    private bool isSpinning;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        if (image == null)
            Debug.LogError($"WheelSpinUI on '{gameObject.name}' needs an Image component on the same GameObject.");
    }

    /// <summary>True while a spin animation is currently playing.</summary>
    public bool IsSpinning => isSpinning;

    /// <summary>
    /// Sets the wheel graphic shown. Call this once when the battle starts,
    /// using whichever Wheel asset the combatant is using - each Wheel owns
    /// its own sprite, since different enemies' wheels look different
    /// (see Wheel.wheelSprite). Call this BEFORE any spinning happens.
    /// </summary>
    public void SetWheelSprite(Sprite sprite)
    {
        if (image == null) return;

        if (sprite == null)
        {
            Debug.LogWarning($"WheelSpinUI on '{gameObject.name}': SetWheelSprite was given a null sprite. " +
                              "Check the Wheel asset has wheelSprite assigned.");
            return;
        }

        image.sprite = sprite;
    }

    /// <summary>
    /// Plays the spin animation, landing on winningIndex out of slotCount total
    /// slots. Calls onComplete when the animation finishes. Does nothing
    /// (logs a warning) if a spin is already in progress.
    /// </summary>
    public void PlaySpin(int winningIndex, int slotCount, System.Action onComplete = null, float durationOverride = -1f)
    {
        if (isSpinning)
        {
            Debug.LogWarning("WheelSpinUI: PlaySpin called while already spinning. Ignoring.");
            return;
        }

        StartCoroutine(SpinRoutine(winningIndex, slotCount, onComplete, durationOverride));
    }

    private IEnumerator SpinRoutine(int winningIndex, int slotCount, System.Action onComplete, float durationOverride)
    {
        isSpinning = true;

        float duration = durationOverride > 0f ? durationOverride : spinDuration;

        float degreesPerSlot = 360f / slotCount;
        float winningSlotCenterAngle = winningIndex * degreesPerSlot;
        float wobble = Random.Range(-landingWobble, landingWobble) * degreesPerSlot;
        float targetAngleWithinOneRotation = (360f - winningSlotCenterAngle - wobble - slotZeroOffsetDegrees + 360f) % 360f;

        float startAngle = rect.localEulerAngles.z;
        float totalRotation = (extraFullSpins * 360f) + targetAngleWithinOneRotation;
        float endAngle = startAngle + totalRotation;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = easeCurve.Evaluate(t);
            float currentAngle = Mathf.Lerp(startAngle, endAngle, easedT);
            rect.localEulerAngles = new Vector3(0f, 0f, -currentAngle);
            yield return null;
        }

        rect.localEulerAngles = new Vector3(0f, 0f, -endAngle);
        isSpinning = false;
        onComplete?.Invoke();
    }
}