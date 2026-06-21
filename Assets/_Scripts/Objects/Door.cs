using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    private string nextSceneName;

    private void OnTriggerStay(Collider other)
    {
        if (IsActive.ExitDoorCooldown) return;

        if (other.CompareTag("Player"))
        {
            IsActive.StartExitDoorCooldown();
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

    private void Start()
    {
        if (IsActive.ExitDoorCooldown && PlayerPrefs.GetString("LastScene") == nextSceneName)
        {
            FindAnyObjectByType<PlayerMove>().transform.position = transform.position;
        }
    }
}
