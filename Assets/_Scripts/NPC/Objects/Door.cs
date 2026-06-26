using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    private string nextSceneName;
    [SerializeField]
    private Transform tpPos;

    private void OnTriggerStay(Collider other)
    {
        if (IsActive.ExitDoorCooldown) return;

        if (other.CompareTag("Player"))
        {
            IsActive.StartExitDoorCooldown();
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

    private void Awake()
    {
        if (IsActive.ExitDoorCooldown && PlayerPrefs.GetString("LastScene") == nextSceneName)
        {
            // Fix it later
            //FindAnyObjectByType<PlayerMove>().EnterTheDoor(tpPos.position);
        }
    }
}
