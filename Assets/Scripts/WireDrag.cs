using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class WireDrag : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Color wireColor;

    public GameObject wireRectPrefab;
    public GameObject wireStartPartPrefab;
    public GameObject wireEndPartPrefab;

    private RectTransform wireRect;
    private RectTransform wireStartPartInstance;
    private RectTransform wireEndPartInstance;

    public bool isConnected = false;
    private bool isDragging = false;
    private Vector3 currentStretchTarget;
    private WireTarget hoveredTarget = null;

    // CHANGED: Made public so CableMatch can verify if it's connected to the right color
    public WireTarget connectedTarget = null;

    private static Dictionary<WireTarget, WireDrag> connections = new Dictionary<WireTarget, WireDrag>();

    void Awake()
    {
        if (wireRectPrefab != null)
        {
            GameObject w = Instantiate(wireRectPrefab, transform.parent as RectTransform);
            w.GetComponent<Image>().color = gameObject.GetComponent<Image>().color;
            wireRect = w.GetComponent<RectTransform>();
            wireRect.sizeDelta = new Vector2(0, wireRect.sizeDelta.y);
        }

        if (wireStartPartPrefab != null)
        {
            GameObject s = Instantiate(wireStartPartPrefab, transform.parent as RectTransform);
            s.GetComponent<Image>().color = gameObject.GetComponent<Image>().color;
            wireStartPartInstance = s.GetComponent<RectTransform>();
            wireStartPartInstance.gameObject.SetActive(false);
        }

        if (wireEndPartPrefab != null)
        {
            GameObject e = Instantiate(wireEndPartPrefab, transform.parent as RectTransform);
            e.GetComponent<Image>().color = gameObject.GetComponent<Image>().color;
            wireEndPartInstance = e.GetComponent<RectTransform>();
            wireEndPartInstance.gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        // FIX: Dynamically update wire stretch every frame!
        // This ensures the wire perfectly follows the hub if the UI is animating.
        if (isConnected && connectedTarget != null)
        {
            StretchWireToTarget(connectedTarget.transform.position);
            PlaceEndPart(connectedTarget.transform.position);
        }
        else if (isDragging)
        {
            // If dragging, we still need to recalculate the stretch from the moving hub
            StretchWireToTarget(currentStretchTarget);

            if (hoveredTarget != null)
            {
                PlaceEndPart(hoveredTarget.transform.position);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isConnected)
        {
            Disconnect();
            return;
        }

        isDragging = true;
        StretchWireToMouse(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isConnected) return;

        WireTarget target = GetUITargetUnderPointer(eventData);

        // CHANGED: Removed "target.targetColor == wireColor" so you can drag to any empty slot
        if (target != null && !IsTargetOccupied(target))
        {
            hoveredTarget = target;
            currentStretchTarget = target.transform.position;
            StretchWireToTarget(target.transform.position);
            PlaceEndPart(target.transform.position);
        }
        else
        {
            hoveredTarget = null;

            if (wireEndPartInstance != null)
                wireEndPartInstance.gameObject.SetActive(false);

            StretchWireToMouse(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isConnected) return;

        isDragging = false;

        if (hoveredTarget != null && !IsTargetOccupied(hoveredTarget))
        {
            Connect(hoveredTarget);
            return;
        }

        WireTarget target = GetUITargetUnderPointer(eventData);

        // CHANGED: Removed "target.targetColor == wireColor" so you can drop on any empty slot
        if (target != null && !IsTargetOccupied(target))
        {
            Connect(target);
            return;
        }

        ResetWire();
    }

    private bool IsTargetOccupied(WireTarget target)
    {
        return connections.ContainsKey(target) && connections[target] != this;
    }

    public void Connect(WireTarget target)
    {
        StretchWireToTarget(target.transform.position);
        PlaceEndPart(target.transform.position);
        isConnected = true;
        hoveredTarget = null;
        connectedTarget = target;
        connections[target] = this;
    }

    private void Disconnect()
    {
        if (connectedTarget != null)
        {
            connections.Remove(connectedTarget);
            connectedTarget = null;
        }

        isConnected = false;
        isDragging = false;
        hoveredTarget = null;
        ResetWire();
    }

    private void ResetWire()
    {
        if (wireRect != null)
            wireRect.sizeDelta = new Vector2(0, wireRect.sizeDelta.y);

        if (wireStartPartInstance != null)
            wireStartPartInstance.gameObject.SetActive(false);

        if (wireEndPartInstance != null)
            wireEndPartInstance.gameObject.SetActive(false);
    }

    private WireTarget GetUITargetUnderPointer(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            WireTarget target = result.gameObject.GetComponent<WireTarget>();
            if (target != null)
                return target;
        }

        return null;
    }

    private void StretchWireToMouse(PointerEventData eventData)
    {
        Vector3 mouseWorldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            wireRect.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out mouseWorldPos
        );

        currentStretchTarget = mouseWorldPos;
        StretchWireToTarget(mouseWorldPos);
    }

    public void UpdateWireColor(Color newColor)
    {
        wireColor = newColor;
        GetComponent<Image>().color = newColor;

        if (wireRect != null)
            wireRect.GetComponent<Image>().color = newColor;

        if (wireStartPartInstance != null)
            wireStartPartInstance.GetComponent<Image>().color = newColor;

        if (wireEndPartInstance != null)
            wireEndPartInstance.GetComponent<Image>().color = newColor;
    }

    private void StretchWireToTarget(Vector3 targetWorldPosition)
    {
        if (wireRect == null) return;

        wireRect.position = transform.position;

        Vector3 dir = targetWorldPosition - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        wireRect.rotation = rotation;

        float distance = dir.magnitude;
        float canvasScale = wireRect.lossyScale.x;
        wireRect.sizeDelta = new Vector2(distance / canvasScale, wireRect.sizeDelta.y);

        if (wireStartPartInstance != null)
        {
            wireStartPartInstance.gameObject.SetActive(true);
            wireStartPartInstance.position = transform.position;
            wireStartPartInstance.rotation = rotation;
        }
    }

    private void PlaceEndPart(Vector3 targetWorldPosition)
    {
        if (wireEndPartInstance == null) return;

        Vector3 dir = targetWorldPosition - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle + 180f);

        wireEndPartInstance.gameObject.SetActive(true);
        wireEndPartInstance.position = targetWorldPosition;
        wireEndPartInstance.rotation = rotation;
    }

    void OnDestroy()
    {
        if (connectedTarget != null)
            connections.Remove(connectedTarget);

        if (wireRect != null) Destroy(wireRect.gameObject);
        if (wireStartPartInstance != null) Destroy(wireStartPartInstance.gameObject);
        if (wireEndPartInstance != null) Destroy(wireEndPartInstance.gameObject);
    }
}
