using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class BottleDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Bottle Configuration")]
    public RectTransform bottle;
    public Canvas canvas;

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

    [Header("Animation Settings")]
    public float rotationDuration = 0.15f;

    private Quaternion startRot;
    private CanvasGroup canvasGroup;
    private bool pouring;
    private Coroutine rotateCoroutine;
    private TankFill[] allTanks;

    private TankFill hoveredWrongTank;

    void Awake()
    {
        startRot = bottle.rotation;

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
            targetTank.AddFill(fillSpeed * Time.deltaTime);
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
        bottle.anchoredPosition += eventData.delta / canvas.scaleFactor;
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