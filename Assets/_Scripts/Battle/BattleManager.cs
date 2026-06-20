using UnityEngine;
using System.Collections;

// Drives the whole battle loop. Drop this on an empty GameObject in your
// battle scene, assign a player wheel + enemy wheel + names/HP in the Inspector,
// hit Play, and watch the Console - the entire battle plays out via Debug.Log
// before you've built a single UI element.
//
// HOOK UP UI LATER: replace the Debug.Log calls and the "AdvanceState" button
// stub with real UI calls. The state machine itself doesn't need to change.
public class BattleManager : MonoBehaviour
{
    [Header("Combatants")]
    public string playerName = "The Knight";
    public int playerMaxHP = 100;
    public int playerAttackPower = 15;

    public string enemyName = "Hook-a-Duck Guy";
    public int enemyMaxHP = 60;
    public int enemyAttackPower = 10;

    [Header("Wheels")]
    [Tooltip("The wheel the PLAYER currently has equipped.")]
    public Wheel playerWheel;
    [Tooltip("The wheel THIS enemy uses. Different enemies can use different wheels.")]
    public Wheel enemyWheel;

    [Header("Tuning")]
    [Tooltip("Seconds to 'pretend' the wheel is spinning before resolving. Stub for now - swap for real animation later.")]
    public float spinDelaySeconds = 1.0f;

    public BattleState CurrentState { get; private set; }

    private Combatant player;
    private Combatant enemy;

    private void Start()
    {
        player = new Combatant(playerName, playerMaxHP, playerAttackPower);
        enemy = new Combatant(enemyName, enemyMaxHP, enemyAttackPower);

        ChangeState(BattleState.Intro);
    }

    private void ChangeState(BattleState newState)
    {
        CurrentState = newState;
        Debug.Log($"[Battle] State -> {newState}");

        switch (newState)
        {
            case BattleState.Intro:
                Debug.Log($"{enemy.displayName} challenges {player.displayName} to a spin!");
                StartCoroutine(DelayThen(1.0f, () => ChangeState(BattleState.PlayerTurn)));
                break;

            case BattleState.PlayerTurn:
                // In the real game: enable a "Spin" button here and wait for the
                // player to press it, then call OnPlayerSpinPressed().
                // For now, auto-spin after a short delay so you can test the loop.
                Debug.Log($"{player.displayName}'s turn. (Auto-spinning for test purposes)");
                StartCoroutine(DelayThen(spinDelaySeconds, OnPlayerSpinPressed));
                break;

            case BattleState.PlayerResolve:
                // handled directly in OnPlayerSpinPressed -> ResolveEffect
                break;

            case BattleState.EnemyTurn:
                Debug.Log($"{enemy.displayName}'s turn.");
                StartCoroutine(DelayThen(spinDelaySeconds, OnEnemySpin));
                break;

            case BattleState.EnemyResolve:
                // handled directly in OnEnemySpin -> ResolveEffect
                break;

            case BattleState.Win:
                Debug.Log($"{player.displayName} wins! {enemy.displayName} is defeated.");
                break;

            case BattleState.Lose:
                Debug.Log($"{player.displayName} has been defeated...");
                break;
        }
    }

    // Call this from your UI "Spin" button's OnClick.
    public void OnPlayerSpinPressed()
    {
        if (CurrentState != BattleState.PlayerTurn) return;

        player.isDefending = false; // reset defend flag each turn
        WheelSlotEffect effect = playerWheel.Spin();

        ChangeState(BattleState.PlayerResolve);
        ResolveEffect(effect, attacker: player, defender: enemy);

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

        enemy.isDefending = false;
        WheelSlotEffect effect = enemyWheel.Spin();

        ChangeState(BattleState.EnemyResolve);
        ResolveEffect(effect, attacker: enemy, defender: player);

        if (player.IsDefeated)
        {
            ChangeState(BattleState.Lose);
            return;
        }

        ChangeState(BattleState.PlayerTurn);
    }

    // Runs whatever effect the wheel landed on. Each effect type knows its
    // own logic (see the Effects/ folder) - this method just delegates.
    // Adding a brand new slot type (Poison, Stun, etc) never requires
    // touching this method.
    private void ResolveEffect(WheelSlotEffect effect, Combatant attacker, Combatant defender)
    {
        if (effect == null)
        {
            Debug.LogWarning($"{attacker.displayName}'s wheel returned no effect. Skipping turn.");
            return;
        }

        effect.Execute(attacker, defender);
    }

    private IEnumerator DelayThen(float seconds, System.Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }
}