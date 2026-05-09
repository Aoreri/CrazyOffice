using UnityEngine;
using TMPro;
using System.Collections.Generic; 

public class UMLManager : MonoBehaviour
{
    public RectTransform umlBoard;
    public GameObject actorPrefab;
    public GameObject useCasePrefab;
    public GameObject linePrefab;

    
    private GameObject lastCreatedActor;
    private GameObject lastCreatedUseCase;

    private float leftX = -250f;      
    private float centerX = 0f;
    private float rightX = 250f;       

    // (Y-Spacing)
    private float actorY = 180f;       
    private float useCaseY = 200f;     
    private float stakeholderY = 180f; 



    public void DrawActor(string actorName)
    {
        GameObject newActor = Instantiate(actorPrefab, umlBoard);
        newActor.GetComponentInChildren<TextMeshProUGUI>().text = actorName;

        RectTransform rt = newActor.GetComponent<RectTransform>();

        if (actorName.ToLower().Contains("customer"))
        {
            rt.anchoredPosition = new Vector2(leftX, actorY);
            actorY -= 320f;

            
            lastCreatedActor = newActor;
        }
        else
        {
            rt.anchoredPosition = new Vector2(rightX, stakeholderY);
            stakeholderY -= 320f;

            //  if u want connect stakeholders caner
            // but usually connected customer and usecases
            lastCreatedActor = newActor;
        }
    }

    public void DrawUseCase(string useCaseName)
    {
        //create use case objection
        GameObject newUseCase = Instantiate(useCasePrefab, umlBoard);
        newUseCase.GetComponentInChildren<TextMeshProUGUI>().text = useCaseName;

        // scale position
        RectTransform useCaseRT = newUseCase.GetComponent<RectTransform>();
        useCaseRT.anchoredPosition = new Vector2(centerX, useCaseY);

        
        useCaseY -= 200f;

        

        // if there are an actor you connect first use case
        if (lastCreatedActor != null)
        {
            CreateLine(lastCreatedActor.GetComponent<RectTransform>(), useCaseRT);
        }

        // if there are written use case then you connect old use case to new use case
        if (lastCreatedUseCase != null)
        {
            CreateLine(lastCreatedUseCase.GetComponent<RectTransform>(), useCaseRT);
        }

        // save use case for new connection
        lastCreatedUseCase = newUseCase;
    }

    // Helper function to prevent code repetition.
    private void CreateLine(RectTransform start, RectTransform end)
    {
        if (linePrefab == null) return;
        GameObject newLine = Instantiate(linePrefab, umlBoard);
        //newLine.transform.SetAsFirstSibling();
        newLine.GetComponent<UMLConnectionLine>().Setup(start, end);
    }
}