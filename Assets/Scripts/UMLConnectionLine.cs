using UnityEngine;
using UnityEngine.UI;

public class UMLConnectionLine : MonoBehaviour
{
    private RectTransform startElement;
    private RectTransform endElement;
    private RectTransform myRect;

    public void Setup(RectTransform start, RectTransform end)
    {
        startElement = start;
        endElement = end;
        myRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (startElement == null || endElement == null) return;

        Vector2 startPos = startElement.anchoredPosition;
        Vector2 endPos = endElement.anchoredPosition;

        Vector2 direction = endPos - startPos;
        Vector2 dirNormalized = direction.normalized;

        
        
        float startOffset = startElement.sizeDelta.x * 0.45f;
        float endOffset = endElement.sizeDelta.x * 0.45f;

        Vector2 adjustedStart = startPos + (dirNormalized * startOffset);
        Vector2 adjustedEnd = endPos - (dirNormalized * endOffset);
        // -------------------------------

        Vector2 finalDir = adjustedEnd - adjustedStart;
        myRect.anchoredPosition = adjustedStart + (finalDir * 0.5f);

        float distance = finalDir.magnitude;
        //myRect.sizeDelta = new Vector2(distance, myRect.sizeDelta.y);
        myRect.sizeDelta = new Vector2(distance, 45f);
        float angle = Mathf.Atan2(finalDir.y, finalDir.x) * Mathf.Rad2Deg;
        myRect.rotation = Quaternion.Euler(0, 0, angle);
    }
}