using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class BGMusicManager : MonoBehaviour
{
    public static BGMusicManager Instance { get; private set; }

    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private MusicLibrary library;
    [SerializeField] private float maxVolume = 1.0f;
    [SerializeField] private string menuSceneName = "MainMenu";

    private AudioSource sourceA;
    private AudioSource sourceB;
    private AudioSource active;

    private MusicUsage currentMode = MusicUsage.None;
    private MusicEntry currentEntry;
    private bool hasEntry;
    private bool forcedTrackActive;

    private Coroutine loopRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sourceA = GetComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();
        ConfigureSource(sourceA);
        ConfigureSource(sourceB);
        active = sourceA;
    }

    private void ConfigureSource(AudioSource s)
    {
        s.outputAudioMixerGroup = musicGroup;
        s.loop = false;
        s.playOnAwake = false;
        s.spatialBlend = 0f;
        s.volume = maxVolume;
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
        UpdateModeForScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        forcedTrackActive = false;
        UpdateModeForScene();
    }

    private bool IsBattleScene()
    {
        return FindAnyObjectByType<BattleManager>() != null;
    }

    private void UpdateModeForScene()
    {
        if (forcedTrackActive) return;

        MusicUsage mode;
        if (SceneManager.GetActiveScene().name == menuSceneName)
            mode = MusicUsage.Menu;
        else if (IsBattleScene())
            mode = MusicUsage.Battle;
        else
            mode = MusicUsage.Overworld;

        if (mode == currentMode && active.isPlaying) return;

        currentMode = mode;

        List<MusicEntry> pool = library.GetEntriesFor(currentMode);
        if (pool.Count == 0)
        {
            Debug.LogWarning($"MusicLibrary: no tracks for mode {currentMode}");
            return;
        }

        MusicEntry pick = PickNonRepeating(pool);
        StartEntry(pick);
    }

    public void PlayTrackForEnemy(string enemyName)
    {
        if (library.TryGetEntryForEnemy(enemyName, out MusicEntry entry))
        {
            forcedTrackActive = true;
            currentMode = MusicUsage.Battle;
            StartEntry(entry);
        }
        else
        {
            Debug.LogWarning($"MusicLibrary: no track found for enemy '{enemyName}', falling back to Battle pool.");
            forcedTrackActive = false;
            UpdateModeForScene();
        }
    }

    private void StartEntry(MusicEntry entry)
    {
        if (hasEntry && currentEntry.clip == entry.clip && active.isPlaying)
            return;

        currentEntry = entry;
        hasEntry = true;

        if (loopRoutine != null) StopCoroutine(loopRoutine);
        loopRoutine = StartCoroutine(PlayWithSeamlessLoop(entry));
    }

    private IEnumerator PlayWithSeamlessLoop(MusicEntry entry)
    {
        active.clip = entry.clip;
        active.volume = maxVolume;
        active.Play();

        while (true)
        {
            yield return new WaitUntil(() =>
                !active.isPlaying || active.time >= entry.loopMarker);

            AudioSource next = (active == sourceA) ? sourceB : sourceA;

            next.clip = entry.clip;
            next.volume = maxVolume;
            next.Play();

            active = next;

            yield return null;
        }
    }

    private MusicEntry PickNonRepeating(List<MusicEntry> pool)
    {
        if (pool.Count == 1) return pool[0];

        MusicEntry next;
        do
            next = pool[Random.Range(0, pool.Count)];
        while (hasEntry && next.clip == currentEntry.clip);

        return next;
    }
}