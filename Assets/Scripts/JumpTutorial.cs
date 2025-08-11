using TMPro;
using UnityEngine;
using System.Collections;

public class JumpTutorial : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text tutorialText; // Assign the same TextMeshPro UI element
    public CanvasGroup tutorialGroup; // Assign the same CanvasGroup
    public float displayDuration = 4f; // Show for 4 seconds

    [Header("Obstacle Detection")]
    public float triggerDistance = 3f; // Show when 3m from obstacle
    public LayerMask obstacleLayer; // Assign your obstacle layer
    public bool debugMode = true; // Toggle debug messages

    private bool _hasShown;

    void Update()
    {
        Debug.Log("Obstacle layer value: " + obstacleLayer.value);

        var players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log("Found " + players.Length + " player objects");
        foreach (var p in players)
        {
            Debug.Log("Player: " + p.name + " at " + p.transform.position);
        }

        if (_hasShown) return;

        bool isNearObstacle = IsPlayerNearObstacle();

        if (debugMode)
        {
            Debug.Log($"Near obstacle: {isNearObstacle} | Distance: {GetDistanceToNearestObstacle():F1}m");
        }

        if (isNearObstacle)
        {
            ShowTutorial();
            _hasShown = true;
        }
    }

    bool IsPlayerNearObstacle()
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (Physics.Raycast(
                player.transform.position + Vector3.up * 0.1f,
                player.transform.forward,
                out RaycastHit hit,
                triggerDistance,
                obstacleLayer))
            {
                if (debugMode) Debug.Log($"Hit obstacle: {hit.collider.name}");
                return true;
            }
        }
        return false;
    }

    float GetDistanceToNearestObstacle()
    {
        float nearestDistance = Mathf.Infinity;
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (Physics.Raycast(
                player.transform.position + Vector3.up * 0.5f,
                player.transform.forward,
                out RaycastHit hit,
                Mathf.Infinity,
                obstacleLayer))
            {
                nearestDistance = Mathf.Min(nearestDistance, hit.distance);
            }
        }
        return nearestDistance;
    }

    void ShowTutorial()
    {
        if (tutorialText == null || tutorialGroup == null)
        {
            Debug.LogError("Tutorial UI references not assigned!");
            return;
        }

        if (debugMode) Debug.Log("Showing jump tutorial");

        tutorialText.text = "<size=36><b>JUMP!</b></size>\nPress <color=#FFFF00>SPACE</color>";
        tutorialGroup.alpha = 1;
        tutorialGroup.gameObject.SetActive(true);
        StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        if (debugMode) Debug.Log($"Tutorial will hide in {displayDuration} seconds");

        yield return new WaitForSeconds(displayDuration);

        float fadeTime = 0.5f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            tutorialGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        tutorialGroup.gameObject.SetActive(false);

        if (debugMode) Debug.Log("Tutorial hidden");
    }

    void OnDrawGizmos()
    {
        if (!debugMode) return;

        Gizmos.color = Color.yellow;
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            Gizmos.DrawRay(player.transform.position + Vector3.up * 0.5f,
                          player.transform.forward * triggerDistance);
        }
    }
}