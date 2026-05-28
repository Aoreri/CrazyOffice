using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIFader : MonoBehaviour
{
    public float fadeDuration = 0.5f; // Efektin kaç saniye süreceði

    private CanvasGroup canvasGroup;

    void Awake()
    {
        
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    void Start()
    {
        
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
          
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null; 
        }

        canvasGroup.alpha = 1f; 
    }

    public void FadeOutAndDestroy()
    {
        StartCoroutine(FadeOutRoutine(true));
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInRoutine());
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutRoutine(false));
    }

    private IEnumerator FadeOutRoutine(bool shouldDestroy)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        if(shouldDestroy)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}