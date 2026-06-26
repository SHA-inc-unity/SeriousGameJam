using UnityEngine;

// A static "mailbox" for passing battle parameters from the overworld into
// the Battle scene. Static classes survive a scene load (they're not tied
// to any GameObject), which is exactly what we need here: NPC sets this,
// THEN loads the Battle scene, which reads it back out in Start().
//
// USAGE (from an NPC / BattleTrigger):
//   BattleSetup.PlayerData = someKnightData;
//   BattleSetup.EnemyData = duckGuyData;
//   BattleSetup.BattleMusic = duckStallTrack;
//   BattleSetup.BattleBackground = funfairBg;
//   SceneManager.LoadScene("Battle");
//
// USAGE (from BattleManager.Start()):
//   Combatant player = BattleSetup.PlayerData.CreateRuntimeCombatant();
//   Combatant enemy = BattleSetup.EnemyData.CreateRuntimeCombatant();
public static class BattleSetup
{
    public static CombatantData PlayerData;
    public static CombatantData EnemyData;
    public static AudioClip BattleMusic;
    public static Sprite BattleBackground;
    public static BattleDialogueHolder BattleDialogue;
    public static System.Action OnBattleWon;
    public static System.Action OnBattleLost;
    public static string WinSceneName;
    public static string LoseSceneName;

    /// <summary>True if all required fields are set. BattleManager checks this
    /// on Start() to fail loudly if the Battle scene was opened without a
    /// proper setup call (e.g. testing the scene directly in the editor).</summary>
    public static bool IsConfigured => PlayerData != null && EnemyData != null;

    /// <summary>Clears all fields. Call after a battle ends so stale data
    /// doesn't leak into the next battle if something forgets to set a field.</summary>
    public static void Clear()
    {
        PlayerData = null;
        EnemyData = null;
        BattleMusic = null;
        BattleBackground = null;
        BattleDialogue = null;
        OnBattleWon = null;
        OnBattleLost = null;
        WinSceneName = null;
        LoseSceneName = null;
    }
}