using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioListener))]
[RequireComponent(typeof(AudioSource))]
public class PlayerAudioManager : MonoBehaviour
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
        if (clip != null) source.PlayOneShot(clip);
    }
}