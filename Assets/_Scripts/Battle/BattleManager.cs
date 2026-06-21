using UnityEngine;
using System.Collections;

// Drives the whole battle loop. Lives on an empty GameObject in the Battle
// scene. Normally gets its combatants from BattleSetup (filled in by a
// BattleTrigger in the overworld before the scene loads) - see Setup/.
//
// EDITOR TESTING: if you hit Play directly on the Battle scene without going
// through an overworld trigger first, BattleSetup won't be configured. In
// that case this falls back to the "Test Fallback" fields below, so you can
// still iterate on the Battle scene in isolation.
//
// HOOK UP UI LATER: replace Announce() to also push text to a UI element
// instead of (or alongside) Debug.Log. Nothing else needs to change -
// every effect already calls through Announce().
public class BattleManager : MonoBehaviour
{
    [Header("Test Fallback (used only if BattleSetup isn't configured)")]
    [Tooltip("Used when testing the Battle scene directly (Play button on this scene), " +
             "bypassing the overworld trigger flow.")]
    public CombatantData testPlayerData;
    public CombatantData testEnemyData;

    [Header("Tuning")]
    [Tooltip("Seconds to 'pretend' the wheel is spinning before resolving. Stub for now - swap for real animation later.")]
    public float spinDelaySeconds = 1.0f;

    [Tooltip("Seconds to show the Win/Lose message before returning to the overworld.")]
    public float endBattleDelaySeconds = 2.0f;

    [Tooltip("Scene to return to after the battle ends. Leave blank to stay on the " +
             "Battle scene (useful while you don't have an overworld scene wired up yet).")]
    public string overworldSceneName = "";

    public BattleState CurrentState { get; private set; }

    private Combatant player;
    private Combatant enemy;

    // Set by EndBattleImmediately() to short-circuit the normal turn flow.
    private bool battleEndedEarly;

    private void Start()
    {
        CombatantData playerData = BattleSetup.IsConfigured ? BattleSetup.PlayerData : testPlayerData;
        CombatantData enemyData = BattleSetup.IsConfigured ? BattleSetup.EnemyData : testEnemyData;

        if (!BattleSetup.IsConfigured)
            Debug.LogWarning("BattleSetup wasn't configured (scene opened directly?). Using Test Fallback data instead.");

        if (playerData == null || enemyData == null)
        {
            Debug.LogError("BattleManager has no valid CombatantData (neither BattleSetup nor Test Fallback). Cannot start battle.");
            return;
        }

        player = playerData.CreateRuntimeCombatant();
        enemy = enemyData.CreateRuntimeCombatant();

        if (player.wheel == null)
            Debug.LogError($"{player.displayName} has no wheel assigned in their CombatantData.");
        if (enemy.wheel == null)
            Debug.LogError($"{enemy.displayName} has no wheel assigned in their CombatantData.");

        // TODO once UI/audio exist: use BattleSetup.BattleMusic and
        // BattleSetup.BattleBackground here to set up the scene's visuals/audio.

        BattleSetup.Clear(); // consumed - prevents stale data leaking into a future battle

        ChangeState(BattleState.Intro);
    }

    private void ChangeState(BattleState newState)
    {
        CurrentState = newState;
        Debug.Log($"[Battle] State -> {newState}");

        switch (newState)
        {
            case BattleState.Intro:
                Announce($"{enemy.displayName} challenges {player.displayName} to a spin!");
                StartCoroutine(DelayThen(1.0f, () => ChangeState(BattleState.PlayerTurn)));
                break;

            case BattleState.PlayerTurn:
                BeginTurn(player, enemy, isPlayerTurn: true);
                break;

            case BattleState.PlayerResolve:
                // handled directly in OnPlayerSpinPressed -> ResolveEffect
                break;

            case BattleState.EnemyTurn:
                BeginTurn(enemy, player, isPlayerTurn: false);
                break;

            case BattleState.EnemyResolve:
                // handled directly in OnEnemySpin -> ResolveEffect
                break;

            case BattleState.Win:
                Announce($"{player.displayName} wins! {enemy.displayName} is defeated.");
                StartCoroutine(DelayThen(endBattleDelaySeconds, ReturnToOverworld));
                break;

            case BattleState.Lose:
                Announce($"{player.displayName} has been defeated...");
                StartCoroutine(DelayThen(endBattleDelaySeconds, ReturnToOverworld));
                break;
        }
    }

