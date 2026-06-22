using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseMenu;

    private void Start()
    {
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
        ChangeMenuActive(!IsActive.isInPause);
    }

    public void ExitMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
