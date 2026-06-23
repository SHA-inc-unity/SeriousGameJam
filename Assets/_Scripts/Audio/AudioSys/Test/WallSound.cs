using UnityEngine;

public class WallSound : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<PlayerMove>() != null)
        {
            Debug.Log("wall triggered");
            GetComponent<ObjectAudioManager>().Interaction();
        }
    }
}
