using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // YENİ EKLENDİ: Buton bas/çek algılaması için

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

    [Header("Audio Settings")]
    // YENİ EKLENDİ: Her bir slotun kendi ses sistemi
    public AudioSource audioSource;
    [Tooltip("Slot her hareket ettiğinde çalacak mekanik sesler")]
    public AudioClip[] tickSounds;
    [Tooltip("Butona parmak basıldığı an çalacak ses")]
    public AudioClip buttonPressSound;
    [Tooltip("Butondan parmak çekildiği an çalacak ses")]
    public AudioClip buttonReleaseSound;

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

    public void Initialize()
    {
        foreach (var item in items)
        {
            if (item != null) Destroy(item.gameObject);
        }
        items.Clear();

        Canvas.ForceUpdateCanvases();

        if (slots != null && slots.Length > 0)
        {
            System.Array.Sort(slots, (a, b) => b.position.y.CompareTo(a.position.y));
        }

        if (itemParent != null)
        {
            LayoutGroup lg = itemParent.GetComponent<LayoutGroup>();
            if (lg != null) lg.enabled = false;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            RectTransform item = SpawnRandomItem(slots[i]);
            item.position = slots[i].position;
            items.Add(item);
        }

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

            // --- YENİ EKLENDİ: BUTON BAS/ÇEK OTOMASYONU ---
            // Unity'den elle ayarlamana gerek kalmadan kod butonu dinlemeye başlıyor
            EventTrigger trigger = spinButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = spinButton.gameObject.AddComponent<EventTrigger>();

            // Basılma (Pressed) Anı
            EventTrigger.Entry downEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            downEntry.callback.AddListener((data) => { PlayButtonSound(buttonPressSound); });
            trigger.triggers.Add(downEntry);

            // Bırakılma (Released) Anı
            EventTrigger.Entry upEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            upEntry.callback.AddListener((data) => { PlayButtonSound(buttonReleaseSound); });
            trigger.triggers.Add(upEntry);
            // ----------------------------------------------

            hasSubscribedButton = true;
        }
    }

    // YENİ EKLENDİ: Buton seslerini sabit frekansta (1f) çalar
    private void PlayButtonSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = 1f; 
            audioSource.PlayOneShot(clip);
        }
    }

    // YENİ EKLENDİ: Slot tıkırtı seslerini Pitch Shifting (çeşitlilik) ile çalar
    private void PlayTickSound()
    {
        if (audioSource != null && tickSounds != null && tickSounds.Length > 0)
        {
            int randIndex = UnityEngine.Random.Range(0, tickSounds.Length);
            // Sesin robotikleşmemesi için her tıkta milimetrik tizleşme/kalınlaşma
            audioSource.pitch = UnityEngine.Random.Range(0.92f, 1.08f);
            audioSource.PlayOneShot(tickSounds[randIndex]);
        }
    }

    RectTransform SpawnRandomItem(RectTransform refSlot = null)
    {
        int index = UnityEngine.Random.Range(0, itemPrefabs.Length);
        GameObject obj = Instantiate(itemPrefabs[index], itemParent);
        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        if (refSlot != null)
        {
            rt.anchorMin = refSlot.anchorMin;
            rt.anchorMax = refSlot.anchorMax;
            rt.pivot = refSlot.pivot;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, refSlot.rect.width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, refSlot.rect.height);
        }

        return rt;
    }

    public void Spin()
    {
        if (isSpinning) return;
        
        // YENİ EKLENDİ: Slot dönmeye başladığı an tıkırtıyı patlat
        PlayTickSound(); 
        
        StartCoroutine(SpinCoroutine());
    }

    IEnumerator SpinCoroutine()
    {
        isSpinning = true; 
        if (spinButton != null) spinButton.interactable = false;

        Vector3 localPos0 = itemParent.InverseTransformPoint(slots[0].position);
        Vector3 localPos1 = itemParent.InverseTransformPoint(slots[1].position);
        float slotDistance = Mathf.Abs(localPos0.y - localPos1.y);

        RectTransform newItem = SpawnRandomItem(slots[0]);
        newItem.position = slots[0].position;
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

            for (int i = 0; i < items.Count; i++)
            {
                items[i].anchoredPosition = startPositions[i] + Vector2.down * moveY;
            }

            yield return null;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            items[i].anchorMin = slots[i].anchorMin;
            items[i].anchorMax = slots[i].anchorMax;
            items[i].pivot = slots[i].pivot;
            items[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slots[i].rect.width);
            items[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slots[i].rect.height);
            items[i].position = slots[i].position;
        }

        RectTransform bottom = items[items.Count - 1];
        items.RemoveAt(items.Count - 1);
        Destroy(bottom.gameObject);

        if (spinButton != null) spinButton.interactable = true;
        isSpinning = false; 

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