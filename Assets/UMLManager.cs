using UnityEngine;
using TMPro;

public class UMLManager : MonoBehaviour
{
    public RectTransform umlBoard;
    public GameObject actorPrefab;
    public GameObject useCasePrefab;

    // Different X positions for Left Actors, Center Use Cases, and Right Stakeholders
    private float leftX = -150f;
    private float centerX = 0f;
    private float rightX = 150f;

    // Separate vertical spacing for each column
    private float actorY = 300f;
    private float useCaseY = 300f;
    private float stakeholderY = 300f;

    public void DrawActor(string actorName)
    {
        GameObject newActor = Instantiate(actorPrefab, umlBoard);

        // Find the Text component in the prefab and set the name
        newActor.GetComponentInChildren<TextMeshProUGUI>().text = actorName;

        RectTransform rt = newActor.GetComponent<RectTransform>();

        // Logic: If it's a customer, put it on the left. Otherwise, on the right.
        if (actorName.ToLower().Contains("müþteri"))
        {
            rt.anchoredPosition = new Vector2(leftX, actorY);
            actorY -= 150f; // Space for the next actor
        }
        else
        {
            rt.anchoredPosition = new Vector2(rightX, stakeholderY);
            stakeholderY -= 150f;
        }
    }

    public void DrawUseCase(string useCaseName)
    {
        GameObject newUseCase = Instantiate(useCasePrefab, umlBoard);
        newUseCase.GetComponentInChildren<TextMeshProUGUI>().text = useCaseName;

        // Use Cases always go to the center
        newUseCase.GetComponent<RectTransform>().anchoredPosition = new Vector2(centerX, useCaseY);
        useCaseY -= 100f;
    }
}