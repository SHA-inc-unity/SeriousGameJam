using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class BattleAudio : MonoBehaviour
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

    public void PlayClip(AudioClip clip)
    {
        if (clip != null)
            source.PlayOneShot(clip);
    }

    public void PlayState(BattleSoundSet set, BattleState state)
    {
        if (set == null) return;

        AudioClip clip = set.GetClip(state);
        if (clip != null)
            source.PlayOneShot(clip);
    }

    public void StartWheelSpin(BattleSoundSet set)
    {
        if (set == null) return;
        AudioClip clip = set.GetWheelSpinClip();
        if (clip == null) return;

        source.loop = true;
        source.clip = clip;
        source.Play();
    }

    public void StopWheelSpin()
    {
        source.loop = false;
        source.Stop();
    }
}