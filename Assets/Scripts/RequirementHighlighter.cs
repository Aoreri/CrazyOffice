using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class RequirementHighlighter : MonoBehaviour, IPointerClickHandler
{
    public UMLManager umlManager;
    public TextMeshProUGUI documentText;

    
    public RectTransform highlightContainer;

    public GameObject highlightPrefab;

    public enum PenType { None, ActorPen, UseCasePen }
    public PenType currentPen = PenType.ActorPen;

    private Dictionary<int, GameObject> activeHighlights = new Dictionary<int, GameObject>();

    
    private HashSet<int> correctAnswers = new HashSet<int>();

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(documentText, eventData.position, eventData.pressEventCamera);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = documentText.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID();

            CheckAnswer(linkID, linkInfo, linkIndex);
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
            Debug.Log("Wrong choice!");
        }
    }

    private void ApplyHighlight(TMP_LinkInfo linkInfo, int linkIndex, Color highlightColor)
    {
        GameObject highlightObj;

        
        if (activeHighlights.ContainsKey(linkIndex))
        {
            highlightObj = activeHighlights[linkIndex];
        }
        else
        {
           
            highlightObj = Instantiate(highlightPrefab, highlightContainer);
            activeHighlights.Add(linkIndex, highlightObj);

            
            TMP_TextInfo textInfo = documentText.textInfo;
            int firstCharIndex = linkInfo.linkTextfirstCharacterIndex;
            int lastCharIndex = firstCharIndex + linkInfo.linkTextLength - 1;

            Vector3 bottomLeft = textInfo.characterInfo[firstCharIndex].bottomLeft;
            Vector3 topRight = textInfo.characterInfo[lastCharIndex].topRight;

            float width = topRight.x - bottomLeft.x;
            Vector3 localCenter = (bottomLeft + topRight) / 2;

            RectTransform highlightRect = highlightObj.GetComponent<RectTransform>();

            highlightRect.localPosition = new Vector3(localCenter.x, localCenter.y - 12f, 0f);

            
            highlightRect.sizeDelta = new Vector2(width + 70f, 45f);
        }

        
        UnityEngine.UI.Image highlightImage = highlightObj.GetComponent<UnityEngine.UI.Image>();
        highlightImage.color = highlightColor;
    }

    public void SelectActorPen() { currentPen = PenType.ActorPen; }
    public void SelectUseCasePen() { currentPen = PenType.UseCasePen; }
}