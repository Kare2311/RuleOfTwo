using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    public Vector2 minBounds, maxBounds; // Set per-level in Inspector

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        transform.position = pos;
    }
}
