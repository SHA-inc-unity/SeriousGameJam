using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class MainMenuController : MonoBehaviour
{
    [Serializable]
    private struct MenuBox
    {
        public string name;
        public GameObject box;
    }
    [SerializeField] private List<MenuBox> boxes;

    [SerializeField] private AudioMixerGroup effectsGroup;
    [SerializeField] private AudioClip selectClip;
    [SerializeField] private AudioClip changeScreenClip;

    private AudioSource source;
    private Dictionary<string, GameObject> menuBox;
    private string nowBox;

    void Start()
    {
        source = GetComponent<AudioSource>();
        source.outputAudioMixerGroup = effectsGroup;
        source.playOnAwake = false;
        source.spatialBlend = 0f;

        BuildDictionary();
        InitBoxes();
        HookSelectSound();
    }

    private void HookSelectSound()
    {
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (var button in allButtons)
        {
            ButtonSelectSound handler = button.gameObject.GetComponent<ButtonSelectSound>();
            if (handler == null)
                handler = button.gameObject.AddComponent<ButtonSelectSound>();

            handler.Init(PlaySelect);
        }
    }

    private void BuildDictionary()
    {
        menuBox = new Dictionary<string, GameObject>();
        foreach (var item in boxes)
        {
            menuBox[item.name] = item.box;
        }
    }

    public void SetBox(string boxName)
    {
        menuBox[boxName].SetActive(true);
        menuBox[nowBox].SetActive(false);
        nowBox = boxName;

        PlayChangeScreen();
    }

    public void PlaySelect()
    {
        if (selectClip != null) source.PlayOneShot(selectClip);
    }

    public void PlayChangeScreen()
    {
        if (changeScreenClip != null) source.PlayOneShot(changeScreenClip);
    }

    private void InitBoxes()
    {
        int i = 0;
        foreach (var item in menuBox)
        {
            if (i == 0)
            {
                nowBox = item.Key;
                item.Value.SetActive(true);
            }
            else item.Value.SetActive(false);
            i++;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}