using System;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Serializable] private struct MenuBox
    {
        public string name;
        public GameObject box;
    }
    [SerializeField] private List<MenuBox> boxes;

    private Dictionary<string, GameObject> menuBox;
    private string nowBox;

    void Start()
    {
        BuildDictionary();
        InitBoxes();
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
}
