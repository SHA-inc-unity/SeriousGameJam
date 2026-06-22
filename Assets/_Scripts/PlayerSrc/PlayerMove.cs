using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMove : MonoBehaviour
{
    [SerializeField]
    private Transform playerTrans;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float speed = 5f;

    private string sceneName;
    private bool isEnterDoor;
    private Vector3 posDoor;


    void Update()
    {
        if(!IsActive.isActive) return;

        Vector3 dir = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) dir += playerTrans.forward;
        if (Keyboard.current.sKey.isPressed) dir -= playerTrans.forward;
        if (Keyboard.current.dKey.isPressed) dir += playerTrans.right;
        if (Keyboard.current.aKey.isPressed) dir -= playerTrans.right;

        dir.y = 0f;
        dir.Normalize();

        rb.MovePosition(rb.position + dir * speed * Time.deltaTime);
    }

    public void EnterTheDoor(Vector3 pos)
    {
        isEnterDoor = true;
        posDoor = pos;
    }




    // Rewrite this shit plz


    void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;

        PlayerPrefs.SetString("LastScene", sceneName);

        if (isEnterDoor)
        {
            transform.position = posDoor;
        }
        else
        {
            if (PlayerPrefs.HasKey("SaveFile"))
            {
                Load();
            }
            else
            {
                PlayerPrefs.SetInt("SaveFile", 1);
                PlayerPrefs.Save();
            }
        }
    }

    void OnDestroy()
    {
        Save();
    }

    private void Save()
    {
        Vector3 p = playerTrans.position;
        PlayerPrefs.SetFloat($"playerX/{sceneName}", p.x);
        PlayerPrefs.SetFloat($"playerY/{sceneName}", p.y);
        PlayerPrefs.SetFloat($"playerZ/{sceneName}", p.z);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey($"playerX/{sceneName}")) return;

        Vector3 pos = new Vector3(
            PlayerPrefs.GetFloat($"playerX/{sceneName}"),
            PlayerPrefs.GetFloat($"playerY/{sceneName}"),
            PlayerPrefs.GetFloat($"playerZ/{sceneName}"));

        rb.position = pos;
        playerTrans.position = pos;
    }
}