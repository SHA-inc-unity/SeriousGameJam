using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;
    [SerializeField]
    private string heroNameStr;
    [SerializeField]
    private TMP_Text heroName, nonHeroName;
    [SerializeField]
    private Button[] answerButtons;
    [SerializeField]
    private GameObject obj;
    [SerializeField]
    private float charDelay = 0.03f, timeToReadDelay = 1f;

    public bool IsBattleStepActive { get => battleStepRoutine != null; }

    private List<DialogueLine> valuesIn;
    private List<BattleDialogueLine> valuesInBattle;
    private BattleState nextBattleState;
    private int nEnd, n = 0;
    private Coroutine typingRoutine;
    private string currentText;
    private AudioClip currentClip;
    private Coroutine battleStepRoutine = null;
    private DialogueHolder currentHolder;

    private BattleTrigger trigger;

    public static DialogueSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        heroName.text = heroNameStr;
    }

    private void Update()
    {
        if (IsActive.isInBattleCutscene) return;

        if (IsActive.isInDialogue)
        {
            if (obj.activeSelf == false)
                obj.SetActive(true);
        }
        else
        {
            if (obj.activeSelf == true)
                obj.SetActive(false);
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (typingRoutine != null)
                SkipTyping();
            else
                PlayNextDialogue();
        }
    }

    public void StartDialogue(DialogueHolder values, BattleTrigger battleTrigger = null, int nStart = 0, int nEnd = -1)
    {
        // Respect the canRepeat flag — bail out silently if already played
        if (!values.canRepeat && values.hasPlayed) return;

        currentHolder = values;
        valuesIn = values.GetDialogueLines();

        if (nEnd < 0) nEnd = valuesIn.Count;

        IsActive.isInDialogue = true;

        this.nEnd = nEnd;
        trigger = battleTrigger;

        n = nStart;
        PlayDialogue(nStart);
    }

    public void StartDialogue(BattleDialogueHolder battleDialogue)
    {
        valuesInBattle = battleDialogue.GetDialogueLines();

        nEnd = valuesInBattle.Count;
        IsActive.isInDialogue = true;
        n = 0;

        nextBattleState = valuesInBattle[0].nextBattleState;
        TriggerPlayDialogue(BattleState.Null);
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
        obj.SetActive(true);

        do
        {
            PlayNextBattleDialogue();

            yield return new WaitUntil(() => typingRoutine == null);

            if (n < valuesInBattle.Count) nextBattleState = valuesInBattle[n].nextBattleState;

            yield return new WaitForSeconds(timeToReadDelay);
        }
        while (nextBattleState == BattleState.Null && IsActive.isInDialogue);

        obj.SetActive(false);

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
            obj.SetActive(false);
        }
    }

    private void PlayBattleDialogue(int n)
    {
        var line = valuesInBattle[n];

        bool isHero = (line.who == heroNameStr);
        heroName.gameObject.SetActive(isHero);
        nonHeroName.gameObject.SetActive(!isHero);
        if (!isHero)
            nonHeroName.text = line.who;

        foreach (var b in answerButtons)
            b.gameObject.SetActive(false);

        currentText = line.text;
        currentClip = line.voice;

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeText());
    }

    private void PlayNextDialogue()
    {
        if (valuesIn[n].answers.Count > 0) return;

        if (n < nEnd - 1)
        {
            n++;
            PlayDialogue(n);
        }
        else
        {
            IsActive.isInDialogue = false;

            if (currentHolder != null)
                currentHolder.hasPlayed = true;

            // Trigger battle on end if flagged — requires a BattleTrigger to have been passed in
            if (currentHolder != null && currentHolder.triggerBattleOnEnd)
            {
                if (trigger != null)
                    trigger.StartBattle();
                else
                    Debug.LogError($"DialogueHolder '{currentHolder.name}' has triggerBattleOnEnd set " +
                                   "but no BattleTrigger was passed into StartDialogue().");
            }

            StartCoroutine(DialogueCooldown());
        }
    }

    public void PlayNextDialogueAnswer(int answerIndex)
    {
        var answer = valuesIn[n].answers[answerIndex];

        if (answer.goToBattle)
        {
            if (trigger == null)
            {
                Debug.LogError("This dialogue answer has goToBattle checked, but no BattleTrigger " +
                               "was passed in to StartDialogue(). Check NPCInteract has a BattleTrigger assigned.");
                return;
            }
            trigger.StartBattle();
        }
        else if (answer.nextDialogue != null)
        {
            StartDialogue(answer.nextDialogue, trigger);
        }
        else
        {
            if (n < nEnd - 1) { n++; PlayDialogue(n); }
            else { IsActive.isInDialogue = false; StartCoroutine(DialogueCooldown()); }
        }
    }

    private IEnumerator DialogueCooldown()
    {
        IsActive.dialogueCooldown = true;
        yield return new WaitForSeconds(0.5f);
        IsActive.dialogueCooldown = false;
    }

    private void PlayDialogue(int n)
    {
        var line = valuesIn[n];

        bool isHero = (line.who == heroNameStr);
        heroName.gameObject.SetActive(isHero);
        nonHeroName.gameObject.SetActive(!isHero);
        if (!isHero)
            nonHeroName.text = line.who;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            bool hasAnswer = i < line.answers.Count;
            answerButtons[i].gameObject.SetActive(hasAnswer);
            if (hasAnswer)
                answerButtons[i].GetComponentInChildren<TMP_Text>().text = line.answers[i].text;
        }

        currentText = line.text;
        currentClip = line.voice;

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        text.text = "";

        float delay = charDelay;

        if (currentClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDialogueClip(currentClip);
            if (currentText.Length > 0)
                delay = currentClip.length / currentText.Length;
        }

        foreach (char c in currentText)
        {
            text.text += c;
            yield return new WaitForSeconds(delay);
        }
        typingRoutine = null;
    }

    private void SkipTyping()
    {
        StopCoroutine(typingRoutine);
        typingRoutine = null;
        text.text = currentText;

        if (AudioManager.Instance != null)
            AudioManager.Instance.StopDialogueClip();
    }
}