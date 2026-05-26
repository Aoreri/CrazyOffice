using System.Collections;
using TMPro;
using UnityEngine;

public class CardSwipe : Puzzle
{
    public CardSwipeHandler cardSwipe;
   
    public float allowedSwipeTimeMin = 0.8f;
    public float allowedSwipeTimeMax = 1.2f;

    public float textChangeTime = 1.25f;

    //Least allowed swap
    public RectTransform cardSwipeLast;

    public TMPro.TextMeshProUGUI deviceText;

    public GameObject greenLightOn, greenLightOff, redLightOn, redLightOff;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    [Tooltip("Kart hızlı, yavaş veya ters çekildiğinde çalacak olan hata/reddetme sesi")]
    public AudioClip errorSound;
    [Tooltip("Kart doğru hızda çekildiğinde çalacak olan olumlu onaylama sesi")]
    public AudioClip successSound;

    private Coroutine replaceCoroutine;
    private Coroutine lightCoroutine;

    void Start()
    {
        cardSwipe.OnSwipeRight += HandleRightSwipe;
    }

    void HandleRightSwipe(float swipeTime)
    {
        float swipeCordDiff = (cardSwipe.transform.position.x - cardSwipeLast.transform.position.x);

        if (swipeCordDiff < 0)
        {
            SwitchLight(false);
            ReplaceText("INVALID READING");
            PlayErrorSound(); // HATA SESİ
            return;
        }

        if(swipeTime < allowedSwipeTimeMin)
        {
            SwitchLight(false);
            ReplaceText("TOO FAST");
            PlayErrorSound(); // HATA SESİ
            return;
        }

        if (swipeTime >= allowedSwipeTimeMax)
        {
            SwitchLight(false);
            ReplaceText("TOO SLOW");
            PlayErrorSound(); // HATA SESİ
            return;
        }

        SwitchLight(true);
        ReplaceText("LETS GO");

        // BAŞARI SESİ
        if (audioSource != null && successSound != null)
        {
            audioSource.pitch = 1f; // Başarı sesinde orijinal frekansı koru
            audioSource.PlayOneShot(successSound);
        }

        EndPuzzle();
        return;
    }

    // Hata sesini pitch modülasyonu ile çalan yardımcı fonksiyon
    void PlayErrorSound()
    {
        if (audioSource != null && errorSound != null)
        {
            // Arka arkaya hızlıca kart çekilirse robotik tınlamasın diye çok hafif pitch dalgalanması
            audioSource.pitch = Random.Range(0.97f, 1.03f);
            audioSource.PlayOneShot(errorSound);
        }
    }

    void SwitchLight(bool passed)
    {
        if (lightCoroutine != null)
            StopCoroutine(lightCoroutine);

        lightCoroutine = StartCoroutine(SwitchLightEnumerator(passed));
    }

    void ReplaceText(string text)
    {
        if (replaceCoroutine != null)
            StopCoroutine(replaceCoroutine);

        replaceCoroutine = StartCoroutine(ReplaceTextEnumerator(text));
    }
    
    IEnumerator ReplaceTextEnumerator(string text)
    {
        deviceText.text = text;
        yield return new WaitForSeconds(textChangeTime);
        deviceText.text = "SWIPE CARD";
    }
    
    IEnumerator SwitchLightEnumerator(bool passed)
    {
        if(passed)
        {
            greenLightOn.SetActive(true);
            greenLightOff.SetActive(false);
        } else
        {
            redLightOn.SetActive(true);
            redLightOff.SetActive(false);
        }

        yield return new WaitForSeconds(textChangeTime);

        if (passed)
        {
            greenLightOn.SetActive(false);
            greenLightOff.SetActive(true);
        }
        else
        {
            redLightOn.SetActive(false);
            redLightOff.SetActive(true);
        }
    }

    protected override void OnStartPuzzle() { }
    protected override void OnEndPuzzle() { }
}