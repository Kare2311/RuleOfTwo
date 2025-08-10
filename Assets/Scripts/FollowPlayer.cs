using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;  // Assign Player1 or Player2 in Inspector
    public Vector3 offset = new Vector3(0, 0, -10); // Camera Z-distance

    void LateUpdate()
    {
        transform.position = target.position + offset;
    }
}