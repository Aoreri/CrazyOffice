using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RequirementHighlighter : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public UMLManager umlManager;
    public TextMeshProUGUI documentText;
    public RectTransform highlightContainer;
    public GameObject highlightPrefab;

    [Header("Underline Settings")]
    public Color underlineColor = Color.black;
    public float underlineThickness = 3f;
    public float underlineOffset = 2f; // How far below the text the line sits

    public enum PenType { None, ActorPen, UseCasePen }
    public PenType currentPen = PenType.ActorPen;

    private Dictionary<int, GameObject> activeHighlights = new Dictionary<int, GameObject>();
    private HashSet<int> correctAnswers = new HashSet<int>();

    // Hover underline tracking
    private GameObject activeUnderline;
    private int hoveredLinkIndex = -1;
    private bool isPointerOverText = false;
    private Camera activeCamera;

    void Start()
    {
        activeUnderline = new GameObject("HoverUnderline");
        activeUnderline.transform.SetParent(highlightContainer, false);
        // No Image here anymore — child rects handle visuals
        activeUnderline.SetActive(false);
    }

    void Update()
    {
        // Only run intersection math if the mouse is actually inside the text block bounds
        if (!isPointerOverText) return;

        Camera cam = documentText.canvas.renderMode == RenderMode.ScreenSpaceOverlay
       ? null
       : documentText.canvas.worldCamera;

        // Convert screen position to the text object's local space first
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
        activeCamera = eventData.enterEventCamera; // Cache the camera for the Update loop
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

        string selectedText = linkInfo.GetLinkText();

        if (currentPen == PenType.ActorPen && linkID == "actor")
        {
            ApplyHighlight(linkInfo, linkIndex, new Color(0.2f, 0.6f, 1f, 0.9f));
            correctAnswers.Add(linkIndex);
            if (umlManager != null) umlManager.DrawActor(selectedText);
        }
        else if (currentPen == PenType.UseCasePen && linkID == "usecase")
        {
            ApplyHighlight(linkInfo, linkIndex, new Color(1f, 0.9f, 0.2f, 0.9f));
            correctAnswers.Add(linkIndex);
            if (umlManager != null) umlManager.DrawUseCase(selectedText);
        }
        else
        {
            ApplyHighlight(linkInfo, linkIndex, new Color(1f, 0.2f, 0.2f, 0.9f));
        }
    }

    private void ApplyHighlight(TMP_LinkInfo linkInfo, int linkIndex, Color highlightColor)
    {
        // Remove old highlights for this link (could be multiple rects from previous multiline)
        if (activeHighlights.ContainsKey(linkIndex))
        {
            foreach (Transform child in activeHighlights[linkIndex].transform)
                Destroy(child.gameObject);
            Destroy(activeHighlights[linkIndex]);
            activeHighlights.Remove(linkIndex);
        }

        // Create a container GameObject to hold all line-rects for this link
        GameObject container = new GameObject($"Highlight_{linkIndex}");
        container.transform.SetParent(highlightContainer, false);
        container.transform.SetAsFirstSibling();
        activeHighlights[linkIndex] = container;

        TMP_TextInfo textInfo = documentText.textInfo;
        int firstChar = linkInfo.linkTextfirstCharacterIndex;
        int lastChar = firstChar + linkInfo.linkTextLength - 1;

        // --- Group characters by line index ---
        // Key = lineIndex, Value = (minX, maxX, minY, maxY) in TMP local space
        var lineBounds = new Dictionary<int, (float minX, float maxX, float minY, float maxY)>();

        for (int i = firstChar; i <= lastChar; i++)
        {
            TMP_CharacterInfo ci = textInfo.characterInfo[i];

            // Skip invisible characters (spaces, line breaks)
            if (!ci.isVisible) continue;

            int line = ci.lineNumber;
            float bLeft = ci.bottomLeft.x;
            float bRight = ci.topRight.x;
            float bBot = ci.bottomLeft.y;
            float bTop = ci.topRight.y;

            if (lineBounds.ContainsKey(line))
            {
                var b = lineBounds[line];
                lineBounds[line] = (
                    Mathf.Min(b.minX, bLeft),
                    Mathf.Max(b.maxX, bRight),
                    Mathf.Min(b.minY, bBot),
                    Mathf.Max(b.maxY, bTop)
                );
            }
            else
            {
                lineBounds[line] = (bLeft, bRight, bBot, bTop);
            }
        }

        // --- Spawn one Image rect per line ---
        foreach (var kvp in lineBounds)
        {
            var b = kvp.Value;

            float width = b.maxX - b.minX;
            float height = b.maxY - b.minY;

            // Local center in TMP's coordinate space
            Vector3 localCenter = new Vector3(
                b.minX + width * 0.5f,
                b.minY + height * 0.5f,
                0f
            );

            // Convert to world, then to highlightContainer local space
            Vector3 worldPos = documentText.transform.TransformPoint(localCenter);
            Vector3 containerLocal = highlightContainer.InverseTransformPoint(worldPos);

            GameObject rect = Instantiate(highlightPrefab, container.transform);
            rect.transform.SetAsFirstSibling();

            RectTransform rt = rect.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localPosition = containerLocal;
            rt.sizeDelta = new Vector2(width + 10f, height + 6f); // small padding

            Image img = rect.GetComponent<Image>();
            img.color = highlightColor;
            img.raycastTarget = false;
        }
    }

    private void UpdateUnderline(int linkIndex)
    {
        // Clear previous underline rects
        foreach (Transform child in activeUnderline.transform)
            Destroy(child.gameObject);
        activeUnderline.SetActive(true);

        TMP_LinkInfo linkInfo = documentText.textInfo.linkInfo[linkIndex];
        TMP_TextInfo textInfo = documentText.textInfo;

        int firstChar = linkInfo.linkTextfirstCharacterIndex;
        int lastChar = firstChar + linkInfo.linkTextLength - 1;

        // Group characters by line (same approach as ApplyHighlight)
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
                    Mathf.Min(b.bottomY, bot)   // lowest point of line
                );
            }
            else
            {
                lineBounds[line] = (left, right, bot);
            }
        }

        // Spawn one underline Image per line
        foreach (var kvp in lineBounds)
        {
            var b = kvp.Value;
            float width = b.maxX - b.minX;

            // Bottom-center in TMP local space, shifted down by offset
            Vector3 localPos = new Vector3(
                b.minX + width * 0.5f,
                b.bottomY - underlineOffset,
                0f
            );

            // Convert to world → highlightContainer local (same as ApplyHighlight)
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