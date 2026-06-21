using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    public static SettingsController Instance;

    [Header("Sound")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumeText;

    private string pharm = "MasterVolume";

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetupVolume();
    }

    // Volume

    private void SetupVolume()
    {
        float saved = PlayerPrefs.GetFloat(pharm, 0.75f);
        volumeSlider.value = saved;
        SetVolume(saved);
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void ReSetupVolume()
    {
        SetVolume(volumeSlider.value);
    }

    public void SetVolume(float value)
    {
        float realValue = 0;
        if (value == 0)
            realValue = -80;
        else
            realValue = Mathf.Log10(value) * 20f;
        mixer.SetFloat(pharm, realValue);
        PlayerPrefs.SetFloat(pharm, value);
        volumeText.text = $"{value*100}%";
    }
}
