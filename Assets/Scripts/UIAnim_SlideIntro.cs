using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnim_SlideIntro : MonoBehaviour
{
    public enum SlideDirection { Left, Right, Top, Bottom, None }
    public System.Action onOutComplete;
    [Header("Animation Settings")]
    [SerializeField] private SlideDirection startDirection = SlideDirection.Right;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float offset = 1500f;

    [Header("Fade Settings")]
    [SerializeField] private bool useFade = true;

    private CanvasGroup canvasGroup;
    private List<RectTransform> childTransforms = new List<RectTransform>();
    private List<Vector2> targetPositions = new List<Vector2>();
    private List<Vector2> startPositions = new List<Vector2>();
    private Coroutine activeCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        foreach (Transform child in transform)
        {
            RectTransform rect = child.GetComponent<RectTransform>();
            if (rect != null)
                childTransforms.Add(rect);
        }
    }

    private void OnEnable()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        if (useFade && canvasGroup != null) canvasGroup.alpha = 0f;
        activeCoroutine = StartCoroutine(SlideAndFadeInRoutine());
    }

    public void EndPuzzle()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(SlideAndFadeOutRoutine());
    }

    private Vector2 CalculateStartPosition(Vector2 targetPos)
    {
        switch (startDirection)
        {
            case SlideDirection.Left: return new Vector2(targetPos.x - offset, targetPos.y);
            case SlideDirection.Right: return new Vector2(targetPos.x + offset, targetPos.y);
            case SlideDirection.Top: return new Vector2(targetPos.x, targetPos.y + offset);
            case SlideDirection.Bottom: return new Vector2(targetPos.x, targetPos.y - offset);
            default: return targetPos;
        }
    }

    private IEnumerator SlideAndFadeInRoutine()
    {
        // Wait one frame for Canvas Layout to finish calculating positions
        yield return null;

        targetPositions.Clear();
        startPositions.Clear();

        foreach (RectTransform rect in childTransforms)
        {
            Vector2 target = rect.anchoredPosition;
            targetPositions.Add(target);
            startPositions.Add(CalculateStartPosition(target));
        }

        for (int i = 0; i < childTransforms.Count; i++)
            childTransforms[i].anchoredPosition = startPositions[i];

        if (useFade && canvasGroup != null) canvasGroup.alpha = 0f;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < childTransforms.Count; i++)
                childTransforms[i].anchoredPosition = Vector2.Lerp(startPositions[i], targetPositions[i], smooth);

            if (useFade && canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        for (int i = 0; i < childTransforms.Count; i++)
            childTransforms[i].anchoredPosition = targetPositions[i];

        if (canvasGroup != null) canvasGroup.alpha = 1f;
        activeCoroutine = null;
    }

    private IEnumerator SlideAndFadeOutRoutine()
    {
        if (targetPositions.Count == 0)
        {
            Destroy(gameObject);
            yield break;
        }

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < childTransforms.Count; i++)
                childTransforms[i].anchoredPosition = Vector2.Lerp(targetPositions[i], startPositions[i], smooth);

            if (useFade && canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        activeCoroutine = null;
        Destroy(gameObject);
        onOutComplete?.Invoke();
    }
}