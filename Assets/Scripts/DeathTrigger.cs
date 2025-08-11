using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DeathTrigger : MonoBehaviour
{
    public CanvasGroup tryAgainScreen; // CanvasGroup for fading
    public float fadeDuration = 1f;    // Fade in time
    public float displayTime = 3f;     // Time to show before restart

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(ShowTryAgainAndRestart());
        }
    }

    IEnumerator ShowTryAgainAndRestart()
    {
        // Make sure panel is visible
        tryAgainScreen.gameObject.SetActive(true);
        tryAgainScreen.alpha = 0f;

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            tryAgainScreen.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        // Wait before restarting
        yield return new WaitForSeconds(displayTime);

        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}