using UnityEngine;

public static class IsActive
{
    public static bool dialogueCooldown = false;
    public static bool isInDialogue = false;
    public static bool isInPause = false;
    private static float exitDoorCooldownUntil = 0f;
    public static bool isInBattleCutscene = false;

    public static bool isActive { get => !isInPause && !isInDialogue; }
    public static bool ExitDoorCooldown => Time.time < exitDoorCooldownUntil;

    public static void StartExitDoorCooldown()
    {
        exitDoorCooldownUntil = Time.time + 2f;
    }
}
