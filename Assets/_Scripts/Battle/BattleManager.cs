using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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

    public float wheelSpinDuration = 3.0f;
    public float enemyTurnEndPause = 3.0f;

    [Tooltip("Scene to return to after the battle ends. Leave blank to stay on the " +
             "Battle scene (useful while you don't have an overworld scene wired up yet).")]
    public string overworldSceneName = "";

    [Tooltip("Key the player presses to spin their wheel on their turn.")]
    public Key playerSpinKey = Key.Space;

    [Header("UI")]
    [SerializeField] private BattleCanvas battleCanvas;

    [Header("Audio")]
    [SerializeField] private BattleAudio battleAudio;
    private BattleSoundSet playerSounds;
    private BattleSoundSet enemySounds;

    public BattleState CurrentState { get; private set; }

    private Combatant player;
    private Combatant enemy;
    private BattleDialogueHolder battleDialogue = null;

    private bool battleEndedEarly;
    private bool waitingForPlayerSpin;

    private void Start()
    {
        CombatantData playerData = BattleSetup.IsConfigured ? BattleSetup.PlayerData : testPlayerData;
        CombatantData enemyData = BattleSetup.IsConfigured ? BattleSetup.EnemyData : testEnemyData;
        battleDialogue = BattleSetup.BattleDialogue;

        playerSounds = playerData.soundSet;
        enemySounds = enemyData.soundSet;

        if (BattleSetup.BattleMusic != null && BGMusicManager.Instance != null)
            BGMusicManager.Instance.PlayForcedBattleTrack(BattleSetup.BattleMusic);

        IsActive.isInBattleCutscene = (battleDialogue != null);
        if (IsActive.isInBattleCutscene) StartDialogue();

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

        battleCanvas.SetPlayerSprite(playerData.battleSprite);
        battleCanvas.SetEnemySprite(enemyData.battleSprite);

        BattleSetup.Clear();

        ChangeState(BattleState.Intro);
    }

    private void Update()
    {
        if (!waitingForPlayerSpin) return;
        if (IsDialoguePlaying()) return;

        if (Keyboard.current[playerSpinKey].wasPressedThisFrame)
        {
            waitingForPlayerSpin = false;
            OnPlayerSpinPressed();
        }
    }

    private bool IsDialoguePlaying()
    {
        return DialogueSystem.Instance != null && DialogueSystem.Instance.IsBattleStepActive;
    }

    private void ChangeState(BattleState newState)
    {
        CurrentState = newState;
        Debug.Log($"[Battle] State -> {newState}");
        if (IsActive.isInBattleCutscene) IsActive.isInBattleCutscene = CheckDialogue(newState);

        StartCoroutine(PlayStateSoundAfterDialogue(newState));

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
                break;

            case BattleState.EnemyTurn:
                BeginTurn(enemy, player, isPlayerTurn: false);
                break;

            case BattleState.EnemyResolve:
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

    private IEnumerator PlayStateSoundAfterDialogue(BattleState state)
    {
        yield return new WaitUntil(() => !IsDialoguePlaying());
        PlayStateSound(state);
    }

    private void BeginTurn(Combatant actor, Combatant opponent, bool isPlayerTurn)
    {
        actor.isDefending = false;

        if (isPlayerTurn)
        {
            Announce($"{player.displayName}'s turn. Press {playerSpinKey} to spin!");
            waitingForPlayerSpin = true;
        }
        else
        {
            Announce($"{enemy.displayName}'s turn.");
            StartCoroutine(DelayThen(spinDelaySeconds, OnEnemySpin));
        }
    }

    public void OnPlayerSpinPressed()
    {
        if (CurrentState != BattleState.PlayerTurn) return;

        StartCoroutine(SpinThenResolve(player, enemy, playerSounds, isPlayerTurn: true));
    }

    private void OnEnemySpin()
    {
        if (CurrentState != BattleState.EnemyTurn) return;

        StartCoroutine(SpinThenResolve(enemy, player, enemySounds, isPlayerTurn: false));
    }

    private IEnumerator SpinThenResolve(Combatant actor, Combatant opponent, BattleSoundSet actorSounds, bool isPlayerTurn)
    {
        yield return new WaitUntil(() => !IsDialoguePlaying());

        if (battleAudio != null) battleAudio.StartWheelSpin(actorSounds);

        yield return new WaitForSeconds(wheelSpinDuration);

        if (battleAudio != null) battleAudio.StopWheelSpin();

        WheelSlotEffect effect = actor.wheel.Spin();

        if (isPlayerTurn)
            ChangeState(BattleState.PlayerResolve);
        else
            ChangeState(BattleState.EnemyResolve);

        ResolveEffect(effect, actor, opponent);
        if (battleEndedEarly) yield break;

        yield return new WaitUntil(() => !IsDialoguePlaying());

        if (opponent.IsDefeated)
        {
            if (isPlayerTurn)
                ChangeState(BattleState.Win);
            else
                ChangeState(BattleState.Lose);
            yield break;
        }

        if (isPlayerTurn)
        {
            ChangeState(BattleState.EnemyTurn);
        }
        else
        {
            yield return new WaitForSeconds(enemyTurnEndPause);
            ChangeState(BattleState.PlayerTurn);
        }
    }

    private void ResolveEffect(WheelSlotEffect effect, Combatant attacker, Combatant defender)
    {
        if (effect == null)
        {
            Announce($"{attacker.displayName}'s wheel returned no effect. Skipping turn.");
            return;
        }

        PlayEffectSound(effect, attacker);

        effect.Execute(attacker, defender, this);
    }

    public Combatant GetPlayer() => player;

    public Combatant GetEnemy() => enemy;

    public void ApplyEffect(WheelSlotEffect effect, Combatant attacker, Combatant defender)
    {
        ResolveEffect(effect, attacker, defender);
    }

    public void EndBattleImmediately(bool playerWon)
    {
        battleEndedEarly = true;
        waitingForPlayerSpin = false;
        StopAllCoroutines();
        if (battleAudio != null) battleAudio.StopWheelSpin();
        ChangeState(playerWon ? BattleState.Win : BattleState.Lose);
    }

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

    private void StartDialogue()
    {
        DialogueSystem.Instance.StartDialogue(battleDialogue);
    }

    private bool CheckDialogue(BattleState battleState)
    {
        return DialogueSystem.Instance.TriggerPlayDialogue(battleState);
    }

    private void PlayEffectSound(WheelSlotEffect effect, Combatant attacker)
    {
        if (battleAudio == null) return;

        BattleSoundSet set;
        if (attacker == player)
            set = playerSounds;
        else
            set = enemySounds;

        AudioClip clip = set != null ? set.GetClip(effect) : null;
        if (clip != null)
            battleAudio.PlayClip(clip);
    }

    private void PlayStateSound(BattleState state)
    {
        if (battleAudio == null) return;

        BattleSoundSet set = GetSoundSetForState(state);
        battleAudio.PlayState(set, state);
    }

    private BattleSoundSet GetSoundSetForState(BattleState state)
    {
        if (state == BattleState.PlayerTurn || state == BattleState.PlayerResolve) return playerSounds;
        if (state == BattleState.EnemyTurn || state == BattleState.EnemyResolve) return enemySounds;

        return playerSounds;
    }
}