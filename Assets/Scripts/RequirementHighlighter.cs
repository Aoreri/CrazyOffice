using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RequirementHighlighter : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public StageManager stageManager;
    public UMLManager umlManager;
    public TextMeshProUGUI documentText;
    public RectTransform highlightContainer;
    public GameObject highlightPrefab;

    [Header("Random Text Settings")]
    [TextArea(5, 10)]
    public string[] randomTexts;

    [Header("Highlight (Marker) Settings")]
    [Range(0.5f, 3f)] public float highlightHeightMultiplier = 1.3f;
    [Range(0f, 2f)] public float highlightWidthPaddingMultiplier = 0.6f;
    [Range(-2f, 2f)] public float highlightVerticalOffset = 0f;

    [Header("Underline Settings")]
    public Color underlineColor = Color.black;
    public float underlineThickness = 3f;
    public float underlineOffset = 2f;

    [Header("Audio Settings")]
    // GÜNCELLENDİ: Artık tek bir ses değil, birden fazla sesi tutacak bir dizi (array)
    public AudioSource audioSource;
    public AudioClip[] markerSounds;

    public enum PenType { None, ActorPen, UseCasePen }
    public PenType currentPen = PenType.ActorPen;

    private Dictionary<int, GameObject> activeHighlights = new Dictionary<int, GameObject>();
    private HashSet<int> correctAnswers = new HashSet<int>();

    private GameObject activeUnderline;
    private int hoveredLinkIndex = -1;
    private bool isPointerOverText = false;
    private Camera activeCamera;

    private int currentScenarioIndex = 0;

    void Start()
    {
        activeUnderline = new GameObject("HoverUnderline");
        activeUnderline.transform.SetParent(highlightContainer, false);
        activeUnderline.SetActive(false);

        SetRandomText();
    }

    public void SetRandomText()
    {
        if (randomTexts != null && randomTexts.Length > 0)
        {
            currentScenarioIndex = Random.Range(0, randomTexts.Length);
            documentText.text = randomTexts[currentScenarioIndex];
            ClearAllHighlights();
        }
    }

    private void ClearAllHighlights()
    {
        foreach (var highlight in activeHighlights.Values)
        {
            if (highlight != null) Destroy(highlight);
        }
        activeHighlights.Clear();
        correctAnswers.Clear();
        ClearUnderline();
    }

    void Update()
    {
        if (!isPointerOverText) return;

        Camera cam = documentText.canvas.renderMode == RenderMode.ScreenSpaceOverlay
       ? null
       : documentText.canvas.worldCamera;

        Vector3 localMousePos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            documentText.rectTransform,
            Input.mousePosition,
            cam,
            out localMousePos
        );

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            documentText, localMousePos, cam);

        if (linkIndex != -1)
        {
            if (linkIndex != hoveredLinkIndex)
            {
                hoveredLinkIndex = linkIndex;
                UpdateUnderline(linkIndex);
            }
        }
        else
        {
            ClearUnderline();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOverText = true;
        activeCamera = eventData.enterEventCamera;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOverText = false;
        ClearUnderline();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Camera cam = documentText.canvas.renderMode == RenderMode.ScreenSpaceOverlay
        ? null
        : documentText.canvas.worldCamera;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            documentText, eventData.position, cam);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = documentText.textInfo.linkInfo[linkIndex];
            CheckAnswer(linkInfo.GetLinkID(), linkInfo, linkIndex);
        }
    }

    private void CheckAnswer(string linkID, TMP_LinkInfo linkInfo, int linkIndex)
    {
        if (correctAnswers.Contains(linkIndex)) return;

        // --- GÜNCELLENDİ: Array içinden rastgele ses seçme mantığı eklendi ---
        if (audioSource != null && markerSounds != null && markerSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, markerSounds.Length);
            audioSource.pitch = Random.Range(0.9f, 1.1f); 
            audioSource.PlayOneShot(markerSounds[randomIndex]);
        }
        // -----------------------------------------------------------------------

        bool isCorrect = false;
        string selectedText = linkInfo.GetLinkText();

        if (currentPen == PenType.ActorPen && linkID == "actor_left")
        {
            ApplyHighlight(linkInfo, linkIndex, new Color(0.2f, 0.6f, 1f, 0.9f));
            if (umlManager != null) umlManager.DrawPrimaryActor(selectedText);
            isCorrect = true;
        }
        else if (currentPen == PenType.ActorPen && linkID == "actor_right")
        {
            ApplyHighlight(linkInfo, linkIndex, new Color(0.2f, 0.6f, 1f, 0.9f));
            if (umlManager != null) umlManager.DrawSecondaryActor(selectedText);
            isCorrect = true;
        }
        else if (currentPen == PenType.UseCasePen && linkID == "usecase")
        {
            ApplyHighlight(linkInfo, linkIndex, new Color(1f, 0.9f, 0.2f, 0.9f));
            if (umlManager != null) umlManager.DrawUseCase(selectedText);
            isCorrect = true;
        }
        else if (currentPen != PenType.None)
        {
            // --- YANLIŞ CEVAP DURUMU ---
            // 1. İşaretlemeyi kırmızı yap
            ApplyHighlight(linkInfo, linkIndex, new Color(1f, 0.2f, 0.2f, 0.9f));

            // 2. Süreye 10 saniye ceza ekle
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.ApplyPenalty(10f);
            }
        }

        if (isCorrect)
        {
            // İlk doğru cevap verildiğinde süreyi başlat
            if (correctAnswers.Count == 0 && TimeManager.Instance != null)
                TimeManager.Instance.StartTimer();

            correctAnswers.Add(linkIndex);

            int totalLinks = documentText.textInfo.linkCount;

            // Tüm doğru cevaplar bulunduğunda
            if (correctAnswers.Count == totalLinks)
            {
                StartCoroutine(CompleteAndStartFirstQuest());
            }
        }
    }

    private IEnumerator CompleteAndStartFirstQuest()
    {
        yield return new WaitForSeconds(1.5f);

        if (stageManager != null)
        {
            stageManager.selectedScenarioIndex = currentScenarioIndex;
            stageManager.StartScenarioQuests();
        }
    }

    private void ApplyHighlight(TMP_LinkInfo linkInfo, int linkIndex, Color highlightColor)
    {
        if (activeHighlights.ContainsKey(linkIndex))
        {
            foreach (Transform child in activeHighlights[linkIndex].transform)
                Destroy(child.gameObject);
            Destroy(activeHighlights[linkIndex]);
            activeHighlights.Remove(linkIndex);
        }

        GameObject container = new GameObject($"Highlight_{linkIndex}");
        container.transform.SetParent(highlightContainer, false);
        container.transform.SetAsFirstSibling();
        activeHighlights[linkIndex] = container;

        TMP_TextInfo textInfo = documentText.textInfo;
        int firstChar = linkInfo.linkTextfirstCharacterIndex;
        int lastChar = firstChar + linkInfo.linkTextLength - 1;

        var lineBounds = new Dictionary<int, (float minX, float maxX, float minY, float maxY)>();

        for (int i = firstChar; i <= lastChar; i++)
        {
            TMP_CharacterInfo ci = textInfo.characterInfo[i];
            if (!ci.isVisible) continue;

            int line = ci.lineNumber;
            float left = ci.bottomLeft.x;
            float right = ci.topRight.x;
            float bot = ci.bottomLeft.y;
            float top = ci.topRight.y;

            if (lineBounds.ContainsKey(line))
            {
                var b = lineBounds[line];
                lineBounds[line] = (
                    Mathf.Min(b.minX, left),
                    Mathf.Max(b.maxX, right),
                    Mathf.Min(b.minY, bot),
                    Mathf.Max(b.maxY, top)
                );
            }
            else
            {
                lineBounds[line] = (left, right, bot, top);
            }
        }

        float fontSize = textInfo.characterInfo[firstChar].pointSize;

        foreach (var kvp in lineBounds)
        {
            var b = kvp.Value;

            float width = b.maxX - b.minX;

            Vector3 localCenter = new Vector3(
                b.minX + width * 0.5f,
                b.minY + (b.maxY - b.minY) * 0.5f,
                0f
            );

            localCenter.y += highlightVerticalOffset * fontSize;

            Vector3 worldPos = documentText.transform.TransformPoint(localCenter);
            Vector3 containerLocal = highlightContainer.InverseTransformPoint(worldPos);

            GameObject rect = Instantiate(highlightPrefab, container.transform);
            rect.transform.SetAsFirstSibling();

            RectTransform rt = rect.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;

            rt.localPosition = containerLocal;

            float finalHeight = fontSize * highlightHeightMultiplier;
            float finalWidth = width + (fontSize * highlightWidthPaddingMultiplier);

            rt.sizeDelta = new Vector2(finalWidth, finalHeight);

            Image img = rect.GetComponent<Image>();
            img.color = highlightColor;
            img.raycastTarget = false;
        }
    }

    private void UpdateUnderline(int linkIndex)
    {
        foreach (Transform child in activeUnderline.transform)
            Destroy(child.gameObject);
        activeUnderline.SetActive(true);

        TMP_LinkInfo linkInfo = documentText.textInfo.linkInfo[linkIndex];
        TMP_TextInfo textInfo = documentText.textInfo;

        int firstChar = linkInfo.linkTextfirstCharacterIndex;
        int lastChar = firstChar + linkInfo.linkTextLength - 1;

        var lineBounds = new Dictionary<int, (float minX, float maxX, float bottomY)>();

        for (int i = firstChar; i <= lastChar; i++)
        {
            TMP_CharacterInfo ci = textInfo.characterInfo[i];
            if (!ci.isVisible) continue;

            int line = ci.lineNumber;
            float left = ci.bottomLeft.x;
            float right = ci.topRight.x;
            float bot = ci.bottomLeft.y;

            if (lineBounds.ContainsKey(line))
            {
                var b = lineBounds[line];
                lineBounds[line] = (
                    Mathf.Min(b.minX, left),
                    Mathf.Max(b.maxX, right),
                    Mathf.Min(b.bottomY, bot)
                );
            }
            else
            {
                lineBounds[line] = (left, right, bot);
            }
        }

        foreach (var kvp in lineBounds)
        {
            var b = kvp.Value;
            float width = b.maxX - b.minX;

            Vector3 localPos = new Vector3(
                b.minX + width * 0.5f,
                b.bottomY - underlineOffset,
                0f
            );

            Vector3 worldPos = documentText.transform.TransformPoint(localPos);
            Vector3 containerLocal = highlightContainer.InverseTransformPoint(worldPos);

            GameObject lineRect = new GameObject("UnderlineLine");
            lineRect.transform.SetParent(activeUnderline.transform, false);

            Image img = lineRect.AddComponent<Image>();
            img.color = underlineColor;
            img.raycastTarget = false;

            RectTransform rt = lineRect.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localPosition = containerLocal;
            rt.sizeDelta = new Vector2(width, underlineThickness);
        }

        activeUnderline.transform.SetAsLastSibling();
    }

    private void ClearUnderline()
    {
        hoveredLinkIndex = -1;
        if (activeUnderline != null)
        {
            foreach (Transform child in activeUnderline.transform)
                Destroy(child.gameObject);
            activeUnderline.SetActive(false);
        }
    }

    public void SelectActorPen() { currentPen = PenType.ActorPen; }
    public void SelectUseCasePen() { currentPen = PenType.UseCasePen; }
}