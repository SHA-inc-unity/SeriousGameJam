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
        Vector3 dir = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) dir += playerTrans.forward;
        if (Keyboard.current.sKey.isPressed) dir -= playerTrans.forward;
        if (Keyboard.current.dKey.isPressed) dir += playerTrans.right;
        if (Keyboard.current.aKey.isPressed) dir -= playerTrans.right;

        dir.y = 0f;
        dir.Normalize();

        rb.MovePosition(rb.position + dir * speed * Time.deltaTime);
    }
}