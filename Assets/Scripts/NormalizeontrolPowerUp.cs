using TMPro;
using UnityEngine;

public class NormalizeControlsPowerUp : MonoBehaviour
{
    [Header("Settings")]
    public float powerupDuration = 10f;
    public GameObject pickupEffect;

    [Header("UI Feedback")]
    public GameObject centerPanel;
    public string message = "Rule #1 broken!";
    public float messageDuration = 2f;

    [Header("Audio")]
    public AudioClip pickupSound;

    private MirrorMovement _mirrorMovement;
    private bool _alreadyCollected;

    void Start()
    {
        _mirrorMovement = FindObjectOfType<MirrorMovement>();
        if (centerPanel != null) centerPanel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_alreadyCollected && other.CompareTag("Player"))
        {
            _alreadyCollected = true;
            ApplyPowerUp();
            Destroy(gameObject); // Immediately destroy the orb
        }
    }

    void ApplyPowerUp()
    {
        // Disable mirror inversion
        if (_mirrorMovement != null)
        {
            _mirrorMovement.SetMirroring(false);
            Invoke("RestoreMirroring", powerupDuration);
        }

        // Visual/Audio feedback
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Show UI message
        if (centerPanel != null)
        {
            centerPanel.GetComponentInChildren<TextMeshProUGUI>().text = message;
            centerPanel.SetActive(true);
            Invoke("HideMessage", messageDuration);
        }
    }

    void HideMessage()
    {
        if (centerPanel != null) centerPanel.SetActive(false);
    }

    void RestoreMirroring()
    {
        if (_mirrorMovement != null)
        {
            _mirrorMovement.SetMirroring(true);
        }
    }
}