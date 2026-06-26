using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTrigger : MonoBehaviour
{
    [Header("Combatants")]
    public CombatantData playerData;
    public CombatantData enemyData;

    [Header("Presentation")]
    public Sprite battleBackground;

    [Header("Scene")]
    public string battleSceneName = "Battle";
    [Tooltip("Scene to go to if the player wins.")]
    public string winSceneName = "";
    [Tooltip("Scene to return to if the player loses.")]
    public string loseSceneName = "";

    [Header("Dialogue")]
    [Tooltip("Can be null")]
    public BattleDialogueHolder dialogue;

    [Header("On Win")]
    [Tooltip("If assigned, replaces one Miss slot on the player's wheel with this effect on victory.")]
    [SerializeField] private WheelSlotEffect replacementEffect;

    public bool defeated = false;

    public void StartBattle()
    {
        if (defeated) return;

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
        BattleSetup.BattleBackground = battleBackground;
        BattleSetup.BattleDialogue = dialogue;
        BattleSetup.OnBattleWon = OnWon;
        BattleSetup.OnBattleLost = OnLost;
        BattleSetup.WinSceneName = winSceneName;
        BattleSetup.LoseSceneName = loseSceneName;

        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeToScene(battleSceneName);
        else
            SceneManager.LoadScene(battleSceneName);
    }

    private void OnWon()
    {
        defeated = true;

        if (replacementEffect == null) return;
        if (playerData == null || playerData.wheel == null) return;

        for (int i = 0; i < playerData.wheel.slots.Length; i++)
        {
            if (playerData.wheel.slots[i].effect is MissEffect)
            {
                playerData.wheel.slots[i].effect = replacementEffect;
                return;
            }
        }

        Debug.LogWarning($"BattleTrigger on '{gameObject.name}': no Miss slot found to replace.");
    }

    private void OnLost()
    {
        defeated = false;
    }
}