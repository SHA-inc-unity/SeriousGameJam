using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtonController : MonoBehaviour
{
    [SerializeField]
    private Button continueBtn;
    private bool saveExist = false;

    private void Start()
    {
        // I DO NOT KNOW HOW TO DO THIS WITHOUT A SAVE SYSTEM
        continueBtn.interactable = saveExist;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void StartNewGame()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("DialogScene");
    }
}
