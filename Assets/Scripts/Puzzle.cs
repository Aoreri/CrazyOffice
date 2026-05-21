using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Puzzle : MonoBehaviour
{
    public string puzzleName;
    public GameObject prefab;

    private float startTime;
    private GameObject instance;

    // Track the active coroutine instead of using StopAllCoroutines()
    private Coroutine activeCoroutine;

    [Header("UI Animation Settings")]
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private float slideOffset = 1500f;
    [SerializeField] private bool useFadeEffect = true;

    // ─────────────────────────────────────────────
    //  Expected Prefab Structure (set up in Inspector):
    //
    //  PuzzlePrefab (Root)
    //    ├── BlurBackground   ← tag: "PuzzleBlurBG"
    //    └── ButtonContainer  ← tag: "PuzzleButtons"
    //           ├── Button_1
    //           └── Button_2 ...
    // ─────────────────────────────────────────────

    public void StartPuzzle()
    {
        gameObject.SetActive(true);

        GameObject canvasObj = GameObject.FindGameObjectWithTag("UI");
        Transform canvasTransform = canvasObj != null ? canvasObj.transform : null;

        if (prefab != null && canvasTransform != null)
        {
            if (instance != null) Destroy(instance);
            instance = Instantiate(prefab, canvasTransform);
            // UIAnim_SlideIntro OnEnable'da otomatik çalışır, ekstra çağrı gerekmez
        }

        startTime = Time.time;
        Debug.Log(puzzleName + " started.");
        OnStartPuzzle();
    }

    public void EndPuzzle()
    {
        Debug.Log(puzzleName + " ended. Time: " + (Time.time - startTime));

        if (instance != null)
        {
            // UIAnim_SlideIntro'nun EndPuzzle'ını çağır, o Destroy'u kendisi halleder
            UIAnim_SlideIntro anim = instance.GetComponent<UIAnim_SlideIntro>();
            if (anim != null)
                anim.EndPuzzle();
            else
                Destroy(instance);
        }
        else
        {
            gameObject.SetActive(false);
        }

        OnEndPuzzle();
    }

    // ─── ENTER: Blur BG fades in place, buttons slide in from the right ────────
    private IEnumerator AnimatePuzzleIn(GameObject targetInstance)
    {
        if (targetInstance == null) yield break;

        // IMPORTANT: Wait one frame so Unity can calculate Canvas Layout first.
        // Without this, anchoredPosition values are not yet valid.
        yield return null;
        if (targetInstance == null) yield break;

        // ── Blur Background ──────────────────────────────────────────────────────
        // The blur BG only fades — it never moves.
        Transform blurBGTransform = FindChildByTag(targetInstance, "PuzzleBlurBG");
        CanvasGroup blurGroup = null;

        if (blurBGTransform != null)
        {
            blurGroup = blurBGTransform.GetComponent<CanvasGroup>();
            if (blurGroup == null) blurGroup = blurBGTransform.gameObject.AddComponent<CanvasGroup>();
            if (useFadeEffect) blurGroup.alpha = 0f;
        }

        // ── Buttons (children of ButtonContainer) ────────────────────────────────
        // Each button slides in from the right and fades in simultaneously.
        Transform buttonContainer = FindChildByTag(targetInstance, "PuzzleButtons");
        List<RectTransform> buttonRects = new List<RectTransform>();
        List<Vector2> targetPositions = new List<Vector2>();
        List<Vector2> startPositions = new List<Vector2>();
        List<CanvasGroup> buttonGroups = new List<CanvasGroup>();

        if (buttonContainer != null)
        {
            foreach (Transform child in buttonContainer)
            {
                RectTransform rect = child.GetComponent<RectTransform>();
                if (rect == null) continue;

                buttonRects.Add(rect);
                targetPositions.Add(rect.anchoredPosition); // Final resting position on screen
                startPositions.Add(new Vector2(rect.anchoredPosition.x + slideOffset, rect.anchoredPosition.y));

                if (useFadeEffect)
                {
                    CanvasGroup cg = child.GetComponent<CanvasGroup>();
                    if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    buttonGroups.Add(cg);
                }

                rect.anchoredPosition = startPositions[startPositions.Count - 1];
            }
        }

        // ── Animation Loop ────────────────────────────────────────────────────────
        float timer = 0f;
        while (timer < duration)
        {
            if (targetInstance == null) yield break;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            // Blur BG → fade in, stays in place
            if (useFadeEffect && blurGroup != null)
                blurGroup.alpha = Mathf.Lerp(0f, 1f, t);

            // Buttons → slide from right to target position + fade in
            for (int i = 0; i < buttonRects.Count; i++)
            {
                if (buttonRects[i] == null) continue;
                buttonRects[i].anchoredPosition = Vector2.Lerp(startPositions[i], targetPositions[i], smooth);
                if (useFadeEffect && i < buttonGroups.Count)
                    buttonGroups[i].alpha = Mathf.Lerp(0f, 1f, t);
            }

            yield return null;
        }

        // Lock final values to prevent floating point drift
        if (targetInstance != null)
        {
            if (useFadeEffect && blurGroup != null) blurGroup.alpha = 1f;

            for (int i = 0; i < buttonRects.Count; i++)
            {
                if (buttonRects[i] == null) continue;
                buttonRects[i].anchoredPosition = targetPositions[i];
                if (useFadeEffect && i < buttonGroups.Count) buttonGroups[i].alpha = 1f;
            }
        }
    }

    // ─── EXIT: Everything fades out in place, no movement ──────────────────────
    private IEnumerator AnimatePuzzleOut(GameObject targetInstance)
    {
        if (targetInstance == null) yield break;

        // Add a single CanvasGroup to the root to fade the entire UI at once.
        // Positions are never touched — everything dissolves where it stands.
        CanvasGroup rootGroup = targetInstance.GetComponent<CanvasGroup>();
        if (rootGroup == null) rootGroup = targetInstance.AddComponent<CanvasGroup>();
        rootGroup.alpha = 1f;

        float timer = 0f;
        while (timer < duration)
        {
            if (targetInstance == null) yield break;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            if (useFadeEffect) rootGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        // Cleanup: destroy the UI clone and deactivate the manager object
        if (targetInstance != null)
        {
            if (useFadeEffect) rootGroup.alpha = 0f;
            Destroy(targetInstance);
        }

        gameObject.SetActive(false);
        activeCoroutine = null;
    }

    // ─── Helper: find a direct child by tag ────────────────────────────────────
    // Tip: if you'd rather not use tags, replace with GetChild(0) / GetChild(1).
    private Transform FindChildByTag(GameObject parent, string tag)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.CompareTag(tag)) return child;
        }
        Debug.LogWarning($"No child with tag '{tag}' found under: {parent.name}");
        return null;
    }

    protected abstract void OnStartPuzzle();
    protected abstract void OnEndPuzzle();
}