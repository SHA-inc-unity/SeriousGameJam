using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueSystemBattle : DialogueSystemBase
{
    [SerializeField] private GameObject heroObj;
    [SerializeField] private TMP_Text heroText;
    [SerializeField] private TMP_Text heroNameField;
    [SerializeField] private GameObject enemyObj;
    [SerializeField] private TMP_Text enemyText;
    [SerializeField] private TMP_Text enemyNameField;
    [SerializeField] private float charDelay = 0.03f;
    [SerializeField] private float timeToReadDelay = 1f;

    protected override float charDelayValue { get => charDelay; }

    public bool IsBattleStepActive { get => battleStepRoutine != null; }

    private List<BattleDialogueLine> valuesInBattle;
    private BattleState nextBattleState;
    private int n = 0;
    private Coroutine battleStepRoutine = null;

    public static DialogueSystemBattle Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        heroNameField.text = heroNameStr;

        heroObj.SetActive(false);
        enemyObj.SetActive(false);
    }

    public void StartDialogue(BattleDialogueHolder battleDialogue)
    {
        valuesInBattle = battleDialogue.GetDialogueLines();

        SetEnemyName();

        IsActive.isInDialogue = true;
        n = 0;

        nextBattleState = valuesInBattle[0].nextBattleState;
        TriggerPlayDialogue(BattleState.Null);
    }

    private void SetEnemyName()
    {
        foreach (var line in valuesInBattle)
        {
            if (line.who != heroNameStr)
            {
                enemyNameField.text = line.who;
                return;
            }
        }
    }

    public bool TriggerPlayDialogue(BattleState battleState)
    {
        if (nextBattleState == battleState)
        {
            if (battleStepRoutine != null) StopCoroutine(battleStepRoutine);
            battleStepRoutine = StartCoroutine(PlayBattleSteps());
        }

        return IsActive.isInDialogue;
    }

    private IEnumerator PlayBattleSteps()
    {
        do
        {
            PlayNextBattleDialogue();

            yield return new WaitUntil(() => typingRoutine == null);

            if (n < valuesInBattle.Count) nextBattleState = valuesInBattle[n].nextBattleState;

            yield return new WaitForSeconds(timeToReadDelay);
        }
        while (nextBattleState == BattleState.Null && IsActive.isInDialogue);

        heroObj.SetActive(false);
        enemyObj.SetActive(false);

        battleStepRoutine = null;
    }

    private void PlayNextBattleDialogue()
    {
        if (n < valuesInBattle.Count)
        {
            PlayBattleDialogue(n);
            n++;
        }
        else
        {
            IsActive.isInDialogue = false;
            heroObj.SetActive(false);
            enemyObj.SetActive(false);
        }
    }

    private void PlayBattleDialogue(int n)
    {
        var line = valuesInBattle[n];

        bool isHero = (line.who == heroNameStr);

        heroObj.SetActive(isHero);
        enemyObj.SetActive(!isHero);

        if (isHero)
            BeginLine(heroText, line.text, line.voice);
        else
            BeginLine(enemyText, line.text, line.voice);
    }
}