using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class PlayerCameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    [Header("Rotation")]
    public float rotateSpeed = 100f;
    [Header("Zoom (Orthographic Size)")]
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 20f;

    Camera cam;
    float currentZoom;
    Keyboard kb;
    Mouse mouse;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        currentZoom = cam.orthographicSize;

        kb = Keyboard.current;
        mouse = Mouse.current;
    }

    void Update()
    {
        if (kb == null || mouse == null) return;

        // — WASD move
        Vector3 delta = Vector3.zero;
        if (kb.wKey.isPressed) delta += transform.up;
        if (kb.sKey.isPressed) delta -= transform.up;
        if (kb.dKey.isPressed) delta += transform.right;
        if (kb.aKey.isPressed) delta -= transform.right;
        transform.position += delta * moveSpeed * Time.deltaTime;

        // — Q/E rotate
        float rot = 0f;
        if (kb.qKey.isPressed) rot += rotateSpeed * Time.deltaTime;
        if (kb.eKey.isPressed) rot -= rotateSpeed * Time.deltaTime;
        transform.Rotate(0f, 0f, rot);

        // — scroll wheel zoom
        float scroll = mouse.scroll.y.ReadValue();
        if (Mathf.Abs(scroll) > 0.001f)
        {
            currentZoom = Mathf.Clamp(currentZoom - scroll * zoomSpeed, minZoom, maxZoom);
            cam.orthographicSize = currentZoom;
        }
    }
}