    // Runs at the start of EVERY turn (player or enemy) before any spin happens.
    private void BeginTurn(Combatant actor, Combatant opponent, bool isPlayerTurn)
    {
        actor.isDefending = false; // reset defend flag each turn

        if (isPlayerTurn)
        {
            // In the real game: enable a "Spin" button here and wait for the
            // player to press it, then call OnPlayerSpinPressed().
            // For now, auto-spin after a short delay so you can test the loop.
            Announce($"{player.displayName}'s turn. (Auto-spinning for test purposes)");
            StartCoroutine(DelayThen(spinDelaySeconds, OnPlayerSpinPressed));
        }
        else
        {
            Announce($"{enemy.displayName}'s turn.");
            StartCoroutine(DelayThen(spinDelaySeconds, OnEnemySpin));
        }
    }

    // Call this from your UI "Spin" button's OnClick.
    public void OnPlayerSpinPressed()
    {
        if (CurrentState != BattleState.PlayerTurn) return;

        WheelSlotEffect effect = player.wheel.Spin();

        ChangeState(BattleState.PlayerResolve);
        ResolveEffect(effect, attacker: player, defender: enemy);
        if (battleEndedEarly) return;

        if (enemy.IsDefeated)
        {
            ChangeState(BattleState.Win);
            return;
        }

        ChangeState(BattleState.EnemyTurn);
    }

    private void OnEnemySpin()
    {
        if (CurrentState != BattleState.EnemyTurn) return;

        WheelSlotEffect effect = enemy.wheel.Spin();

        ChangeState(BattleState.EnemyResolve);
        ResolveEffect(effect, attacker: enemy, defender: player);
        if (battleEndedEarly) return;

        if (player.IsDefeated)
        {
            ChangeState(BattleState.Lose);
            return;
        }

        ChangeState(BattleState.PlayerTurn);
    }

    // Runs whatever effect the wheel landed on. Each effect type knows its
    // own logic (see the Effects/ folder) - this method just delegates.
    // Adding a brand new slot type never requires touching this method.
    private void ResolveEffect(WheelSlotEffect effect, Combatant attacker, Combatant defender)
    {
        if (effect == null)
        {
            Announce($"{attacker.displayName}'s wheel returned no effect. Skipping turn.");
            return;
        }

        effect.Execute(attacker, defender, this);
    }

    // ---- Public hooks for effects to call (see Effects/) ----

    /// <summary>Read-only access to the player combatant.</summary>
    public Combatant GetPlayer() => player;

    /// <summary>Read-only access to the enemy combatant.</summary>
    public Combatant GetEnemy() => enemy;

    /// <summary>
    /// Lets one WheelSlotEffect trigger another effect's logic directly
    /// (e.g. "this slot has a 50% chance to also apply Attack_Crit").
    /// </summary>
    public void ApplyEffect(WheelSlotEffect effect, Combatant attacker, Combatant defender)
    {
        ResolveEffect(effect, attacker, defender);
    }

    /// <summary>
    /// Ends the battle right now, skipping any further turns. Use for
    /// effects that should win/lose the fight outright (rare gimmick slots, etc).
    /// </summary>
    public void EndBattleImmediately(bool playerWon)
    {
        battleEndedEarly = true;
        StopAllCoroutines();
        ChangeState(playerWon ? BattleState.Win : BattleState.Lose);
    }

    /// <summary>
    /// Central logging hook. Every effect should call this instead of
    /// Debug.Log directly - this is the one seam to extend later for UI text,
    /// floating combat text, etc, without touching every effect file again.
    /// </summary>
    public void Announce(string message)
    {
        Debug.Log(message);
    }

    private void ReturnToOverworld()
    {
        if (string.IsNullOrEmpty(overworldSceneName))
        {
            Announce("(overworldSceneName not set - staying on Battle scene. Set it in the Inspector once your overworld scene exists.)");
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(overworldSceneName);
    }

    private IEnumerator DelayThen(float seconds, System.Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }
}