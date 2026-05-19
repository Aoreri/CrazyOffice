using UnityEngine;

public class AnalogSaatKontrol : MonoBehaviour
{
    [Header("Saat Kollari")]
    public RectTransform yelkovanNesnesi; // Uzun ok
    public RectTransform akrepNesnesi;    // Kısa ok

    [Header("Donus Hizlari")]
    [Tooltip("Yelkovanin 1 saniyede kac derece donecegini belirler.")]
    public float yelkovanHizi = 6f; 

    [Tooltip("Akrebin 1 saniyede kac derece donecegini belirler.")]
    public float akrepHizi = 0.5f;

    void Update()
    {
        // 1. Yelkovanı Döndür (Z ekseninde eksi yönde, yani saat yönünde)
        // Time.deltaTime oyunun akış saniyesidir.
        yelkovanNesnesi.Rotate(0f, 0f, -yelkovanHizi * Time.deltaTime);

        // 2. Akrebi Döndür (Daha yavaş bir şekilde saat yönünde)
        akrepNesnesi.Rotate(0f, 0f, -akrepHizi * Time.deltaTime);
    }
}