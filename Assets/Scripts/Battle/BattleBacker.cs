using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleBacker : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("OverWorld");
    }
}
