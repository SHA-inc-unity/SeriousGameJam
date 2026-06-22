using UnityEngine;
using UnityEngine.InputSystem;

public class cameraZoom : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [SerializeField] private float zoom;
    [SerializeField] private float zoomMultiplier;
    [SerializeField] private float minZoom;
    [SerializeField] private float velocity;
    public float zoomSpeed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        zoom = cam.fieldOfView;

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = Vector3.zero;


        if (dir != Vector3.zero)
        {

            cam.fieldOfView = 60;

        }
        else if (dir == Vector3.zero)
        {
            Debug.Log("hi");
            zoom -= zoomMultiplier;
            zoom = Mathf.Clamp(zoom, minZoom, zoom);
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, zoom, ref velocity, zoomSpeed);
        }
    }


}
