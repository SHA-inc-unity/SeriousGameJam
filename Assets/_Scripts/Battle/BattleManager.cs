using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleManager : MonoBehaviour
{
    [Header("Test Fallback (used only if BattleSetup isn't configured)")]
    public CombatantData testPlayerData;
    public CombatantData testEnemyData;

    [Header("Tuning")]
    public float endBattleDelaySeconds = 2.0f;
    public float wheelSpinDuration = 3.0f;
    public float enemyTurnEndPause = 3.0f;

    public Key playerSpinKey = Key.Space;

    [Header("UI")]
    [SerializeField] private BattleCanvas battleCanvas;
    [SerializeField] private WheelUpgradeScreen upgradeScreen;

    [Header("Audio")]
    [SerializeField] private BattleAudio battleAudio;
    private BattleSoundSet playerSounds;
    private BattleSoundSet enemySounds;

    public BattleState CurrentState { get; private set; }

    private Combatant player;
    private Combatant enemy;
    private CombatantData playerSourceData;
    private BattleDialogueHolder battleDialogue = null;

    private bool battleOver;
    private bool playerOnCooldown;
    private SpinResult currentSpinResult;

    // Cached scene names and callbacks (must be stored before BattleSetup.Clear() is called)
    private string winSceneName;
    private string loseSceneName;
    private System.Action onBattleWon;
    private System.Action onBattleLost;

    private Dictionary<Combatant, int> pendingDamageReductions = new Dictionary<Combatant, int>();
    private HashSet<Combatant> pendingDeflects = new HashSet<Combatant>();
    private List<System.Action> pendingNextSpinEffects = new List<System.Action>();

    public StatusEffectManager StatusEffects { get; private set; }
    private Dictionary<Combatant, WheelSpinUI> combatantWheelUI = new Dictionary<Combatant, WheelSpinUI>();
    private HashSet<Combatant> stunnedCombatants = new HashSet<Combatant>();
    private Dictionary<Combatant, float> spinDurationMultipliers = new Dictionary<Combatant, float>();

    private void Start()
    {
        CombatantData playerData = BattleSetup.IsConfigured ? BattleSetup.PlayerData : testPlayerData;
        CombatantData enemyData = BattleSetup.IsConfigured ? BattleSetup.EnemyData : testEnemyData;
        battleDialogue = BattleSetup.BattleDialogue;

        playerSounds = playerData.soundSet;
        enemySounds = enemyData.soundSet;

        IsActive.isInBattleCutscene = (battleDialogue != null);
        if (IsActive.isInBattleCutscene) StartDialogue();

        if (!BattleSetup.IsConfigured)
            Debug.LogWarning("BattleSetup wasn't configured. Using Test Fallback.");

        if (playerData == null || enemyData == null)
        {
            Debug.LogError("BattleManager has no valid CombatantData. Cannot start battle.");
            return;
        }

        player = playerData.CreateRuntimeCombatant();
        enemy = enemyData.CreateRuntimeCombatant();
        playerSourceData = playerData;

        if (BGMusicManager.Instance != null)
            BGMusicManager.Instance.PlayTrackForEnemy(enemy.displayName);

        StatusEffects = gameObject.AddComponent<StatusEffectManager>();
        StatusEffects.Init(this);
        StatusEffects.Register(player);
        StatusEffects.Register(enemy);

        combatantWheelUI[player] = battleCanvas.PlayerWheelUI;
        combatantWheelUI[enemy] = battleCanvas.EnemyWheelUI;
        spinDurationMultipliers[player] = 1f;
        spinDurationMultipliers[enemy] = 1f;

        battleCanvas.SetPlayerSprite(playerData.battleSprite);
        battleCanvas.SetEnemySprite(enemyData.battleSprite);
        battleCanvas.SetBackground(BattleSetup.BattleBackground);

        if (player.wheel != null) battleCanvas.BuildPlayerWheel(player.wheel);
        if (enemy.wheel != null) battleCanvas.BuildEnemyWheel(enemy.wheel);

        battleCanvas.InitPlayerHP(player.maxHP);
        battleCanvas.InitEnemyHP(enemy.maxHP);

        // Cache scene names and callbacks BEFORE clearing BattleSetup
        winSceneName = BattleSetup.WinSceneName;
        loseSceneName = BattleSetup.LoseSceneName;
        onBattleWon = BattleSetup.OnBattleWon;
        onBattleLost = BattleSetup.OnBattleLost;

        BattleSetup.Clear();

        CurrentState = BattleState.Intro;
        Announce($"{enemy.displayName} appears! Press {playerSpinKey} to spin!");
        CheckDialogueState(BattleState.Intro);
        StartCoroutine(DelayThen(1f, () => CurrentState = BattleState.PlayerTurn));
    }

    private void Update()
    {
        if (battleOver) return;
        if (CurrentState == BattleState.Intro) return;
        if (playerOnCooldown) return;

        if (Keyboard.current[playerSpinKey].wasPressedThisFrame)
            StartCoroutine(SimultaneousSpin());
    }

    // Called by UI Button on the player's wheel
    public void TryPlayerSpin()
    {
        if (battleOver) return;
        if (CurrentState == BattleState.Intro) return;
        if (playerOnCooldown) return;
        StartCoroutine(SimultaneousSpin());
    }

    private IEnumerator SimultaneousSpin()
    {
        playerOnCooldown = true;

        CurrentState = BattleState.PlayerTurn;
        CheckDialogueState(BattleState.PlayerTurn);

        // --- Determine intended slots for both combatants ---
        bool playerStunned = stunnedCombatants.Contains(player);
        bool enemyStunned = stunnedCombatants.Contains(enemy);

        if (playerStunned) Announce($"{player.displayName} is stunned and can't spin!");
        if (enemyStunned) Announce($"{enemy.displayName} is stunned and can't spin!");

        ChooseIntendedSlot(player, out int playerIntended);
        ChooseIntendedSlot(enemy, out int enemyIntended);

        // --- Spin both wheels simultaneously ---
        bool playerAnimDone = false;
        bool enemyAnimDone = false;
        int playerConfirmed = playerIntended;
        int enemyConfirmed = enemyIntended;

        // Pass -1f so WheelSpinUI uses its own spinDuration set on BattleCanvas.
        // spinCooldown on the Wheel asset is only for the between-round pause, not animation length.
        float playerDuration = -1f;
        float enemyDuration = -1f;

        if (!playerStunned)
        {
            if (battleAudio != null) battleAudio.StartWheelSpin(playerSounds);
            battleCanvas.PlayPlayerWheelSpin(playerIntended, player.wheel.slots.Length, (result) =>
            {
                playerConfirmed = result;
                playerAnimDone = true;
            }, playerDuration);
        }
        else
        {
            playerAnimDone = true;
        }

        if (!enemyStunned)
        {
            if (battleAudio != null) battleAudio.StartWheelSpin(enemySounds);
            battleCanvas.PlayEnemyWheelSpin(enemyIntended, enemy.wheel.slots.Length, (result) =>
            {
                enemyConfirmed = result;
                enemyAnimDone = true;
            }, enemyDuration);
        }
        else
        {
            enemyAnimDone = true;
        }

        // Wait for both wheels to finish
        yield return new WaitUntil(() => playerAnimDone && enemyAnimDone);

        if (battleOver) yield break;

        // --- Collect effects ---
        WheelSlotEffect playerEffect = playerStunned ? null : player.wheel.slots[playerConfirmed].effect;
        WheelSlotEffect enemyEffect = enemyStunned ? null : enemy.wheel.slots[enemyConfirmed].effect;

        currentSpinResult = new SpinResult { playerEffect = playerEffect, enemyEffect = enemyEffect };

        StatusEffects.NotifySpinCompleted(player);
        StatusEffects.NotifySpinCompleted(enemy);

        if (!playerStunned) PlayEffectSound(playerEffect, player);
        if (!enemyStunned) PlayEffectSound(enemyEffect, enemy);

        CurrentState = BattleState.PlayerResolve;
        CheckDialogueState(BattleState.PlayerResolve);

        // --- Resolve in priority order (higher priority first, player wins ties) ---
        bool playerGoesFirst = true;
        if (playerEffect != null && enemyEffect != null)
            playerGoesFirst = playerEffect.priority >= enemyEffect.priority;
        else if (playerEffect == null)
            playerGoesFirst = false;

        if (playerGoesFirst)
        {
            if (playerEffect != null)
            {
                CheckDialogueEffect(playerEffect);
                ResolveEffect(playerEffect, attacker: player, defender: enemy);
                if (CheckBattleOver()) yield break;
            }
            if (enemyEffect != null)
            {
                CheckDialogueEffect(enemyEffect);
                ResolveEffect(enemyEffect, attacker: enemy, defender: player);
                if (CheckBattleOver()) yield break;
            }
        }
        else
        {
            if (enemyEffect != null)
            {
                CheckDialogueEffect(enemyEffect);
                ResolveEffect(enemyEffect, attacker: enemy, defender: player);
                if (CheckBattleOver()) yield break;
            }
            if (playerEffect != null)
            {
                CheckDialogueEffect(playerEffect);
                ResolveEffect(playerEffect, attacker: player, defender: enemy);
                if (CheckBattleOver()) yield break;
            }
        }

        yield return new WaitForSeconds(player.wheel.spinCooldown);

        // Apply any effects that were queued to trigger at the START of the next spin
        if (pendingNextSpinEffects.Count > 0)
        {
            foreach (var action in pendingNextSpinEffects)
                action?.Invoke();
            pendingNextSpinEffects.Clear();
        }

        playerOnCooldown = false;
        Announce($"Press {playerSpinKey} to spin again!");
    }

    private void ChooseIntendedSlot(Combatant combatant, out int intendedIndex)
    {
        RiggedStatus rigged = StatusEffects.Get<RiggedStatus>(combatant);

        if (rigged != null && Random.value < 0.5f)
        {
            intendedIndex = rigged.riggedSlotIndex;
            return;
        }

        combatant.wheel.SpinWithIndex(out intendedIndex);
    }

    private void ResolveEffect(WheelSlotEffect effect, Combatant attacker, Combatant defender)
    {
        if (effect == null)
        {
            Announce($"{attacker.displayName}'s wheel returned no effect. Skipping.");
            return;
        }

        effect.Execute(attacker, defender, this, currentSpinResult);
    }

    private bool CheckBattleOver()
    {
        if (enemy.IsDefeated) { EndBattle(playerWon: true); return true; }
        if (player.IsDefeated) { EndBattle(playerWon: false); return true; }
        return false;
    }

    private void EndBattle(bool playerWon)
    {
        battleOver = true;
        StopAllCoroutines();
        if (battleAudio != null) battleAudio.StopWheelSpin();
        CurrentState = playerWon ? BattleState.Win : BattleState.Lose;

        if (playerWon)
            playerSourceData.GrantPermanentHP(1);

        CheckDialogueState(CurrentState);

        Announce(playerWon
            ? $"{player.displayName} wins! {enemy.displayName} is defeated."
            : $"{player.displayName} has been defeated...");

        if (playerWon)
        {
            onBattleWon?.Invoke();
            onBattleWon = null;
        }
        else
        {
            onBattleLost?.Invoke();
            onBattleLost = null;
        }

        if (playerWon && upgradeScreen != null && WheelUpgradeScreen.HasAnyUpgradeAvailable(playerSourceData))
            StartCoroutine(DelayThen(endBattleDelaySeconds, ShowUpgradeScreen));
        else
            StartCoroutine(DelayThen(endBattleDelaySeconds, ReturnToOverworld));
    }

    private void ShowUpgradeScreen()
    {
        upgradeScreen.Show(playerSourceData, onComplete: ReturnToOverworld);
    }

    public Combatant GetPlayer() => player;
    public Combatant GetEnemy() => enemy;
    public BattleCanvas BattleCanvasRef => battleCanvas;

    public void ApplyEffect(WheelSlotEffect effect, Combatant attacker, Combatant defender)
        => ResolveEffect(effect, attacker, defender);

    public void SetStunned(Combatant combatant, bool stunned)
    {
        if (stunned) stunnedCombatants.Add(combatant);
        else stunnedCombatants.Remove(combatant);
    }

    public void MultiplySpinDuration(Combatant combatant, float multiplier)
    {
        if (!spinDurationMultipliers.ContainsKey(combatant))
            spinDurationMultipliers[combatant] = 1f;
        spinDurationMultipliers[combatant] *= multiplier;

        if (combatantWheelUI.TryGetValue(combatant, out WheelSpinUI ui))
            ui.spinDuration *= multiplier;
    }

    public void ApplyDamage(Combatant attacker, Combatant defender, int rawDamage)
    {
        if (StatusEffects.Has<EvadeStatus>(defender))
        {
            StatusEffects.Remove(defender, StatusEffects.Get<EvadeStatus>(defender));
            Announce($"{defender.displayName} evades the attack!");
            return;
        }

        if (pendingDeflects.Contains(defender))
        {
            pendingDeflects.Remove(defender);
            Announce($"{defender.displayName} deflects! {attacker.displayName} takes {rawDamage} reflected damage!");
            attacker.TakeDamage(rawDamage);
            NotifyHPChanged(attacker);
            CheckBattleOver();
            return;
        }

        if (StatusEffects.Has<VulnerableStatus>(defender))
        {
            rawDamage *= 2;
            StatusEffects.Remove(defender, StatusEffects.Get<VulnerableStatus>(defender));
            Announce($"{defender.displayName} is Vulnerable! Hit deals double damage!");
        }

        int reduction = 0;
        if (pendingDamageReductions.TryGetValue(defender, out reduction))
            pendingDamageReductions.Remove(defender);

        int finalDamage = Mathf.Max(0, rawDamage - reduction);
        defender.TakeDamage(finalDamage);
        NotifyHPChanged(defender);
        Announce($"{attacker.displayName} hits for {finalDamage}! " +
                 $"{defender.displayName} HP: {defender.currentHP}/{defender.maxHP}");
    }

    public void NotifyHPChanged(Combatant combatant)
    {
        if (combatant == player)
            battleCanvas.UpdatePlayerHP(player.currentHP, player.overhealth);
        else
            battleCanvas.UpdateEnemyHP(enemy.currentHP, enemy.overhealth);
    }
    
    public void NotifyWheelChanged(Combatant combatant)
    {
        if (combatant == player)
            battleCanvas.RefreshPlayerWheelSprites(player.wheel);
        else
            battleCanvas.RefreshEnemyWheelSprites(enemy.wheel);
    }

    public void SetPendingDamageReduction(Combatant defender, int amount)
        => pendingDamageReductions[defender] = amount;

    public bool HasPendingDamageReduction(Combatant combatant)
        => pendingDamageReductions.ContainsKey(combatant);

    public void SetPendingDeflect(Combatant combatant)
        => pendingDeflects.Add(combatant);

    public bool HasPendingDeflect(Combatant combatant)
        => pendingDeflects.Contains(combatant);

    /// <summary>
    /// Queues an action to run at the start of the next spin (after the current one fully resolves).
    /// Use this for effects like Duckify that should apply to the NEXT round, not the current one.
    /// </summary>
    public void QueueNextSpinEffect(System.Action action)
        => pendingNextSpinEffects.Add(action);

    public void EndBattleImmediately(bool playerWon) => EndBattle(playerWon);

    public void Announce(string message) => Debug.Log(message);

    private void ReturnToOverworld()
    {
        string scene = CurrentState == BattleState.Win
            ? winSceneName
            : loseSceneName;

        if (string.IsNullOrEmpty(scene))
        {
            Announce("(scene name not set - staying on Battle scene.)");
            return;
        }

        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeToScene(scene);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    private IEnumerator DelayThen(float seconds, System.Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    private void StartDialogue()
    {
        DialogueSystemBattle.Instance.StartDialogue(battleDialogue);
    }

    private void CheckDialogueState(BattleState battleState)
    {
        if (!IsActive.isInBattleCutscene) return;
        if (DialogueSystemBattle.Instance == null) return;
        DialogueSystemBattle.Instance.TriggerByState(battleState);
    }

    private void CheckDialogueEffect(WheelSlotEffect effect)
    {
        if (!IsActive.isInBattleCutscene) return;
        if (DialogueSystemBattle.Instance == null) return;
        if (effect == null) return;
        DialogueSystemBattle.Instance.TriggerByEffect(effect);
    }

    private void PlayEffectSound(WheelSlotEffect effect, Combatant attacker)
    {
        if (battleAudio == null) return;

        BattleSoundSet set = (attacker == player) ? playerSounds : enemySounds;
        AudioClip clip = set != null ? set.GetClip(effect) : null;
        if (clip != null)
            battleAudio.PlayClip(clip);
    }

    public void UseSkill(int skillIndex)
    {
        switch (skillIndex)
        {
            case 0: break;
            case 1: break;
            case 2: break;
            case 3: break;
            default: break;
        }
    }
}