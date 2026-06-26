using UnityEngine;

public class BackManagerCube : MonoBehaviour
{
    [SerializeField]
    private BackManager backManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMove player = other.GetComponentInParent<PlayerMove>();
            if (player != null && !backManager.isFading)
                StartCoroutine(backManager.FadeAndTeleport(player));
        }
    }
}
