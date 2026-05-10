using UnityEngine;

public abstract class Puzzle : MonoBehaviour
{
    public string puzzleName;
    public GameObject prefab;
    
    private float startTime;

    private GameObject instance;

    public void StartPuzzle()
    {
        

        GameObject canvasObj = GameObject.FindGameObjectWithTag("UI");
        Transform canvasTransform = canvasObj.transform;

        if (prefab != null && canvasTransform != null)
        {
            instance = Instantiate(prefab, canvasTransform);
        }
        else
        {
            Debug.LogWarning("Prefab or Canvas Parent is missing!");
        }

        startTime = Time.time;
        Debug.Log(puzzleName + " started.");
        
        gameObject.SetActive(true);
        OnStartPuzzle();

    }

    public void EndPuzzle()
    {
        Debug.Log(puzzleName + " ended. " + (Time.time - startTime));

        gameObject.SetActive(false);

        if (instance != null)
        {
            Destroy(instance);
        }

        OnEndPuzzle();
    }

    protected abstract void OnStartPuzzle();
    protected abstract void OnEndPuzzle();
}