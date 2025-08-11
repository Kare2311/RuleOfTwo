using UnityEngine;

public class MovingPlatform1 : MonoBehaviour
{
    public Transform upPosition;   // Empty GameObject marking the top position
    public Transform downPosition; // Empty GameObject marking the bottom position
    public float speed = 2f;

    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = downPosition.position;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    public void MoveUp()
    {
        targetPosition = upPosition.position;
    }
}