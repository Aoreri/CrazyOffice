using UnityEngine;
using UnityEngine.EventSystems;

public class WireDrag : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public string wireColor = "Red";

    public RectTransform wireRect;

    private bool isConnected = false;

    void Start()
    {
        // Start the wire with zero length
        if (wireRect != null)
        {
            wireRect.sizeDelta = new Vector2(0, wireRect.sizeDelta.y);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isConnected) return;
        StretchWireToMouse(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isConnected) return;
        StretchWireToMouse(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isConnected) return;

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2 dropPos2D = new Vector2(worldPoint.x, worldPoint.y);

        Collider2D hit = Physics2D.OverlapPoint(dropPos2D);

        if (hit != null)
        {
            Debug.Log("hit");
            WireTarget target = hit.GetComponent<WireTarget>();

            if (target != null && target.targetColor == this.wireColor)
            {
                // Success! Snap perfectly to the target hub's exact position
                StretchWireToTarget(target.transform.position);
                isConnected = true;
                return;
            }
        }

        wireRect.sizeDelta = new Vector2(0, wireRect.sizeDelta.y);
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

        StretchWireToTarget(mouseWorldPos);
    }

    private void StretchWireToTarget(Vector3 targetWorldPosition)
    {
        if (wireRect == null) return;

        wireRect.position = transform.position;

        Vector3 dir = targetWorldPosition - transform.position;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        wireRect.rotation = Quaternion.Euler(0, 0, angle);

        float distance = dir.magnitude;
        float canvasScale = wireRect.lossyScale.x; 
        wireRect.sizeDelta = new Vector2(distance / canvasScale, wireRect.sizeDelta.y);
    }
}