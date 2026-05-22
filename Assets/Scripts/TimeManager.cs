using UnityEngine;
using TMPro;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    // Singleton instance makes it accessible from any other script
    public static TimeManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI timerText;

    [Tooltip("This will be used as a template and cloned for each penalty.")]
    public TextMeshProUGUI penaltyText;

    private float timeElapsed;
    private bool isRunning = false;

    private void Awake()
    {
        // Setup Singleton
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        timeElapsed = 0f;
        UpdateTimerDisplay();

        // Setup the penalty text template
        if (penaltyText != null)
        {
            // Ensure the template has a CanvasGroup for fading
            if (penaltyText.GetComponent<CanvasGroup>() == null)
            {
                penaltyText.gameObject.AddComponent<CanvasGroup>();
            }

            // Hide the template so only the clones show up
            penaltyText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isRunning)
        {
            // Count up instead of down
            timeElapsed += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    // Public function to start the timer from zero
    public void StartTimer()
    {
        isRunning = true;
    }

    // Public function to start with a specific time already elapsed
    public void StartTimer(float startingTime)
    {
        timeElapsed = startingTime;
        isRunning = true;
    }

    // Public function to stop or pause the timer
    public void StopTimer()
    {
        isRunning = false;
    }

    // The penalty mechanic
    public void ApplyPenalty(float penaltyAmount = 5f)
    {
        timeElapsed += penaltyAmount;
        UpdateTimerDisplay();

        if (penaltyText != null)
        {
            // Clone the template and parent it to the same container
            GameObject newPenaltyText = Instantiate(penaltyText.gameObject, penaltyText.transform.parent);

            // Pass the newly created object to the coroutine
            StartCoroutine(AnimatePenaltyText(newPenaltyText, penaltyAmount));
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        float minutes = Mathf.FloorToInt(timeElapsed / 60);
        float seconds = Mathf.FloorToInt(timeElapsed % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Handles the floating and fading text animation for individual clones
    private IEnumerator AnimatePenaltyText(GameObject penaltyObj, float amount)
    {
        TextMeshProUGUI textComp = penaltyObj.GetComponent<TextMeshProUGUI>();
        CanvasGroup canvasGroup = penaltyObj.GetComponent<CanvasGroup>();
        RectTransform rect = penaltyObj.GetComponent<RectTransform>();

        textComp.text = $"-{amount}";
        penaltyObj.SetActive(true);

        // Start completely transparent for fade-in
        canvasGroup.alpha = 0f;

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, 120f); // Floats up 120 pixels

        float duration = 1.5f;
        float elapsed = 0f;

        // Split the duration into two phases (20% fade in, 80% fade out)
        float fadeInTime = duration * 0.3f;
        float fadeOutTime = duration * 0.7f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 1. Handle Floating (Continuous over the whole duration)
            float floatProgress = elapsed / duration;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, floatProgress);

            // 2. Handle Fading (Fade In -> Fade Out)
            if (elapsed < fadeInTime)
            {
                // Fading In
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
            }
            else
            {
                // Fading Out
                float timeInFadeOutPhase = elapsed - fadeInTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timeInFadeOutPhase / fadeOutTime);
            }

            yield return null;
        }

        // Clean up the cloned object after the animation is completely finished
        Destroy(penaltyObj);
    }
}