using UnityEngine;
using UnityEngine.UI;

public class TankFill : MonoBehaviour
{
    [Header("UI")]
    public Image filler;
    public RectTransform topItem;

    [Tooltip("The area the bottle needs to be near to trigger pouring")]
    public RectTransform fillerPos;

    [Tooltip("The exact position the bottle should snap to when pouring")]
    public RectTransform bottlePos;

    [Header("Fill Limits")]
    [Range(0f, 1f)]
    public float fillAmount = 0.523f;

    public float minFill = 0.2f;
    public float maxFill = 0.7f;

    [Header("Top Item Movement")]
    public float topYAtMin = -60f;
    public float topYAtMax = -18.4f;

    void Start()
    {
        UpdateVisual();
    }

    public void AddFill(float amount)
    {
        fillAmount += amount;
        fillAmount = Mathf.Clamp(fillAmount, minFill, maxFill);

        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (filler != null) filler.fillAmount = fillAmount;

        if (topItem != null)
        {
            float t = Mathf.InverseLerp(minFill, maxFill, fillAmount);
            Vector2 pos = topItem.anchoredPosition;
            pos.y = Mathf.Lerp(topYAtMin, topYAtMax, t);
            topItem.anchoredPosition = pos;
        }
    }
}