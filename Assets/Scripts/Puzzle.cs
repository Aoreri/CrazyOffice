using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Puzzle : MonoBehaviour
{
    public string puzzleName;
    public GameObject prefab;

    [Header("Opening Animation Settings")]
    public bool useOpeningAnimation = true;
    public float backgroundFadeDuration = 0.5f;
    public float slideInDuration = 0.5f;

    [Header("Closing Animation Settings")]
    public bool useClosingAnimation = true;
    public float backgroundFadeOutDuration = 0.4f;
    public float slideOutDuration = 0.4f;

    [Tooltip("Disables custom scripts and blocks UI clicks on the prefab while an animation is playing.")]
    public bool disableLogicDuringAnimation = true;

    private float startTime;
    private GameObject instance;
    private Coroutine animationCoroutine;
    private PuzzleAnimRunner animRunner;
    private bool isEnding;

    /// <summary>
    /// Returns the active UI root — the instantiated prefab if one exists, otherwise this puzzle's own gameObject.
    /// </summary>
    private GameObject ActiveUI => instance != null ? instance : gameObject;

    public void StartPuzzle()
    {
        isEnding = false;
        gameObject.SetActive(true);
        GameObject canvasObj = GameObject.FindGameObjectWithTag("UI");

        if (canvasObj != null && prefab != null)
        {
            Transform canvasTransform = canvasObj.transform;
            instance = Instantiate(prefab, canvasTransform);
            animRunner = instance.AddComponent<PuzzleAnimRunner>();

            if (useOpeningAnimation)
            {
                animationCoroutine = animRunner.StartCoroutine(AnimateIntro(instance));
            }
        }

        startTime = Time.time;
        Debug.Log("[Puzzle] " + puzzleName + " started.");

        OnStartPuzzle();
    }

    public void EndPuzzle()
    {
        // Guard against being called twice (e.g. button spam)
        if (isEnding) return;
        isEnding = true;

        Debug.Log("[Puzzle] " + puzzleName + " ending. Duration: " + (Time.time - startTime));

        // Stop any running intro animation
        if (animRunner != null && animationCoroutine != null)
        {
            animRunner.StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (useClosingAnimation)
        {
            GameObject animTarget = ActiveUI;

            // Pick a coroutine host: use animRunner (lives on instance) if available,
            // otherwise fall back to this MonoBehaviour (lives on gameObject).
            MonoBehaviour coroutineHost = (animRunner != null) ? (MonoBehaviour)animRunner : this;

            Debug.Log("[Puzzle] Starting closing animation on " + animTarget.name);
            animationCoroutine = coroutineHost.StartCoroutine(AnimateOutroAndCleanUp(animTarget));
        }
        else
        {
            Debug.Log("[Puzzle] Closing animation disabled.");
            OnEndPuzzle();
            CleanUp();
        }
    }

    /// <summary>
    /// Runs the outro animation and then performs cleanup.
    /// Kept as a single coroutine so gameObject.SetActive(false) happens last.
    /// </summary>
    private IEnumerator AnimateOutroAndCleanUp(GameObject animTarget)
    {
        yield return AnimateOutro(animTarget);

        Debug.Log("[Puzzle] Closing animation finished. Cleaning up.");
        OnEndPuzzle();
        CleanUp();
    }

    private void CleanUp()
    {
        if (instance != null)
        {
            Destroy(instance);
            instance = null;
        }

        animRunner = null;
        animationCoroutine = null;
        isEnding = false;
        Destroy(gameObject);
        //gameObject.SetActive(false);
    }

    protected abstract void OnStartPuzzle();
    protected abstract void OnEndPuzzle();

    // ==================== OPENING ANIMATION ====================
    private IEnumerator AnimateIntro(GameObject uiInstance)
    {
        // --- SCRIPT & CLICK DISABLER SETUP ---
        List<MonoBehaviour> pausedScripts = new List<MonoBehaviour>();
        CanvasGroup canvasGroup = null;

        if (disableLogicDuringAnimation)
        {
            canvasGroup = SetupCanvasGroup(uiInstance, false);
            pausedScripts = DisableCustomScripts(uiInstance);
        }

        // Background image is optional — animation still works without it
        Image mainPanelImage = uiInstance.GetComponent<Image>();
        Transform animRoot = (mainPanelImage != null) ? mainPanelImage.transform : uiInstance.transform;

        // SETUP ALPHA FADE
        float targetAlpha = 1f;
        if (mainPanelImage != null)
        {
            targetAlpha = mainPanelImage.color.a;
            Color startColor = mainPanelImage.color;
            startColor.a = 0f;
            mainPanelImage.color = startColor;
        }

        // SETUP SLIDE POSITIONS
        List<RectTransform> elementsToSlide = new List<RectTransform>();
        List<Vector2> originalPositions = new List<Vector2>();

        LayoutGroup[] layoutGroups = animRoot.GetComponentsInChildren<LayoutGroup>(true);
        foreach (var layout in layoutGroups) layout.enabled = false;

        float screenOffset = Screen.width * 1.5f;

        for (int i = 0; i < animRoot.childCount; i++)
        {
            RectTransform childRect = animRoot.GetChild(i) as RectTransform;
            if (childRect != null)
            {
                elementsToSlide.Add(childRect);
                originalPositions.Add(childRect.anchoredPosition);
                childRect.anchoredPosition = new Vector2(childRect.anchoredPosition.x + screenOffset, childRect.anchoredPosition.y);
            }
        }

        // PERFORM ALPHA FADE IN
        if (mainPanelImage != null && backgroundFadeDuration > 0f)
        {
            float elapsed = 0f;
            while (elapsed < backgroundFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                Color c = mainPanelImage.color;
                c.a = Mathf.Lerp(0f, targetAlpha, elapsed / backgroundFadeDuration);
                mainPanelImage.color = c;
                yield return null;
            }

            Color finalColor = mainPanelImage.color;
            finalColor.a = targetAlpha;
            mainPanelImage.color = finalColor;
        }

        // PERFORM SLIDE IN
        if (elementsToSlide.Count > 0 && slideInDuration > 0f)
        {
            float elapsed = 0f;
            while (elapsed < slideInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideInDuration);

                for (int i = 0; i < elementsToSlide.Count; i++)
                {
                    Vector2 startPos = new Vector2(originalPositions[i].x + screenOffset, originalPositions[i].y);
                    elementsToSlide[i].anchoredPosition = Vector2.Lerp(startPos, originalPositions[i], t);
                }
                yield return null;
            }

            for (int i = 0; i < elementsToSlide.Count; i++)
            {
                elementsToSlide[i].anchoredPosition = originalPositions[i];
            }
        }

        // RESTORE EVERYTHING
        foreach (var layout in layoutGroups) layout.enabled = true;
        RestoreAll(pausedScripts, canvasGroup);
    }

    // ==================== CLOSING ANIMATION ====================
    private IEnumerator AnimateOutro(GameObject uiTarget)
    {
        // --- SCRIPT & CLICK DISABLER SETUP ---
        List<MonoBehaviour> pausedScripts = new List<MonoBehaviour>();
        CanvasGroup canvasGroup = null;

        if (disableLogicDuringAnimation)
        {
            canvasGroup = SetupCanvasGroup(uiTarget, false);
            pausedScripts = DisableCustomScripts(uiTarget);
        }

        // Background image is optional — slide still works without it
        Image mainPanelImage = uiTarget.GetComponent<Image>();
        Transform animRoot = (mainPanelImage != null) ? mainPanelImage.transform : uiTarget.transform;

        float currentAlpha = (mainPanelImage != null) ? mainPanelImage.color.a : 0f;

        // GATHER CHILD ELEMENTS FOR SLIDE OUT
        List<RectTransform> elementsToSlide = new List<RectTransform>();
        List<Vector2> originalPositions = new List<Vector2>();

        LayoutGroup[] layoutGroups = animRoot.GetComponentsInChildren<LayoutGroup>(true);
        foreach (var layout in layoutGroups) layout.enabled = false;

        float screenOffset = Screen.width * 1.5f;

        for (int i = 0; i < animRoot.childCount; i++)
        {
            RectTransform childRect = animRoot.GetChild(i) as RectTransform;
            if (childRect != null)
            {
                elementsToSlide.Add(childRect);
                originalPositions.Add(childRect.anchoredPosition);
            }
        }

        // PERFORM SLIDE OUT (elements slide off to the right)
        if (elementsToSlide.Count > 0 && slideOutDuration > 0f)
        {
            float elapsed = 0f;
            while (elapsed < slideOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideOutDuration);

                for (int i = 0; i < elementsToSlide.Count; i++)
                {
                    if (elementsToSlide[i] == null) continue;
                    Vector2 endPos = new Vector2(originalPositions[i].x + screenOffset, originalPositions[i].y);
                    elementsToSlide[i].anchoredPosition = Vector2.Lerp(originalPositions[i], endPos, t);
                }
                yield return null;
            }
        }

        // PERFORM ALPHA FADE OUT (background fades to transparent)
        if (mainPanelImage != null && backgroundFadeOutDuration > 0f)
        {
            float elapsed = 0f;
            while (elapsed < backgroundFadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                Color c = mainPanelImage.color;
                c.a = Mathf.Lerp(currentAlpha, 0f, elapsed / backgroundFadeOutDuration);
                mainPanelImage.color = c;
                yield return null;
            }

            Color finalColor = mainPanelImage.color;
            finalColor.a = 0f;
            mainPanelImage.color = finalColor;
        }

        // RESTORE EVERYTHING so the gameObject is in a clean state when deactivated.
        // Scene-based puzzles reuse the same gameObject, so positions/alpha must be reset.
        for (int i = 0; i < elementsToSlide.Count; i++)
        {
            if (elementsToSlide[i] != null)
                elementsToSlide[i].anchoredPosition = originalPositions[i];
        }

        if (mainPanelImage != null)
        {
            Color c = mainPanelImage.color;
            c.a = currentAlpha;
            mainPanelImage.color = c;
        }

        foreach (var layout in layoutGroups) layout.enabled = true;
        RestoreAll(pausedScripts, canvasGroup);
    }

    // ==================== SHARED HELPERS ====================

    private CanvasGroup SetupCanvasGroup(GameObject target, bool interactable)
    {
        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg == null) cg = target.AddComponent<CanvasGroup>();
        cg.interactable = interactable;
        cg.blocksRaycasts = interactable;
        return cg;
    }

    private List<MonoBehaviour> DisableCustomScripts(GameObject target)
    {
        List<MonoBehaviour> paused = new List<MonoBehaviour>();
        MonoBehaviour[] allScripts = target.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var script in allScripts)
        {
            if (script == null || script is PuzzleAnimRunner || script == this) continue;

            string scriptNamespace = script.GetType().Namespace;
            bool isUnityScript = scriptNamespace != null &&
                                 (scriptNamespace.StartsWith("UnityEngine") || scriptNamespace.StartsWith("System"));

            if (!isUnityScript && script.enabled)
            {
                script.enabled = false;
                paused.Add(script);
            }
        }

        return paused;
    }

    private void RestoreAll(List<MonoBehaviour> pausedScripts, CanvasGroup canvasGroup)
    {
        if (pausedScripts != null)
        {
            foreach (var script in pausedScripts)
            {
                if (script != null) script.enabled = true;
            }
        }

        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}

// Dummy motor to run the coroutine safely on the active prefab instance
public class PuzzleAnimRunner : MonoBehaviour { }
