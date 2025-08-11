using UnityEngine;

public class MovingSaw : MonoBehaviour
{
    public float moveDistance = 3f; // How far it moves left/right
    public float speed = 2f; // Movement speed
    private Vector3 startPosition;
    private bool movingRight = true;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Calculate movement
        float movement = speed * Time.deltaTime;

        if (movingRight)
        {
            transform.Translate(Vector3.right * movement);
            if (Vector3.Distance(startPosition, transform.position) >= moveDistance)
                movingRight = false;
        }
        else
        {
            transform.Translate(Vector3.left * movement);
            if (Vector3.Distance(startPosition, transform.position) <= 0.1f)
                movingRight = true;
        }
    }
}