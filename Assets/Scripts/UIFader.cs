using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIFader : MonoBehaviour
{
    [Header("Fade Ayarlarý")]
    public float fadeDuration = 0.5f; // Efektin kaç saniye süreceði

    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Script çalýþtýðýnda Canvas Group'u otomatik bulur ve görünmez yapar
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    void Start()
    {
        // Obje sahneye ilk yaratýldýðýnda yumuþakça belirme efektini baþlatýr
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // Geçen zamana göre 0 ile 1 arasýnda saydamlýðý artýrýr
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null; // Bir sonraki frame'i bekle
        }

        canvasGroup.alpha = 1f; // Sonunda kesin olarak tam görünür yap
    }

    // Ýleride tahtayý temizlemek istersen diye Fade Out ve Silme özelliði
    public void FadeOutAndDestroy()
    {
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // 1'den 0'a doðru saydamlýðý azaltýr
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        Destroy(gameObject); // Tamamen görünmez olunca objeyi sahneden sil
    }
}