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

    void Start()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            RectTransform item = SpawnRandomItem();
            item.anchoredPosition = slots[i].anchoredPosition;
            items.Add(item);
        }

        // The individual button still works!
        spinButton.onClick.AddListener(Spin);
    }

    RectTransform SpawnRandomItem()
    {
        int index = UnityEngine.Random.Range(0, itemPrefabs.Length);
        GameObject obj = Instantiate(itemPrefabs[index], itemParent);
        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        return rt;
    }

    public void Spin()
    {
        if (!spinButton.interactable) return;
        StartCoroutine(SpinCoroutine());
    }

    IEnumerator SpinCoroutine()
    {
        isSpinning = true; // Mark as spinning
        spinButton.interactable = false;

        float slotDistance = Mathf.Abs(slots[0].anchoredPosition.y - slots[1].anchoredPosition.y);
        RectTransform newItem = SpawnRandomItem();

        Vector2 spawnPos = slots[0].anchoredPosition;
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
            float curveT = spinCurve.Evaluate(t);
            float moveY = Mathf.Lerp(0, slotDistance, curveT);

            for (int i = 0; i < items.Count; i++)
            {
                items[i].anchoredPosition = startPositions[i] + Vector2.down * moveY;
            }

            yield return null;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            items[i].anchoredPosition = slots[i].anchoredPosition;
        }

        RectTransform bottom = items[items.Count - 1];
        items.RemoveAt(items.Count - 1);
        Destroy(bottom.gameObject);

        spinButton.interactable = true;
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
            RectTransform item = SpawnRandomItem();
            item.anchoredPosition = slots[i].anchoredPosition;
            items.Add(item);
        }
    }
}