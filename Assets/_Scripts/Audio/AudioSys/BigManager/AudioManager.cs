using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private PlayerAudioManager playerAudio;
    private DialogueAudioManager dialogueAudio;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Rescan();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Rescan();
    }

    private void Rescan()
    {
        playerAudio = FindAnyObjectByType<PlayerAudioManager>();
        dialogueAudio = FindAnyObjectByType<DialogueAudioManager>();

        ApplyListenerRouting();
    }

    private void ApplyListenerRouting()
    {
        AudioListener camListener = null;
        if (Camera.main != null)
            camListener = Camera.main.GetComponent<AudioListener>();

        if (playerAudio != null)
        {
            AudioListener playerListener = playerAudio.GetComponent<AudioListener>();
            if (playerListener != null) playerListener.enabled = true;
            if (camListener != null) camListener.enabled = false;
        }
        else
        {
            if (camListener != null) camListener.enabled = true;
        }
    }

    public void PlayDialogueClip(AudioClip clip)
    {
        if (dialogueAudio == null)
            dialogueAudio = FindAnyObjectByType<DialogueAudioManager>();

        if (dialogueAudio != null) dialogueAudio.Play(clip);
    }

    public void StopDialogueClip()
    {
        if (dialogueAudio == null)
            dialogueAudio = FindAnyObjectByType<DialogueAudioManager>();

        if (dialogueAudio != null) dialogueAudio.Stop();
    }
}