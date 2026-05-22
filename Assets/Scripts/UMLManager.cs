using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UMLManager : MonoBehaviour
{
    [Header("UML Board Settings")]
    public RectTransform umlBoard;

    [Header("Prefabs")]
    public GameObject actorPrefab;
    public GameObject useCasePrefab;
    public GameObject linePrefab;

    [Header("Layout Settings")]
    public float horizontalOffset = 270f;
    public float verticalSpacing = 160f;
    public float topStartOffset = 150f;

    private Dictionary<string, GameObject> createdObjects = new Dictionary<string, GameObject>();

    private int leftActorCount = 0;
    private int rightActorCount = 0;
    private int useCaseCount = 0;

    // --- TAM ÝSTEDÝÐÝN ZÝNCÝR MANTIÐI ---
    private RectTransform pendingActor;       // Sadece kendisinden sonraki ÝLK Use Case'e baðlanmak için bekleyen aktör
    private RectTransform lastUseCaseInChain; // Use Case'leri yukarýdan aþaðýya birbirine baðlayan dikey zincir

    // RequirementHighlighter'ýn (actor_left için) çaðýracaðý yeni metod
    public void DrawPrimaryActor(string actorName)
    {
        DrawActorCore(actorName, true); // true = Sola çiz
    }

    // RequirementHighlighter'ýn (actor_right için) çaðýracaðý yeni metod
    public void DrawSecondaryActor(string actorName)
    {
        DrawActorCore(actorName, false); // false = Saða çiz
    }

    // Artýk kelime aramýyoruz, doðrudan dýþarýdan gelen komuta (isLeft) göre çiziyoruz
    private void DrawActorCore(string actorName, bool isLeft)
    {
        // Ayný obje tekrar seçilirse (yanlýþ sýra) cezalandýr ve sýfýrla
        if (createdObjects.ContainsKey(actorName))
        {
            ResetFlow();
            return;
        }

        GameObject newActor = Instantiate(actorPrefab, umlBoard);
        newActor.name = "Actor_" + actorName;
        createdObjects.Add(actorName, newActor);
        newActor.GetComponentInChildren<TextMeshProUGUI>().text = actorName;

        // isLeft deðiþkenine göre saða veya sola yerleþtirme
        float posX = isLeft ? -horizontalOffset : horizontalOffset;
        float startY = (umlBoard.rect.height / 2f) - topStartOffset;
        float posY = startY - ((isLeft ? leftActorCount++ : rightActorCount++) * verticalSpacing);

        RectTransform actorRect = newActor.GetComponent<RectTransform>();
        actorRect.anchoredPosition = new Vector2(posX, posY);

        // Aktör sahneye çýktý, kendine ait bir Use Case gelmesini bekliyor
        pendingActor = actorRect;
    }

    public void DrawUseCase(string useCaseName)
    {
        // Ayný obje tekrar seçilirse sýfýrla
        if (createdObjects.ContainsKey(useCaseName))
        {
            ResetFlow();
            return;
        }

        GameObject newUseCase = Instantiate(useCasePrefab, umlBoard);
        newUseCase.name = "UseCase_" + useCaseName;
        createdObjects.Add(useCaseName, newUseCase);
        newUseCase.GetComponentInChildren<TextMeshProUGUI>().text = useCaseName;

        float startY = (umlBoard.rect.height / 2f) - topStartOffset;
        float posY = startY - (useCaseCount * verticalSpacing);

        RectTransform useCaseRect = newUseCase.GetComponent<RectTransform>();
        useCaseRect.anchoredPosition = new Vector2(0f, posY);

        // 1. KURAL (KIRMIZI ÇÝZGÝLER): Use Case'ler HER ZAMAN bir öncekine baðlanýr
        if (lastUseCaseInChain != null)
        {
            CreateLine(lastUseCaseInChain, useCaseRect);
        }

        // 2. KURAL (AKTÖR ÇÝZGÝSÝ): Bekleyen bir Aktör varsa, bu Use Case'e baðlanýr ve görevi biter
        if (pendingActor != null)
        {
            CreateLine(pendingActor, useCaseRect);
            pendingActor = null; // Aktör baðlandý! Artýk bir sonraki Use Case'e sataþmayacak.
        }

        // Zinciri aþaðýya doðru uzatmak için bu Use Case'i hafýzaya alýyoruz
        lastUseCaseInChain = useCaseRect;
        useCaseCount++;
    }

    public void CreateLine(RectTransform start, RectTransform end)
    {
        if (linePrefab == null) return;
        GameObject newLine = Instantiate(linePrefab, umlBoard);
        newLine.name = "Line_" + start.name + "_to_" + end.name;
        newLine.transform.SetAsFirstSibling();

        var connection = newLine.GetComponent<UMLConnectionLine>();
        if (connection != null) connection.Setup(start, end);
    }

    private void ResetFlow()
    {
        Debug.Log("Hatalý Seçim: Akýþ Baþa Döndü!");
        pendingActor = null;
        lastUseCaseInChain = null;
    }
}