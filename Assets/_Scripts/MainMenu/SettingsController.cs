using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    public static SettingsController Instance;

    [Header("Sound")]
    [SerializeField] private AudioMixer mixer;

    [SerializeField] private Slider masterVolumeSlider, effectsVolumeSlider, musicVolumeSlider;
    [SerializeField] private TMP_Text masterVolumeText, effectsVolumeText, musicVolumeText;

    private const string MasterParam = "MasterVolume";
    private const string EffectsParam = "EffectsVolume";
    private const string MusicParam = "MusicVolume";

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetupVolume(masterVolumeSlider, masterVolumeText, MasterParam);
        SetupVolume(effectsVolumeSlider, effectsVolumeText, EffectsParam);
        SetupVolume(musicVolumeSlider, musicVolumeText, MusicParam);
    }

    private void SetupVolume(Slider slider, TMP_Text label, string param)
    {
        float saved = PlayerPrefs.GetFloat(param, 0.75f);
        slider.value = saved;
        ApplyVolume(param, label, saved);

        slider.onValueChanged.AddListener(value => ApplyVolume(param, label, value));
    }

    private void ApplyVolume(string param, TMP_Text label, float value)
    {
        float db = (value <= 0f) ? -80f : Mathf.Log10(value) * 20f;
        mixer.SetFloat(param, db);
        PlayerPrefs.SetFloat(param, value);
        label.text = $"{Mathf.RoundToInt(value * 100)}%";
    }

    public void ReSetupVolume()
    {
        ApplyVolume(MasterParam, masterVolumeText, masterVolumeSlider.value);
        ApplyVolume(EffectsParam, effectsVolumeText, effectsVolumeSlider.value);
        ApplyVolume(MusicParam, musicVolumeText, musicVolumeSlider.value);
    }
}
