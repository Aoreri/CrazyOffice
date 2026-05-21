using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Jackpot : MonoBehaviour
{
    [Header("References")]
    public Button spinButton;

    [Tooltip("Top -> Bottom")]
    public RectTransform[] slots;
    public RectTransform itemParent;

    [Header("Prefabs")]
    public GameObject[] itemPrefabs;

    [Header("Spin")]
    public float spinDuration = 0.25f;
    public AnimationCurve spinCurve;

    private List<RectTransform> items = new();

    // Data for GameManager to read
    public List<RectTransform> CurrentItems => items;
    public Action OnSpinComplete;

    // NEW: Track if this specific column is spinning
    public bool isSpinning = false;

    private bool hasSubscribedButton = false;

    void Start()
    {
        SubscribeButton();
        if (items.Count == 0)
        {
            Initialize();
        }
    }

    /// <summary>
    /// Clears all existing items and spawns fresh ones at slot positions.
    /// Safe to call multiple times (e.g. on puzzle restart).
    /// </summary>
    public void Initialize()
    {
        // Clear any existing items first
        foreach (var item in items)
        {
            if (item != null) Destroy(item.gameObject);
        }
        items.Clear();

        // Force Unity to recalculate layout so slot positions are up-to-date
        Canvas.ForceUpdateCanvases();

        // Sort slots physically Top -> Bottom using world space Y to completely ignore anchor quirks
        if (slots != null && slots.Length > 0)
        {
            System.Array.Sort(slots, (a, b) => b.position.y.CompareTo(a.position.y));
        }

        // Disable LayoutGroup if it exists so manual animations work smoothly
        if (itemParent != null)
        {
            LayoutGroup lg = itemParent.GetComponent<LayoutGroup>();
            if (lg != null) lg.enabled = false;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            RectTransform item = SpawnRandomItem(slots[i]);
            // Unity magically calculates the exact anchoredPosition needed to put the item here!
            item.position = slots[i].position;
            items.Add(item);
        }

        // Make sure the spin button is interactable for a new round
        if (spinButton != null)
            spinButton.interactable = true;

        isSpinning = false;

        SubscribeButton();
    }

    private void SubscribeButton()
    {
        if (!hasSubscribedButton && spinButton != null)
        {
            spinButton.onClick.AddListener(Spin);
            hasSubscribedButton = true;
        }
    }

    RectTransform SpawnRandomItem(RectTransform refSlot = null)
    {
        int index = UnityEngine.Random.Range(0, itemPrefabs.Length);
        GameObject obj = Instantiate(itemPrefabs[index], itemParent);
        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        // Safely match anchors and size
        if (refSlot != null)
        {
            rt.anchorMin = refSlot.anchorMin;
            rt.anchorMax = refSlot.anchorMax;
            rt.pivot = refSlot.pivot;

            // This safely updates sizeDelta to match physical size without breaking stretch anchors
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, refSlot.rect.width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, refSlot.rect.height);
        }

        return rt;
    }

    public void Spin()
    {
        if (isSpinning) return;
        StartCoroutine(SpinCoroutine());
    }

    IEnumerator SpinCoroutine()
    {
        isSpinning = true; // Mark as spinning
        if (spinButton != null) spinButton.interactable = false;

        // Safely calculate true local physical distance regardless of anchor differences
        Vector3 localPos0 = itemParent.InverseTransformPoint(slots[0].position);
        Vector3 localPos1 = itemParent.InverseTransformPoint(slots[1].position);
        float slotDistance = Mathf.Abs(localPos0.y - localPos1.y);

        RectTransform newItem = SpawnRandomItem(slots[0]);
        // Let Unity calculate the base anchoredPosition for the Top Slot
        newItem.position = slots[0].position;
        // Now offset it smoothly by the local pixel distance!
        Vector2 spawnPos = newItem.anchoredPosition;
        spawnPos.y += slotDistance;
        newItem.anchoredPosition = spawnPos;

        items.Insert(0, newItem);

        Vector2[] startPositions = new Vector2[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            startPositions[i] = items[i].anchoredPosition;
        }

        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinDuration);
            float curveT = (spinCurve != null && spinCurve.length > 1) ? spinCurve.Evaluate(t) : t;
            float moveY = Mathf.Lerp(0, slotDistance, curveT);

            // Interpolate perfectly using native anchoredPosition for buttery smoothness
            for (int i = 0; i < items.Count; i++)
            {
                items[i].anchoredPosition = startPositions[i] + Vector2.down * moveY;
            }

            yield return null;
        }

        // Snap exactly to slots
        for (int i = 0; i < slots.Length; i++)
        {
            // Update anchors before snapping
            items[i].anchorMin = slots[i].anchorMin;
            items[i].anchorMax = slots[i].anchorMax;
            items[i].pivot = slots[i].pivot;
            items[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slots[i].rect.width);
            items[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slots[i].rect.height);

            // Re-align perfectly with world space to guarantee no drift over many spins
            items[i].position = slots[i].position;
        }

        RectTransform bottom = items[items.Count - 1];
        items.RemoveAt(items.Count - 1);
        Destroy(bottom.gameObject);

        if (spinButton != null) spinButton.interactable = true;
        isSpinning = false; // Mark as stopped BEFORE telling the GameManager

        OnSpinComplete?.Invoke();
    }

    public void RemoveAndRespawnItems()
    {
        foreach (var item in items)
        {
            if (item != null) Destroy(item.gameObject);
        }
        items.Clear();

        for (int i = 0; i < slots.Length; i++)
        {
            RectTransform item = SpawnRandomItem(slots[i]);
            item.position = slots[i].position;
            items.Add(item);
        }
    }
}
