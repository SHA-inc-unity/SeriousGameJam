using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField]
    private Transform playerTrans;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float speed = 5f;


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




    // Rewrite this shit plz


    void Start()
    {
        Load();
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