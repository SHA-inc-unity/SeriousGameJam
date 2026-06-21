using UnityEngine;

public static class IsActive
{
    public static bool dialogueCooldown = false;
    public static bool isInDialogue = false;
    public static bool isInPause = false;

    public static bool isActive { get => !isInPause && !isInDialogue; }
}
