using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioListener))]
[RequireComponent(typeof(AudioSource))]
public class PlayerAudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup effectsGroup;

    private AudioSource source;

    public bool IsPlaying
    {
        get { return source.isPlaying; }
    }

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.outputAudioMixerGroup = effectsGroup;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    public void Play(AudioClip clip)
    {
        if (clip != null) source.PlayOneShot(clip);
    }

    public void PlayStep(AudioClip clip)
    {
        if (clip == null) return;
        source.clip = clip;
        source.Play();
    }
}