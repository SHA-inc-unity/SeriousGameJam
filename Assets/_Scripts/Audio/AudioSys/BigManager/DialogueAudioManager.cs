using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class DialogueAudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup effectsGroup;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.outputAudioMixerGroup = effectsGroup;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        source.clip = clip;
        source.Play();
    }

    public void Stop() => source.Stop();
}