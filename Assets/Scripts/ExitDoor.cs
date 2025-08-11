using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class ExitDoorManager : MonoBehaviour
{
    [Header("Player Assignments")]
    public Transform player1;
    public Transform player2;

    [Header("Door Assignments")]
    public Transform door1;
    public Transform door2;
    public float interactionRadius = 3f;

    [Header("UI Settings")]
    public GameObject centerPanel;
    public TextMeshProUGUI centerText;
    public KeyCode interactKey = KeyCode.E;
    public string readyMessage = "Press 'E' to continue";
    public float messageDuration = 3f;

    [Header("Transition Settings")]
    public string nextLevelName = "Level2";
    public float transitionDelay = 0.5f;

    private bool _player1Near;
    private bool _player2Near;
    private Coroutine _messageTimer;

    void Start()
    {
        if (centerPanel != null) centerPanel.SetActive(false);

        // Auto-find players if not assigned
        if (player1 == null || player2 == null)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length > 0) player1 = players[0].transform;
            if (players.Length > 1) player2 = players[1].transform;
        }
    }

    void Update()
    {
        CheckPlayerPositions();
        HandleInteractionInput();
    }

    void CheckPlayerPositions()
    {
        bool wasPlayer1Near = _player1Near;
        bool wasPlayer2Near = _player2Near;

        _player1Near = Vector3.Distance(player1.position, door1.position) <= interactionRadius;
        _player2Near = Vector3.Distance(player2.position, door2.position) <= interactionRadius;

        // Only update when state changes
        if (_player1Near != wasPlayer1Near || _player2Near != wasPlayer2Near)
        {
            UpdateDoorStatus();
        }
    }

    void HandleInteractionInput()
    {
        if (_player1Near && _player2Near && Input.GetKeyDown(interactKey))
        {
            TransitionToNextLevel();
        }
    }

    void UpdateDoorStatus()
    {
        if (centerPanel == null) return;

        bool bothNear = _player1Near && _player2Near;

        if (bothNear)
        {
            ShowReadyMessage();
        }
        else
        {
            HideReadyMessage();
        }
    }

    void ShowReadyMessage()
    {
        centerPanel.SetActive(true);
        centerText.text = readyMessage;

        // Start/restart the message timer
        if (_messageTimer != null) StopCoroutine(_messageTimer);
        _messageTimer = StartCoroutine(HideMessageAfterDelay());
    }

    void HideReadyMessage()
    {
        if (_messageTimer != null)
        {
            StopCoroutine(_messageTimer);
            _messageTimer = null;
        }
        centerPanel.SetActive(false);
    }

    IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        HideReadyMessage();
    }

    void TransitionToNextLevel()
    {
        // Immediately hide message and load level
        HideReadyMessage();
        StartCoroutine(LoadNextLevel());
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(transitionDelay);

        if (!string.IsNullOrEmpty(nextLevelName))
        {
            SceneManager.LoadScene(nextLevelName);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = _player1Near ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(door1.position, interactionRadius);

        Gizmos.color = _player2Near ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(door2.position, interactionRadius);
    }
}