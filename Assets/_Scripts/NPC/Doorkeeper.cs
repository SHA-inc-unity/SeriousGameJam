using System.Collections;
using UnityEngine;

public class DoorKeeperMover : MonoBehaviour
{
    [SerializeField] private Door door;
    [SerializeField] private Transform goOff;
    [SerializeField] private float moveDuration = 1f;

    private bool hasMoved = false;

    private void Update()
    {
        if (hasMoved) return;

        if (door.IsOpen)
        {
            hasMoved = true;
            StartCoroutine(MoveAway());
        }
    }

    private IEnumerator MoveAway()
    {
        Vector3 from = transform.position;
        Vector3 to = goOff.position;
        float t = 0f;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, t / moveDuration);
            yield return null;
        }

        transform.position = to;

        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null) box.enabled = false;
    }
}