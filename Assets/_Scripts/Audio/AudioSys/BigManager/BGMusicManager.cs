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
        {
            Debug.Log($"[Music] StartEntry SKIP (same clip already playing): {entry.clip.name}");
            return;
        }

        Debug.Log($"[Music] StartEntry: {entry.clip.name}. Stopping both sources. A.playing={sourceA.isPlaying}, B.playing={sourceB.isPlaying}");

        currentEntry = entry;
        hasEntry = true;

        if (loopRoutine != null) StopCoroutine(loopRoutine);

        sourceA.Stop();
        sourceB.Stop();
        active = sourceA;

        loopRoutine = StartCoroutine(PlayWithSeamlessLoop(entry));
    }

    private IEnumerator PlayWithSeamlessLoop(MusicEntry entry)
    {
        if (entry.loopMarker <= 0f || entry.loopMarker >= entry.clip.length)
        {
            Debug.LogWarning($"[Music] invalid marker, simple loop. marker={entry.loopMarker}, len={entry.clip.length}");
            active.clip = entry.clip;
            active.volume = maxVolume;
            active.loop = true;
            active.Play();
            yield break;
        }

        Debug.Log($"[Music] seamless start: {entry.clip.name}, marker={entry.loopMarker}, len={entry.clip.length}");

        active.clip = entry.clip;
        active.volume = maxVolume;
        active.loop = false;
        active.Play();

        while (true)
        {
            AudioSource current = active;

            yield return new WaitUntil(() =>
                !current.isPlaying || current.time >= entry.loopMarker);

            AudioSource next = (current == sourceA) ? sourceB : sourceA;

            Debug.Log($"[Music] LOOP SWITCH at time={current.time:F2}. {(current == sourceA ? "A" : "B")}→{(next == sourceA ? "A" : "B")}");

            next.Stop();
            next.clip = entry.clip;
            next.volume = maxVolume;
            next.loop = false;
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