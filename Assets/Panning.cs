using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panning : MonoBehaviour
{
    public float scrollPanSpeed = 0.5f;
    public float panSpeed = 20f;
    public float zoomSpeed = 5f;
    public bool enableCameraMovement = false;

    private Camera cam;

    void Start()
    {
        cam = Camera.main; // Assuming this script is attached to the main camera
    }

    void Update()
    {
        if (enableCameraMovement)
        {
            
            PanCamera();
            ZoomCamera();
        }
    }

    void PanCamera()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.DownArrow))
        {
            float moveX = Input.GetAxis("Horizontal") * panSpeed * Time.deltaTime;
            float moveY = Input.GetAxis("Vertical") * panSpeed * Time.deltaTime;

            transform.Translate(moveX, moveY, 0);
        }
    }

    private Vector3 lastMousePosition;
    private Vector3 startPos;
    // void ScrollToPan()
    // {
    //     if (Input.GetMouseButtonDown(2)) // Middle mouse button pressed
    //     {
    //         lastMousePosition = Input.mousePosition; // Store mouse position
    //         startPos = transform.position;
    //     }
    //     if (Input.GetMouseButton(2)) // Middle mouse button held down
    //     {
    //         Vector3 delta = Input.mousePosition - lastMousePosition; // Difference in mouse position
    //         // delta *= scrollPanSpeed; // Apply panning speed
    //         transform.position = startPos + delta*panSpeed; // Move camera
    //         // lastMousePosition = Input.mousePosition; // Update last mouse position
    //     }
    // }

    void ZoomCamera()
    {
        float zoomChange = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        cam.orthographicSize -= zoomChange;
        cam.orthographicSize = Mathf.Max(cam.orthographicSize, 5f); // Prevent the camera from zooming too close
    }
}
