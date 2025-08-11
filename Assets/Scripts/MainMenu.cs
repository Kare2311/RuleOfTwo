using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    public string firstLevelName = "Level1";

    [Header("Fade Settings")]
    public float fadeInDuration = 1.5f;
    public float elementDelay = 0.3f;
    public float fadeOutDuration = 1f;

    [Header("UI References")]
    public Image background;
    public TMP_Text titleText;
    public Button startButton;
    public Button exitButton;
    public CanvasGroup[] uiElements;

    [Header("Animation Settings")]
    public float titleWobbleAmount = 2f;
    public float buttonHoverScale = 1.1f;
    public float buttonHoverDuration = 0.2f;

    private Vector3[] originalScales;
    private bool isTransitioning;

    void Start()
    {
        // Move title higher (adjust the 200f value to your preference)
        titleText.transform.position = new Vector3(
            titleText.transform.position.x,
            Screen.height * 0.75f, // Positions title at 75% of screen height
            titleText.transform.position.z
        );

        // Initialize UI elements array if not set in inspector
        if (uiElements == null || uiElements.Length == 0)
        {
            uiElements = new CanvasGroup[]
            {
                background.GetComponent<CanvasGroup>(),
                titleText.GetComponent<CanvasGroup>(),
                startButton.GetComponent<CanvasGroup>(),
                exitButton.GetComponent<CanvasGroup>()
            };
        }

        // Store original scales for animation resets
        originalScales = new Vector3[]
        {
            titleText.transform.localScale,
            startButton.transform.localScale,
            exitButton.transform.localScale
        };

        // Start fade-in sequence
        StartCoroutine(InitializeMenu());
    }

    IEnumerator InitializeMenu()
    {
        // Set all elements to transparent initially
        foreach (var element in uiElements)
        {
            if (element != null)
            {
                element.alpha = 0;
                element.gameObject.SetActive(true);
            }
        }

        // Fade in background first
        if (uiElements.Length > 0 && uiElements[0] != null)
            yield return StartCoroutine(FadeElement(uiElements[0], 1, fadeInDuration));

        // Then fade other elements sequentially
        for (int i = 1; i < uiElements.Length; i++)
        {
            if (uiElements[i] != null)
            {
                yield return new WaitForSeconds(elementDelay);
                yield return StartCoroutine(FadeElement(uiElements[i], 1, fadeInDuration));
            }
        }
    }

    void Update()
    {
        if (!isTransitioning)
        {
            // Add subtle wobble to title
            titleText.transform.localPosition = new Vector3(
                Mathf.Sin(Time.time * 0.5f) * titleWobbleAmount,
                Mathf.Cos(Time.time * 0.3f) * (titleWobbleAmount / 2),
                0
            );
        }
    }

    public void OnStartButtonHover()
    {
        if (!isTransitioning)
            StartCoroutine(ScaleButton(startButton.transform, originalScales[1] * buttonHoverScale, buttonHoverDuration));
    }

    public void OnExitButtonHover()
    {
        if (!isTransitioning)
            StartCoroutine(ScaleButton(exitButton.transform, originalScales[2] * buttonHoverScale, buttonHoverDuration));
    }

    public void OnButtonHoverExit(Transform button)
    {
        if (!isTransitioning)
            StartCoroutine(ScaleButton(button, originalScales[button == startButton.transform ? 1 : 2], buttonHoverDuration));
    }

    public void StartGame()
    {
        if (!isTransitioning)
            StartCoroutine(FadeAndLoad());
    }

    public void ExitGame()
    {
        if (!isTransitioning)
            StartCoroutine(FadeAndExit());
    }

    IEnumerator FadeAndLoad()
    {
        isTransitioning = true;

        // Fade out all elements
        for (int i = uiElements.Length - 1; i >= 0; i--)
        {
            if (uiElements[i] != null)
            {
                StartCoroutine(FadeElement(uiElements[i], 0, fadeOutDuration));
                yield return new WaitForSeconds(elementDelay / 2);
            }
        }

        yield return new WaitForSeconds(fadeOutDuration);
        SceneManager.LoadScene(firstLevelName);
    }

    IEnumerator FadeAndExit()
    {
        isTransitioning = true;

        foreach (var element in uiElements.Reverse())
        {
            if (element != null)
            {
                StartCoroutine(FadeElement(element, 0, fadeOutDuration));
                yield return new WaitForSeconds(elementDelay / 2);
            }
        }

        yield return new WaitForSeconds(fadeOutDuration);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator FadeElement(CanvasGroup element, float targetAlpha, float duration)
    {
        float startAlpha = element.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            element.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        element.alpha = targetAlpha;
    }

    IEnumerator ScaleButton(Transform button, Vector3 targetScale, float duration)
    {
        Vector3 startScale = button.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            button.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        button.localScale = targetScale;
    }
}