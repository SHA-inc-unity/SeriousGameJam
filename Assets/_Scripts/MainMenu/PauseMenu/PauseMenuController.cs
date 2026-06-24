using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class PauseMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseMenu;

    [SerializeField] private AudioMixerGroup effectsGroup;
    [SerializeField] private AudioClip selectClip;
    [SerializeField] private AudioClip changeScreenClip;

    private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        source.outputAudioMixerGroup = effectsGroup;
        source.playOnAwake = false;
        source.spatialBlend = 0f;

        ChangeMenuActive(false);
    }

    private void Update()
    {
        if ((IsActive.isActive || IsActive.isInPause) && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ChangeMenuActive();
        }
    }

    private void ChangeMenuActive(bool active)
    {
        IsActive.isInPause = active;
        pauseMenu.SetActive(active);
    }

    public void ChangeMenuActive()
    {
        PlayChangeScreen();
        ChangeMenuActive(!IsActive.isInPause);
    }

    public void PlaySelect()
    {
        if (selectClip != null) source.PlayOneShot(selectClip);
    }

    public void PlayChangeScreen()
    {
        if (changeScreenClip != null) source.PlayOneShot(changeScreenClip);
    }

    public void ExitMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}