using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ModemPuzzle : Puzzle
{
    public Image lights;

    [Header("Antennas")]
    public AntennaRotate anten1;
    public AntennaRotate anten2;

    [Header("UI Transition")]
    public Image targetImage;
    public float fadeDuration = 2.0f;

    public float acceptableRange = 0.1f;

    public float designatedValue1, designatedValue2;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    [Tooltip("Tek bir tick/bip sesi")]
    public AudioClip tickSound;
    [Tooltip("Sinyal bulununca çalacak başarı sesi")]
    public AudioClip successSound;

    [Tooltip("Sinyal çok zayıfken tickler arası saniye bazında bekleme süresi")]
    public float maxTickInterval = 1.0f;
    [Tooltip("Sinyal en güçlüyken tickler arası minimum bekleme süresi (Çok düşürme, üst üste biner)")]
    public float minTickInterval = 0.08f;

    private float tickTimer = 0f;
    private bool isSolved = false; // Bulmaca çözüldüğünde Update'in tekrar tekrar çalışmasını engeller

    void Start()
    {
        anten1.antennaDelta = UnityEngine.Random.value;
        anten2.antennaDelta = UnityEngine.Random.value;

        designatedValue1 = UnityEngine.Random.value;
        designatedValue2 = UnityEngine.Random.value;
    }

    void Update()
    {
        // Bulmaca çözüldüyse artık sinyal hesaplama veya ses çalma
        if (isSolved) return; 

        float diff1 = Mathf.Abs(anten1.antennaDelta - designatedValue1);
        float diff2 = Mathf.Abs(anten2.antennaDelta - designatedValue2);

        float signal1 = Mathf.InverseLerp(acceptableRange, 0, diff1);
        float signal2 = Mathf.InverseLerp(acceptableRange, 0, diff2);

        float solveDelta = (signal1 + signal2) / 2f;

        lights.fillAmount = solveDelta;

        // --- SES TETİKLEME MANTIĞI (Ritim Kontrolü) ---
        // Sinyal belli bir seviyenin üstündeyse (örn: %5) ticklemeye başla
        if (solveDelta > 0.05f)
        {
            tickTimer -= Time.deltaTime;

            if (tickTimer <= 0f)
            {
                // Sinyal gücüne göre sıradaki tick'in ne zaman çalacağını hesapla
                float currentInterval = Mathf.Lerp(maxTickInterval, minTickInterval, solveDelta);
                tickTimer = currentInterval;

                if (audioSource != null && tickSound != null)
                {
                    // Tansiyonu artırmak için hedefe yaklaştıkça frekansı hafif tizleştir (0.8'den 1.2'ye)
                    audioSource.pitch = Mathf.Lerp(0.8f, 1.2f, solveDelta);
                    audioSource.PlayOneShot(tickSound);
                }
            }
        }

        // --- BULMACA ÇÖZÜLME KONTROLÜ ---
        if (solveDelta >= 0.98f)
        {
            isSolved = true; // Tekrar tekrar girmemesi için kilitliyoruz

            // Başarı sesini orijinal pitch değerinde (1f) patlatıyoruz
            if (audioSource != null && successSound != null)
            {
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(successSound);
            }

            anten1.enabled = false;
            anten2.enabled = false;
            StartCoroutine(FadeImageOut());
        }
    }

    IEnumerator FadeImageOut()
    {
        float startAlpha = targetImage.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);

            Color c = targetImage.color;
            c.a = newAlpha;
            targetImage.color = c;

            yield return null;
        }

        Color finalColor = targetImage.color;
        finalColor.a = 0f;
        targetImage.color = finalColor;

        yield return new WaitForSeconds(2);
        EndPuzzle();
    }

    protected override void OnEndPuzzle() { }

    protected override void OnStartPuzzle() { }
}