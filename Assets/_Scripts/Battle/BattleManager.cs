using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    [Header("Test Fallback (used only if BattleSetup isn't configured)")]
    public CombatantData testPlayerData;
    public CombatantData testEnemyData;

    [Header("Tuning")]
    public float endBattleDelaySeconds = 2.0f;
    public string overworldSceneName = "";
    
    public Key playerSpinKey = Key.Space;

    [Header("UI")]
    [SerializeField] private BattleCanvas battleCanvas;

    public BattleState CurrentState { get; private set; }

    private Combatant player;
    private Combatant enemy;

    private bool battleOver;
    private bool playerOnCooldown;

    private Dictionary<Combatant, int> pendingDamageReductions = new Dictionary<Combatant, int>();
    
    private HashSet<Combatant> pendingDeflects = new HashSet<Combatant>();
    
    public StatusEffectManager StatusEffects { get; private set; }
    private Dictionary<Combatant, WheelSpinUI> combatantWheelUI = new Dictionary<Combatant, WheelSpinUI>();
    private HashSet<Combatant> stunnedCombatants = new HashSet<Combatant>();
    private Dictionary<Combatant, float> spinDurationMultipliers = new Dictionary<Combatant, float>();

    private void Start()
    {
        CombatantData playerData = BattleSetup.IsConfigured ? BattleSetup.PlayerData : testPlayerData;
        CombatantData enemyData  = BattleSetup.IsConfigured ? BattleSetup.EnemyData  : testEnemyData;

        if (!BattleSetup.IsConfigured)
            Debug.LogWarning("BattleSetup wasn't configured. Using Test Fallback.");

        if (playerData == null || enemyData == null)
        {
            Debug.LogError("BattleManager has no valid CombatantData. Cannot start battle.");
            return;
        }

        player = playerData.CreateRuntimeCombatant();
        enemy  = enemyData.CreateRuntimeCombatant();
        
        StatusEffects = gameObject.AddComponent<StatusEffectManager>();
        StatusEffects.Init(this);
        StatusEffects.Register(player);
        StatusEffects.Register(enemy);

        combatantWheelUI[player] = battleCanvas.PlayerWheelUI;
        combatantWheelUI[enemy] = battleCanvas.EnemyWheelUI;
        spinDurationMultipliers[player] = 1f;
        spinDurationMultipliers[enemy]  = 1f;

        battleCanvas.SetPlayerSprite(playerData.battleSprite);
        battleCanvas.SetEnemySprite(enemyData.battleSprite);

        if (player.wheel != null) battleCanvas.SetPlayerWheel(player.wheel.wheelSprite);
        if (enemy.wheel  != null) battleCanvas.SetEnemyWheel(enemy.wheel.wheelSprite);
        
        battleCanvas.InitPlayerHP(player.maxHP);
        battleCanvas.InitEnemyHP(enemy.maxHP);

        BattleSetup.Clear();

        CurrentState = BattleState.Intro;
        Announce($"{enemy.displayName} appears! Press {playerSpinKey} to spin!");

        StartCoroutine(EnemyLoop());
    }

    private void Update()
    {
        if (battleOver) return;
        if (CurrentState == BattleState.Intro) return;
        if (playerOnCooldown) return;

        if (Keyboard.current[playerSpinKey].wasPressedThisFrame)
            StartCoroutine(PlayerSpin());
    }

    // --- Player ---

    private IEnumerator PlayerSpin()
    {
        playerOnCooldown = true;
        CurrentState = BattleState.PlayerTurn;

        if (stunnedCombatants.Contains(player))
        {
            Announce($"{player.displayName} is stunned and can't spin!");
            yield return new WaitForSeconds(1f);
            playerOnCooldown = false;
            yield break;
        }

        WheelSlotEffect effect = SpinWheel(player, out int winningIndex);

        bool animDone = false;
        float duration = player.wheel.spinCooldown * spinDurationMultipliers[player];
        battleCanvas.PlayPlayerWheelSpin(winningIndex, player.wheel.slotCount, () => animDone = true, duration);
        yield return new WaitUntil(() => animDone);

        if (battleOver) yield break;

        StatusEffects.NotifySpinCompleted(player);
        ResolveEffect(effect, attacker: player, defender: enemy);

        if (CheckBattleOver()) yield break;

        yield return new WaitForSeconds(player.wheel.spinCooldown);
        playerOnCooldown = false;
        Announce($"Press {playerSpinKey} to spin again!");
    }

    // --- Enemy ---

    // Replace EnemyLoop()'s spin section similarly:
    private IEnumerator EnemyLoop()
    {
        yield return new WaitForSeconds(enemy.wheel.spinCooldown);

        while (!battleOver)
        {
            CurrentState = BattleState.EnemyTurn;

            if (stunnedCombatants.Contains(enemy))
            {
                Announce($"{enemy.displayName} is stunned and can't spin!");
                yield return new WaitForSeconds(enemy.wheel.spinCooldown);
                continue;
            }

            Announce($"{enemy.displayName} spins!");
            WheelSlotEffect effect = SpinWheel(enemy, out int winningIndex);

            bool animDone = false;
            float duration = enemy.wheel.spinCooldown * spinDurationMultipliers[enemy];
            battleCanvas.PlayEnemyWheelSpin(winningIndex, enemy.wheel.slotCount, () => animDone = true, duration);
            yield return new WaitUntil(() => animDone);

            if (battleOver) yield break;

            StatusEffects.NotifySpinCompleted(enemy);
            ResolveEffect(effect, attacker: enemy, defender: player);

            if (CheckBattleOver()) yield break;

            yield return new WaitForSeconds(enemy.wheel.spinCooldown);
        }
    }
    
    private WheelSlotEffect SpinWheel(Combatant combatant, out int winningIndex)
    {
        RiggedStatus rigged = StatusEffects.Get<RiggedStatus>(combatant);

        if (rigged != null && Random.value < 0.5f)
        {
            winningIndex = rigged.riggedSlotIndex;
            StatusEffects.NotifySpinCompleted(combatant); // ticks rigged duration
            return combatant.wheel.slots[winningIndex].effect;
        }

        return combatant.wheel.SpinWithIndex(out winningIndex);
    }

    // --- Resolution ---

    private void ResolveEffect(WheelSlotEffect effect, Combatant attacker, Combatant defender)
    {
        if (effect == null)
        {
            Announce($"{attacker.displayName}'s wheel returned no effect. Skipping.");
            return;
        }
        effect.Execute(attacker, defender, this);
    }

    private bool CheckBattleOver()
    {
        if (enemy.IsDefeated)  { EndBattle(playerWon: true);  return true; }
        if (player.IsDefeated) { EndBattle(playerWon: false); return true; }
        return false;
    }

    private void EndBattle(bool playerWon)
    {
        battleOver = true;
        StopAllCoroutines();
        CurrentState = playerWon ? BattleState.Win : BattleState.Lose;
        Announce(playerWon
            ? $"{player.displayName} wins! {enemy.displayName} is defeated."
            : $"{player.displayName} has been defeated...");
        StartCoroutine(DelayThen(endBattleDelaySeconds, ReturnToOverworld));
    }

    // --- Public hooks for effects ---

    public Combatant GetPlayer() => player;
    public Combatant GetEnemy()  => enemy;

    public void ApplyEffect(WheelSlotEffect effect, Combatant attacker, Combatant defender)
        => ResolveEffect(effect, attacker, defender);
    
    public void SetStunned(Combatant combatant, bool stunned)
    {
        if (stunned) stunnedCombatants.Add(combatant);
        else         stunnedCombatants.Remove(combatant);
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
        if (pendingDeflects.Contains(defender))
        {
            pendingDeflects.Remove(defender);
            Announce($"{defender.displayName} deflects! {attacker.displayName} takes {rawDamage} reflected damage!");
            attacker.TakeDamage(rawDamage);
            NotifyHPChanged(attacker);
            CheckBattleOver();
            return;
        }

        // Vulnerable: double damage, consumed immediately
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

    public void SetPendingDamageReduction(Combatant defender, int amount)
        => pendingDamageReductions[defender] = amount;

    public void EndBattleImmediately(bool playerWon) => EndBattle(playerWon);

    public void Announce(string message) => Debug.Log(message);

    // --- Helpers ---

    private void ReturnToOverworld()
    {
        if (string.IsNullOrEmpty(overworldSceneName))
        {
            Announce("(overworldSceneName not set - staying on Battle scene.)");
            return;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(overworldSceneName);
    }

    private IEnumerator DelayThen(float seconds, System.Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }
    
    public void NotifyHPChanged(Combatant combatant)
    {
        if (combatant == player)
            battleCanvas.UpdatePlayerHP(player.currentHP, player.overhealth);
        else
            battleCanvas.UpdateEnemyHP(enemy.currentHP, enemy.overhealth);
    }
    
    public bool HasPendingDamageReduction(Combatant combatant)
        => pendingDamageReductions.ContainsKey(combatant);
    public void SetPendingDeflect(Combatant combatant)
        => pendingDeflects.Add(combatant);

    public bool HasPendingDeflect(Combatant combatant)
        => pendingDeflects.Contains(combatant);
}