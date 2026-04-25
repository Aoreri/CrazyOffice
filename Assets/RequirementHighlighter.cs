using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class RequirementHighlighter : MonoBehaviour, IPointerClickHandler
{
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
        if (currentPen == PenType.ActorPen && linkID == "actor")
        {
            ApplyHighlight(linkInfo, "#0000FF55"); // Mavi
            Debug.Log("True! Actor found.");
        }
        else if (currentPen == PenType.UseCasePen && linkID == "usecase")
        {
            ApplyHighlight(linkInfo, "#FFFF0055"); // Sarý
            Debug.Log("True! Use Case found.");
        }
        
        else
        {
            ApplyHighlight(linkInfo, "#FF000055"); // Kýrmýzý (Hata)
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