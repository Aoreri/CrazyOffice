using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class RequirementHighlighter : MonoBehaviour, IPointerClickHandler
{
    public UMLManager umlManager;
    public TextMeshProUGUI documentText;

    public enum PenType { None, ActorPen, UseCasePen }
    public PenType currentPen = PenType.ActorPen; // Baþlangýçta Mavi Kalem seçili

    public void OnPointerClick(PointerEventData eventData)
    {
        // Input.mousePosition YERÝNE eventData.position KULLANIYORUZ
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(documentText, eventData.position, eventData.pressEventCamera);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = documentText.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID();

            CheckAnswer(linkID, linkInfo);
        }
    }

    
    

    private void CheckAnswer(string linkID, TMP_LinkInfo linkInfo)
    {
        // Getting the text that the player clicked on (e.g., "Müþteriler")
        string selectedText = linkInfo.GetLinkText();

        if (currentPen == PenType.ActorPen && linkID == "actor")
        {
            ApplyHighlight(linkInfo, "#0000FF55"); // Blue highlight
            Debug.Log("True! Actor found.");

            // Tell UMLManager to draw the actor
            if (umlManager != null)
            {
                umlManager.DrawActor(selectedText);
            }
        }
        else if (currentPen == PenType.UseCasePen && linkID == "usecase")
        {
            ApplyHighlight(linkInfo, "#FFFF0055"); // Yellow highlight
            Debug.Log("True! Use Case found.");

            // Tell UMLManager to draw the use case
            if (umlManager != null)
            {
                umlManager.DrawUseCase(selectedText);
            }
        }
        else
        {
            ApplyHighlight(linkInfo, "#FF000055"); // Red highlight for error
            Debug.Log("Wrong choice! Stability is declining....");
        }
    }

    private void ApplyHighlight(TMP_LinkInfo linkInfo, string colorHex)
    {
        string fullText = documentText.text;
        string originalWord = linkInfo.GetLinkText();

        if (!originalWord.Contains("<mark"))
        {
            string highlightedWord = $"<mark={colorHex}>{originalWord}</mark>";
            documentText.text = fullText.Replace(originalWord, highlightedWord);
        }
    }

    // UI Butonlarýndan çaðýracaðýmýz kalem deðiþtirme fonksiyonlarý
    public void SelectActorPen() { currentPen = PenType.ActorPen; }
    public void SelectUseCasePen() { currentPen = PenType.UseCasePen; }
}