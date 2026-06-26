using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMove : MonoBehaviour
{
    //camera
    [SerializeField] private Camera cam;

    [SerializeField] private float zoom;
    private float currentFOV;

    [SerializeField] private float zoomMultiplier;
    [SerializeField] private float minZoom;
    [SerializeField] private float velocity;
    public float zoomSpeed;

    //

    [SerializeField]
    private Transform playerTrans;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private List<AudioClip> footstepClips;

    private PlayerAudioManager audioManager;
    private AudioClip lastStep;
    private Vector3 startScenePos;

    private bool isEnterDoor;
    private Vector3 posDoor;
    private string sceneName;

    void Start()
    {
        audioManager = GetComponent<PlayerAudioManager>();
        startScenePos = transform.position;

        currentFOV = cam.fieldOfView;
        zoom = cam.fieldOfView;

        sceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastScene", sceneName);

        if (isEnterDoor)
        {
            rb.position = posDoor;
            playerTrans.position = posDoor;
        }
        else
        {
            Load();
        }
    }

    void Update()
    {
        if (IsActive.isInDialogue || IsActive.isInBattleCutscene)
        {
            anim.SetBool("isWalking", false);
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 dir = Vector3.zero;

        bool up = Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed;
        bool down = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;
        bool right = Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;
        bool left = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;

        if (up) dir += playerTrans.forward;
        if (down) dir -= playerTrans.forward;
        if (right) dir += playerTrans.right;
        if (left) dir -= playerTrans.right;

        dir.y = 0f;
        dir.Normalize();

        rb.MovePosition(rb.position + dir * speed * Time.deltaTime);

        if (dir != Vector3.zero)
        {
            cam.fieldOfView = currentFOV;
            zoom = currentFOV;
            velocity = 0f;

            anim.SetBool("isWalking", true);
            anim.SetFloat("InputX", dir.x);
            anim.SetFloat("InputZ", dir.z);

            PlayFootstep();
        }
        else
        {
            anim.SetBool("isWalking", false);
            anim.SetFloat("LastInputX", dir.x);
            anim.SetFloat("LastInputZ", dir.z);

            zoom -= zoomMultiplier;
            zoom = Mathf.Clamp(zoom, minZoom, currentFOV);
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, minZoom, ref velocity, zoomSpeed);
        }
    }

    public void BackToStart()
    {
        transform.position = startScenePos;
    }

    public void EnterTheDoor(Vector3 pos)
    {
        isEnterDoor = true;
        posDoor = pos;
    }

    private void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Count == 0) return;
        if (audioManager == null) return;
        if (audioManager.IsPlaying) return;

        AudioClip clip = PickStep();
        audioManager.PlayStep(clip);
    }

    private AudioClip PickStep()
    {
        if (footstepClips.Count == 1) return footstepClips[0];

        AudioClip next;
        do
        {
            next = footstepClips[UnityEngine.Random.Range(0, footstepClips.Count)];
        }
        while (next == lastStep);

        lastStep = next;
        return next;
    }

    void OnDestroy()
    {
        Save();
    }

    private void Save()
    {
        Vector3 p = playerTrans.position;
        PlayerPrefs.SetFloat("playerX", p.x);
        PlayerPrefs.SetFloat("playerY", p.y);
        PlayerPrefs.SetFloat("playerZ", p.z);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey("playerX")) return;

        Vector3 pos = new Vector3(
            PlayerPrefs.GetFloat("playerX"),
            PlayerPrefs.GetFloat("playerY"),
            PlayerPrefs.GetFloat("playerZ"));

        rb.position = pos;
        playerTrans.position = pos;
    }
}