using UnityEngine;
using UnityEngine.EventSystems;

public class CardSwipeHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Vector2 startPos;
    private float startTime;

    private RectTransform rectTransform;
    private Canvas canvas;

    private Vector2 originalPosition;
    private bool hasStoredStartPos = false;

    private float pointerStartX;
    private float cardStartX;

    [Header("Swipe Settings")]
    public float minDistance = 150f;
    public float minVelocity = 1000f;

    [Header("Return Settings")]
    public float returnTime = 0.15f;

    private bool isReturning = false;
    private Vector2 velocityRef = Vector2.zero;
    private Vector2 returnFromPosition;

    public System.Action<float> OnSwipeRight;
    public System.Action<float> OnSwipeLeft;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    void Start()
    {
        originalPosition = rectTransform.anchoredPosition;
        hasStoredStartPos = true;
    }

    void Update()
    {
        if (isReturning)
        {
            rectTransform.anchoredPosition = Vector2.SmoothDamp(
                returnFromPosition,
                originalPosition,
                ref velocityRef,
                returnTime
            );

            returnFromPosition = rectTransform.anchoredPosition;

            if (Vector2.Distance(rectTransform.anchoredPosition, originalPosition) < 0.1f)
            {
                rectTransform.anchoredPosition = originalPosition;
                velocityRef = Vector2.zero;
                isReturning = false;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!hasStoredStartPos)
        {
            originalPosition = rectTransform.anchoredPosition;
            hasStoredStartPos = true;
        }

        startPos = eventData.position;
        startTime = Time.time;
        pointerStartX = eventData.position.x;
        cardStartX = rectTransform.anchoredPosition.x;

        isReturning = false;
        velocityRef = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float dragDeltaX = (eventData.position.x - pointerStartX) / canvas.scaleFactor;
        float newX = cardStartX + dragDeltaX;

        newX = Mathf.Max(newX, originalPosition.x);

        if (newX < rectTransform.anchoredPosition.x)
            return;

        rectTransform.anchoredPosition = new Vector2(newX, originalPosition.y);
        returnFromPosition = rectTransform.anchoredPosition;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        float swipeTime = Time.time - startTime;
        float distance = (eventData.position - startPos).magnitude;
        float velocity = distance / swipeTime;

        float deltaX = eventData.position.x - startPos.x;

        // only trigger right swipe
        if (deltaX > 0 && (deltaX > minDistance || velocity > minVelocity))
        {
            OnSwipeRight?.Invoke(swipeTime);
        }

        StartReturn();
    }

    void StartReturn()
    {
        velocityRef = Vector2.zero;
        isReturning = true;
    }
}