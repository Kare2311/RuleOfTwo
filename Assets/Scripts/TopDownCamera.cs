using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform player;  // Assign your player in Inspector
    public float height = 10f;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, 0);

    [Header("Zoom")]
    public float minHeight = 5f;
    public float maxHeight = 15f;
    public float zoomSpeed = 2f;

    void Start()
    {
        if (player == null)
        {
            // Auto-find player if not assigned
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Calculate target position (directly above player)
        Vector3 targetPosition = player.position + offset + Vector3.up * height;

        // Smooth movement
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        // Always look directly down at player
        transform.rotation = Quaternion.Euler(90f, 0, 0);

        // Optional zoom control (comment out if not needed)
        HandleZoom();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        height = Mathf.Clamp(height - scroll * zoomSpeed, minHeight, maxHeight);
    }
}