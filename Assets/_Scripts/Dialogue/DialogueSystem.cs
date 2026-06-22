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
    private float charDelay = 0.03f;

    private List<DialogueLine> valuesIn;
    private int nEnd, n = 0;
    private Coroutine typingRoutine;
    private string currentText;

    // No longer fetched via GetComponent (that only checked THIS GameObject,
    // which is wrong - BattleTrigger lives on the NPC, not on DialogueSystem).
    // Instead, whoever starts the dialogue passes in that NPC's own trigger.
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
        // in a good way, this needs to be redone for an event
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
            if(typingRoutine != null)
                SkipTyping();
            else
                PlayNextDialogue();
        }
    }

    // NEW PARAM: battleTrigger - pass in the NPC's own BattleTrigger here
    // (see NPCInteract.cs). Defaults to null so this still compiles fine
    // anywhere that calls StartDialogue without a trigger (NPCs that never
    // start a battle don't need one).
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
                Debug.LogError("This dialogue answer has goToBattle checked, but no BattleTrigger " +
                               "was passed in to StartDialogue(). Check NPCInteract has a BattleTrigger assigned.");
                return;
            }
            trigger.StartBattle();
        }
        else if (answer.nextDialogue != null)
        {
            // Pass the same trigger along, in case the NEXT dialogue chunk
            // also has a goToBattle answer further down the conversation.
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

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        text.text = "";
        foreach (char c in currentText)
        {
            text.text += c;
            yield return new WaitForSeconds(charDelay);
        }
        typingRoutine = null;
    }

    private void SkipTyping()
    {
        StopCoroutine(typingRoutine);
        typingRoutine = null;
        text.text = currentText;
    }
}