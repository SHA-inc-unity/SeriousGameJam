using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueSystem : DialogueSystemBase
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private TMP_Text heroName, nonHeroName;
    [SerializeField] private GameObject obj;
    [SerializeField] private Button[] answerButtons;
    [SerializeField] private float charDelay = 0.03f;

    protected override float charDelayValue { get => charDelay; }

    private List<DialogueLine> valuesIn;
    private int nEnd, n = 0;
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
            if (obj.activeSelf == false) obj.SetActive(true);
        }
        else
        {
            if (obj.activeSelf == true) obj.SetActive(false);
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (typingRoutine != null) SkipTyping();
            else PlayNextDialogue();
        }
    }

    public void StartDialogue(DialogueHolder values, BattleTrigger battleTrigger = null, int nStart = 0, int nEnd = -1)
    {
        valuesIn = values.GetDialogueLines();

        if (nEnd < 0) nEnd = valuesIn.Count;

        IsActive.isInDialogue = true;

        this.nEnd = nEnd;
        trigger = battleTrigger;

        n = nStart;
        PlayDialogue(nStart);
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
                Debug.LogError("This dialogue answer has goToBattle checked, but no BattleTrigger was passed in to StartDialogue(). Check NPCInteract has a BattleTrigger assigned.");
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

        BeginLine(text, line.text, line.voice);
    }
}