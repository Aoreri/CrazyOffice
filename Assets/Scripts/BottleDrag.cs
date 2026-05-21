using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class BottleDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Bottle Configuration")]
    public RectTransform bottle;

    [Tooltip("Assign the matching color Tank object here in the Inspector")]
    public TankFill targetTank;

    [Header("Bottle Visuals")]
    public Image bottleImage;

    [Tooltip("Normal bottle sprite")]
    public Sprite normalSprite;

    [Tooltip("Pouring bottle sprite")]
    public Sprite pouringSprite;

    [Header("Pour Settings")]
    public float snapDistance = 120f;
    public float rotateAngle = -65f;
    public float fillSpeed = 0.25f;

    [Header("Penalty Settings")]
    [Tooltip("Seconds the user can hold a full tank before triggering a penalty and auto-pulling")]
    public float overfillPenaltyTime = 1.5f;

    [Header("Animation Settings")]
    public float rotationDuration = 0.15f;

    private Quaternion startRot;
    private CanvasGroup canvasGroup;
    private bool pouring;
    private Coroutine rotateCoroutine;
    private TankFill[] allTanks;

    private TankFill hoveredWrongTank;
    private float overfillTimer = 0f;

    // Tracks the parent canvas dynamically
    private Canvas parentCanvas;

    void Awake()
    {
        startRot = bottle.rotation;

        // Automatically locate the canvas this UI element belongs to
        parentCanvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        allTanks = FindObjectsByType<TankFill>(FindObjectsInactive.Exclude);

        SetNormalVisual();
    }

    void Update()
    {
        if (pouring && targetTank != null)
        {
            // We subtract a tiny amount (0.001f) to prevent floating-point inaccuracies from failing the check
            if (targetTank.fillAmount >= targetTank.maxFill - 0.001f)
            {
                // Tank is full, start the penalty timer
                overfillTimer += Time.deltaTime;

                if (overfillTimer >= overfillPenaltyTime)
                {
                    Debug.Log($"PENALTY: Bottle kept pouring after '{targetTank.gameObject.name}' was full!");

                    // TODO: apply actual penalty here (e.g., reduce score, play error sound)

                    // Auto-pull (stop pouring and tilt the bottle back)
                    StopPouring();
                    overfillTimer = 0f;
                }
            }
            else
            {
                // Tank is not full, continue filling and ensure timer stays at 0
                targetTank.AddFill(fillSpeed * Time.deltaTime);
                overfillTimer = 0f;
            }
        }
        else
        {
            // Reset the timer if we are not actively pouring
            overfillTimer = 0f;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        StopPouring();
        hoveredWrongTank = null;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Safely apply the canvas scale factor, defaulting to 1f if a canvas somehow isn't found
        float scaleFactor = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
        bottle.anchoredPosition += eventData.delta / scaleFactor;

        TrySnapToCorrectTank();
        TrackWrongTankHover();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (!pouring)
        {
            if (hoveredWrongTank != null)
            {
                Debug.Log($"PENALTY: bottle dropped near wrong tank '{hoveredWrongTank.gameObject.name}'");
                // TODO: apply actual penalty here
            }

            if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
            rotateCoroutine = StartCoroutine(SmoothRotate(startRot));
            SetNormalVisual();
        }

        hoveredWrongTank = null;
    }

    void TrySnapToCorrectTank()
    {
        if (targetTank == null || targetTank.fillerPos == null || targetTank.bottlePos == null)
            return;

        float dist = Vector2.Distance(bottle.position, targetTank.fillerPos.position);

        if (dist < snapDistance || pouring)
        {
            bottle.position = targetTank.bottlePos.position;

            if (!pouring)
            {
                pouring = true;
                SetPouringVisual();

                if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
                rotateCoroutine = StartCoroutine(SmoothRotate(Quaternion.Euler(0, 0, rotateAngle)));
            }
        }
    }

    void TrackWrongTankHover()
    {
        if (pouring) return;

        hoveredWrongTank = null;

        if (allTanks == null) return;

        foreach (TankFill tank in allTanks)
        {
            if (tank == targetTank) continue;

            if (tank.fillerPos == null)
            {
                Debug.LogWarning($"TrackWrongTankHover: '{tank.gameObject.name}' is missing FillerPos.");
                continue;
            }

            float dist = Vector2.Distance(bottle.position, tank.fillerPos.position);
            if (dist < snapDistance)
            {
                hoveredWrongTank = tank;
                break;
            }
        }
    }

    void StopPouring()
    {
        if (pouring)
        {
            pouring = false;

            if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
            rotateCoroutine = StartCoroutine(SmoothRotate(startRot));
        }

        SetNormalVisual();
    }

    private IEnumerator SmoothRotate(Quaternion targetRotation)
    {
        float elapsedTime = 0f;
        Quaternion currentRotation = bottle.rotation;

        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            bottle.rotation = Quaternion.Lerp(currentRotation, targetRotation, elapsedTime / rotationDuration);
            yield return null;
        }

        bottle.rotation = targetRotation;
    }

    void SetNormalVisual()
    {
        if (bottleImage != null && normalSprite != null)
            bottleImage.sprite = normalSprite;
    }

    void SetPouringVisual()
    {
        if (bottleImage != null && pouringSprite != null)
            bottleImage.sprite = pouringSprite;
    }
}