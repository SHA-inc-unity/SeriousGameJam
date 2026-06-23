using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class ObjectAudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup effectsGroup;
    [SerializeField] private AudioClip interactionClip;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.outputAudioMixerGroup = effectsGroup;
        source.playOnAwake = false;
        source.spatialBlend = 1f;
    }

    public void Interaction()
    {
        if (interactionClip != null) source.PlayOneShot(interactionClip);
    }
}