using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public MovingPlatform1 platform; // Assign in Inspector
    private bool activated = false;
    public float delayBeforeMove = 1f; // Seconds to wait before moving up

    void OnTriggerEnter(Collider other)
    {
        if (!activated && other.CompareTag("Player"))
        {
            activated = true;
            StartCoroutine(ActivatePlatform());
        }
    }

    private System.Collections.IEnumerator ActivatePlatform()
    {
        yield return new WaitForSeconds(delayBeforeMove);
        platform.MoveUp();
    }
}