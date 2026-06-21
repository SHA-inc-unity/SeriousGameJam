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

    public void StartDialogue(DialogueHolder values, int nStart = 0, int nEnd = -1)
    {
        valuesIn = values.GetDialogueLines();

        if (nEnd < 0) nEnd = valuesIn.Count;

        IsActive.isInDialogue = true;

        this.nEnd = nEnd;

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
            // Will need to add an upload of enemy and character characteristics
            SceneManager.LoadScene("BattleScene");
        }
        else if (answer.nextDialogue != null)
        {
            StartDialogue(answer.nextDialogue);
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
