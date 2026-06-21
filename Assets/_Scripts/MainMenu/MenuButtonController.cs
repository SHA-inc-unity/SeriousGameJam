using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtonController : MonoBehaviour
{
    [SerializeField]
    private Button continueBtn;
    [SerializeField]
    private string newGameSceneName;

    private void Start()
    {
        continueBtn.interactable = PlayerPrefs.HasKey("SaveFile");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void StartNewGame()
    {
        PlayerPrefs.DeleteAll();
        SettingsController.Instance.ReSetupVolume();

        SceneManager.LoadScene(newGameSceneName);
    }


    public void ContinueGame()
    {
        SceneManager.LoadScene(newGameSceneName);
    }
}
