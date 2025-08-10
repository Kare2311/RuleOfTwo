using TMPro;
using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial UI")]
    public TMP_Text tutorialText;
    public CanvasGroup tutorialGroup;
    public float displayTime = 3f;

    [Header("Initial Tutorial")]
    public string initialMessage = "<b>RULE 1:</b> The <color=#00FFFF>LEFT</color> character moves normally\n" +
                                 "The <color=#FFA500>RIGHT</color> character mirrors your inputs!\n" +
                                 "Try moving with <color=#FFFF00><b>A/D</b></color> keys";

    private bool _hasShownTutorial;

    void Start()
    {
        // Initialize UI
        if (tutorialGroup != null)
        {
            tutorialGroup.alpha = 0;
            tutorialGroup.gameObject.SetActive(true);
        }

        ShowInitialTutorial();
    }

    void ShowInitialTutorial()
    {
        if (!_hasShownTutorial && tutorialText != null && tutorialGroup != null)
        {
            _hasShownTutorial = true;
            tutorialText.text = initialMessage;

            // Immediate appear
            tutorialGroup.alpha = 1;
            tutorialGroup.gameObject.SetActive(true);

            // Auto-hide after duration
            StartCoroutine(HideAfterDelay());
        }
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);

        // Smooth fade out
        float fadeDuration = 0.5f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            tutorialGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        tutorialGroup.gameObject.SetActive(false);
    }
}