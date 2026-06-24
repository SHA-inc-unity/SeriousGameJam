using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerAudioManager))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField]
    private Transform playerTrans;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float speed = 5f;

    [SerializeField] private List<AudioClip> footstepClips;
    [SerializeField] private float stepInterval = 0.4f;

    private PlayerAudioManager audioManager;
    private float stepTimer;
    private AudioClip lastStep;

    private string sceneName;
    private bool isEnterDoor;
    private Vector3 posDoor;

    void Update()
    {
        if (!IsActive.isActive) return;

        Vector3 dir = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) dir += playerTrans.forward;
        if (Keyboard.current.sKey.isPressed) dir -= playerTrans.forward;
        if (Keyboard.current.dKey.isPressed) dir += playerTrans.right;
        if (Keyboard.current.aKey.isPressed) dir -= playerTrans.right;

        dir.y = 0f;
        dir.Normalize();

        rb.MovePosition(rb.position + dir * speed * Time.deltaTime);

        HandleFootsteps(dir);
    }

    private void HandleFootsteps(Vector3 dir)
    {
        if (dir == Vector3.zero)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = stepInterval;
        }
    }

    private void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Count == 0) return;

        AudioClip clip = PickStep();
        audioManager.Play(clip);
    }

    private AudioClip PickStep()
    {
        if (footstepClips.Count == 1) return footstepClips[0];

        AudioClip next;
        do
        {
            next = footstepClips[Random.Range(0, footstepClips.Count)];
        }
        while (next == lastStep);

        lastStep = next;
        return next;
    }

    public void EnterTheDoor(Vector3 pos)
    {
        isEnterDoor = true;
        posDoor = pos;
    }

    void Start()
    {
        audioManager = GetComponent<PlayerAudioManager>();

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