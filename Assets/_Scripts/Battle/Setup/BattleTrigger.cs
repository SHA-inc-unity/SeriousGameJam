using UnityEngine;
using UnityEngine.SceneManagement;

// Attach this to ANY NPC GameObject that should be able to start a battle.
// Call StartBattle() from wherever your dialogue/interaction system decides
// the conversation should turn into a fight (e.g. after the player picks a
// "no way I'm freeing these ducks" dialogue option).
//
// All the per-NPC battle data is set in the Inspector below, so each NPC
// just needs this component + their own CombatantData/music/background
// assigned - no code changes needed per-NPC.
public class BattleTrigger : MonoBehaviour
{
    [Header("Combatants")]
    [Tooltip("The player's current combatant data. Usually the same asset across all battles, " +
             "but exposed here in case you want a specific override for a special fight.")]
    public CombatantData playerData;

    [Tooltip("This NPC's combatant data - their stats, sprite, and wheel.")]
    public CombatantData enemyData;

    [Header("Presentation")]
    public AudioClip battleMusic;
    public Sprite battleBackground;

    [Header("Scene")]
    [Tooltip("Name of the Battle scene to load. Must match the scene's name exactly " +
             "and the scene must be added to Build Settings.")]
    public string battleSceneName = "Battle";

    [Header("Dialogue")]
    [Tooltip("Can be null")]
    public BattleDialogueHolder dialogue;

    /// <summary>
    /// Call this to start the battle - fills in BattleSetup, then loads the Battle scene.
    /// </summary>
    public void StartBattle()
    {
        if (enemyData == null)
        {
            Debug.LogError($"BattleTrigger on '{gameObject.name}' has no enemyData assigned. Cannot start battle.");
            return;
        }

        if (playerData == null)
        {
            Debug.LogError($"BattleTrigger on '{gameObject.name}' has no playerData assigned. Cannot start battle.");
            return;
        }

        BattleSetup.PlayerData = playerData;
        BattleSetup.EnemyData = enemyData;
        BattleSetup.BattleMusic = battleMusic;
        BattleSetup.BattleBackground = battleBackground;
        BattleSetup.BattleDialogue = dialogue;

        SceneManager.LoadScene(battleSceneName);
    }
}