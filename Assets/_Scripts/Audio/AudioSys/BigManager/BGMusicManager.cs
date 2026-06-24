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
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float maxVolume = 1.0f;

    private AudioSource source;
    private MusicUsage currentMode = MusicUsage.None;
    private AudioClip lastClip;
    private Coroutine fadeRoutine;
    private bool forcedTrackActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        source.outputAudioMixerGroup = musicGroup;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
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
        MusicUsage mode;
        if (IsBattleScene())
            mode = MusicUsage.Battle;
        else
            mode = MusicUsage.Overworld;

        if (mode == currentMode && source.isPlaying)
            return;

        currentMode = mode;
        PlayRandomForMode();
    }

    public void PlayForcedBattleTrack(AudioClip clip)
    {
        if (clip == null) return;

        forcedTrackActive = true;
        currentMode = MusicUsage.Battle;

        if (source.clip == clip && source.isPlaying) return;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(CrossFadeTo(clip));
    }

    private void PlayRandomForMode()
    {
        if (forcedTrackActive) return;

        List<AudioClip> pool = library.GetClipsFor(currentMode);
        if (pool.Count == 0)
        {
            Debug.LogWarning($"MusicLibrary: no tracks for mode {currentMode}");
            return;
        }

        AudioClip next = PickNonRepeating(pool);

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(CrossFadeTo(next));
    }

    private AudioClip PickNonRepeating(List<AudioClip> pool)
    {
        if (pool.Count == 1)
            return pool[0];

        AudioClip next;
        do
            next = pool[Random.Range(0, pool.Count)];
        while (next == lastClip);

        return next;
    }

    private IEnumerator CrossFadeTo(AudioClip next)
    {
        if (source.isPlaying)
        {
            float t = 0f;
            float startVol = source.volume;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                source.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
                yield return null;
            }
        }

        source.clip = next;
        lastClip = next;
        source.volume = 0f;
        source.Play();

        float t2 = 0f;
        while (t2 < fadeDuration)
        {
            t2 += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, maxVolume, t2 / fadeDuration);
            yield return null;
        }
        source.volume = maxVolume;

        fadeRoutine = null;
    }

    public void PlayNext()
    {
        PlayRandomForMode();
    }
}