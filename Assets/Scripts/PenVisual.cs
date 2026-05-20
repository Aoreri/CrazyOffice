using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class PenAnimator : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Quaternion originalRotation;

    [Header("Animasyon Ayarlarý")]
    public float moveUpAmount = 30f;       // Ne kadar yukarý çýkacak
    public float tiltAngle = 15f;          // Ucu ne kadar kalkacak (Z ekseninde dönüþ)
    public float animationDuration = 0.2f; // Hareketin hýzý

    private bool isSelected = false;
    private Coroutine activeAnimation;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        originalRotation = rectTransform.localRotation;
    }

    public void SelectPen()
    {
        if (!isSelected)
        {
            isSelected = true;
            if (activeAnimation != null) StopCoroutine(activeAnimation);

            // Hedefler: Hem yukarý çýk, hem de Z ekseninde ucunu kaldýr
            Vector2 targetPos = originalPosition + new Vector2(0f, moveUpAmount);
            Quaternion targetRot = originalRotation * Quaternion.Euler(0, 0, tiltAngle);

            activeAnimation = StartCoroutine(AnimatePen(targetPos, targetRot, animationDuration));
        }
    }

    public void DeselectPen()
    {
        if (isSelected)
        {
            isSelected = false;
            if (activeAnimation != null) StopCoroutine(activeAnimation);

            // Eski pozisyon ve rotasyona geri dön
            activeAnimation = StartCoroutine(AnimatePen(originalPosition, originalRotation, animationDuration));
        }
    }

    // Hem pozisyonu hem rotasyonu ayný anda pürüzsüzce deðiþtiren sistem
    IEnumerator AnimatePen(Vector2 targetPos, Quaternion targetRot, float duration)
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Quaternion startRot = rectTransform.localRotation;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // SmoothStep ile hareketin baþý ve sonu daha yumuþak olur (kaliteli oyun hissi)
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            rectTransform.localRotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return null;
        }

        // Animasyon bitince tam hedefe oturduðundan emin ol
        rectTransform.anchoredPosition = targetPos;
        rectTransform.localRotation = targetRot;
    }
}