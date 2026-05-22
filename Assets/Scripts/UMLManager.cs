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

    
    private RectTransform pendingActor;       
    private RectTransform lastUseCaseInChain; 

    
    public void DrawPrimaryActor(string actorName)
    {
        DrawActorCore(actorName, true); // true 
    }

    
    public void DrawSecondaryActor(string actorName)
    {
        DrawActorCore(actorName, false); // false
    }

    
    private void DrawActorCore(string actorName, bool isLeft)
    {
        
        if (createdObjects.ContainsKey(actorName))
        {
            ResetFlow();
            return;
        }

        GameObject newActor = Instantiate(actorPrefab, umlBoard);
        newActor.name = "Actor_" + actorName;
        createdObjects.Add(actorName, newActor);
        newActor.GetComponentInChildren<TextMeshProUGUI>().text = actorName;

        
        float posX = isLeft ? -horizontalOffset : horizontalOffset;
        float startY = (umlBoard.rect.height / 2f) - topStartOffset;
        float posY = startY - ((isLeft ? leftActorCount++ : rightActorCount++) * verticalSpacing);

        RectTransform actorRect = newActor.GetComponent<RectTransform>();
        actorRect.anchoredPosition = new Vector2(posX, posY);

        
        pendingActor = actorRect;
    }

    public void DrawUseCase(string useCaseName)
    {
        
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

        
        if (lastUseCaseInChain != null)
        {
            CreateLine(lastUseCaseInChain, useCaseRect);
        }

        
        if (pendingActor != null)
        {
            CreateLine(pendingActor, useCaseRect);
            pendingActor = null; 
        }

        
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