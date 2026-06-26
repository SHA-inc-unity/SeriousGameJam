using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueSystemBattle : DialogueSystemBase
{
    [SerializeField] private GameObject heroObj;
    [SerializeField] private TMP_Text heroText;
    [SerializeField] private GameObject enemyObj;
    [SerializeField] private TMP_Text enemyText;
    [SerializeField] private float charDelay = 0.03f;
    [SerializeField] private float timeToReadDelay = 1f;

    protected override float charDelayValue { get => charDelay; }

    public bool IsBattleStepActive { get => playRoutine != null; }

    private List<BattleDialogueLine> lines;
    private int queueIndex;
    private Coroutine playRoutine = null;

    public static DialogueSystemBattle Instance { get; private set; }

    private void Awake()
    {
        IsActive.isInBattleCutscene = false;
        IsActive.isInDialogue = false;
        IsActive.isInPause = false;
        IsActive.dialogueCooldown = false;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        heroObj.SetActive(false);
        enemyObj.SetActive(false);
    }

    public void StartDialogue(BattleDialogueHolder battleDialogue)
    {
        lines = battleDialogue.GetDialogueLines();
        queueIndex = 0;

        IsActive.isInDialogue = true;

        TryAdvanceQueue();
    }

    public bool TriggerByState(BattleState state)
    {
        if (queueIndex >= lines.Count) return IsActive.isInDialogue;
        if (playRoutine != null) return IsActive.isInDialogue;

        BattleDialogueLine head = lines[queueIndex];

        if (head.nextWheelEffect == null && head.nextBattleState == state && state != BattleState.Null)
            PlayFromHead();

        return IsActive.isInDialogue;
    }

    public bool TriggerByEffect(WheelSlotEffect effect)
    {
        if (queueIndex >= lines.Count) return IsActive.isInDialogue;
        if (playRoutine != null) return IsActive.isInDialogue;

        BattleDialogueLine head = lines[queueIndex];

        if (head.nextWheelEffect != null && head.nextWheelEffect == effect)
            PlayFromHead();

        return IsActive.isInDialogue;
    }

    private void PlayFromHead()
    {
        if (playRoutine != null) StopCoroutine(playRoutine);
        playRoutine = StartCoroutine(PlayHeadThenNulls());
    }

    private IEnumerator PlayHeadThenNulls()
    {
        do
        {
            PlayLine(queueIndex);
            queueIndex++;

            yield return new WaitUntil(() => typingRoutine == null);
            yield return new WaitForSeconds(timeToReadDelay);
        }
        while (queueIndex < lines.Count && IsConditionless(lines[queueIndex]) && IsActive.isInDialogue);

        heroObj.SetActive(false);
        enemyObj.SetActive(false);

        if (queueIndex >= lines.Count)
            IsActive.isInDialogue = false;

        playRoutine = null;
    }

    private void TryAdvanceQueue()
    {
        if (queueIndex < lines.Count && IsConditionless(lines[queueIndex]))
            PlayFromHead();
    }

    private bool IsConditionless(BattleDialogueLine line)
    {
        return line.nextWheelEffect == null && line.nextBattleState == BattleState.Null;
    }

    private void PlayLine(int index)
    {
        var line = lines[index];

        bool isHero = (line.who == heroNameStr);

        heroObj.SetActive(isHero);
        enemyObj.SetActive(!isHero);

        if (isHero)
            BeginLine(heroText, line.text, line.voice);
        else
            BeginLine(enemyText, line.text, line.voice);
    }
}