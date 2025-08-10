using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class ExitDoor : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject interactPrompt;  // Assign a World Space Canvas with Text+CanvasGroup
    public float promptHeight = 2f;    // How high above player to show prompt
    public float fadeSpeed = 5f;       // How fast prompt appears/disappears

    [Header("Door Settings")]
    public KeyCode interactKey = KeyCode.E;
    public string nextLevelName = "Level2";
    public float interactionRadius = 3f;

    [Header("Feedback")]
    public AudioClip openSound;
    public Animator doorAnimator;
    public ParticleSystem openEffect;

    

    [SerializeField] Transform _player;
    private CanvasGroup _promptGroup;
    private bool _isPlayerNear;
    private bool _isInteracting;

    void Start()
    {
        // Initialize player reference
        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (_player == null) Debug.LogError("Player not found!");
        }

        // Initialize UI
        if (interactPrompt != null)
        {
            _promptGroup = interactPrompt.GetComponent<CanvasGroup>() ?? interactPrompt.AddComponent<CanvasGroup>();
            _promptGroup.alpha = 0;
            interactPrompt.SetActive(false);

            // Set initial text
            Text promptText = interactPrompt.GetComponentInChildren<Text>();
            if (promptText != null) promptText.text = "Press 'E' to exit";
        }

        
        
    }

    

    void Update()
    {
        if (_isInteracting || _player == null || interactPrompt == null) return;

        // Check distance to door
        float distance = Vector3.Distance(transform.position, _player.position);
        bool wasPlayerNear = _isPlayerNear;
        _isPlayerNear = distance <= interactionRadius;

        // Only update when state changes
        if (_isPlayerNear != wasPlayerNear)
        {
            if (_isPlayerNear)
                StartCoroutine(ShowPrompt());
            else
                StartCoroutine(HidePrompt());
        }

        // Position prompt above player's head
        if (_isPlayerNear)
        {
            interactPrompt.transform.position = _player.position + Vector3.up * promptHeight;
            interactPrompt.transform.LookAt(Camera.main.transform);
            interactPrompt.transform.rotation = Quaternion.Euler(0, interactPrompt.transform.eulerAngles.y + 180, 0);
        }

        // Handle interaction
        if (_isPlayerNear && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(OpenDoor());
        }
    }

    IEnumerator ShowPrompt()
    {
        interactPrompt.SetActive(true);
        float targetAlpha = 1f;

        while (_promptGroup.alpha < targetAlpha)
        {
            _promptGroup.alpha = Mathf.MoveTowards(_promptGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }


    IEnumerator HidePrompt()
    {
        float targetAlpha = 0f;

        while (_promptGroup.alpha > targetAlpha)
        {
            _promptGroup.alpha = Mathf.MoveTowards(_promptGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }

        interactPrompt.SetActive(false);
    }

    IEnumerator OpenDoor()
    {
        _isInteracting = true;
        _promptGroup.alpha = 0;

        if (openSound != null) AudioSource.PlayClipAtPoint(openSound, transform.position);
        if (doorAnimator != null) doorAnimator.SetTrigger("Open");
        if (openEffect != null) openEffect.Play();

        yield return new WaitForSeconds(1.5f);
        if (!string.IsNullOrEmpty(nextLevelName)) SceneManager.LoadScene(nextLevelName);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}