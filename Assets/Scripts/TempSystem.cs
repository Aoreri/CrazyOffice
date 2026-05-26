using UnityEngine;
using UnityEngine.UI;

public class TempSystem : Puzzle
{
    [Range(0f, 1f)]
    public float currentTemp = 1f;

    public float increaseSpeed = 0.05f;
    public float decreaseSpeed = 0.2f;

    [Header("Critical")]
    public float criticalTemp = 0.85f;
    public float damageInterval = 10f;

    [Header("Fan")]
    public Button fanButton;
    public Image fanButtonImage;
    public Sprite fanOffSprite;
    public Sprite fanOnSprite;

    [Header("Fan Speed")]
    public float clickPower = 0.3f;       
    public float maxFanSpeed = 1f;        
    public float fanDecayRate = 0.4f;   

    [Header("Arrow")]
    public RectTransform arrow;
    public RectTransform arrowEnd;

    [Header("Temperature Images")]
    public Image tempImage;
    public Sprite greenSprite;
    public Sprite orangeSprite;
    public Sprite redSprite;

    [Header("Audio Settings")]
    // YENİ EKLENDİ: Sesleri yönetecek iki ayrı AudioSource (Fan ve Alarm çakışmasın diye)
    public AudioSource fanAudioSource;
    public AudioSource alarmAudioSource;
    [Space(5)]
    public AudioClip fanLoopClip;         // Kesintisiz dönen fan uğultusu
    public AudioClip alarmBeepClip;       // "Beep beep" şeklinde döngüsel alarm
    public AudioClip successSound;         // Bulmaca bittiğinde çalacak mutlu ses

    [Header("Dynamic Audio Modifiers")]
    public float minFanPitch = 0.4f;       // Fan en yavaş dönerkenki kalın ses (Bass)
    public float maxFanPitch = 1.3f;       // Fan tam hızdayken çıkacağı tiz ses

    private Sprite currentSprite;
    private float crossfadeTimer = 1f;
    private Image fadingOutImage;

    [Header("Animation")]
    public float visualLerpSpeed = 5f;    

    private float fanSpeed = 0f;          
    private float damageTimer;
    private Vector2 arrowStartPos;

    private bool stopped = false;

    void Start()
    {
        arrowStartPos = arrow.anchoredPosition;
        fanButton.onClick.AddListener(OnFanClick);
        tempImage.fillAmount = Mathf.Lerp(0.3f, 0.8f, currentTemp);

        if (currentTemp >= 0.7f)
            currentSprite = redSprite;
        else if (currentTemp >= 0.4f)
            currentSprite = orangeSprite;
        else
            currentSprite = greenSprite;

        tempImage.sprite = currentSprite;
        tempImage.color = new Color(1f, 1f, 1f, 1f);
        crossfadeTimer = 1f;

        // YENİ EKLENDİ: Başlangıçta döngüsel sesleri hazır hale getiriyoruz
        SetupLoopingSounds();
    }

    private void SetupLoopingSounds()
    {
        if (fanAudioSource != null && fanLoopClip != null)
        {
            fanAudioSource.clip = fanLoopClip;
            fanAudioSource.loop = true;
            fanAudioSource.volume = 0f; // Başlangıçta fan duruyor, ses kapalı
            fanAudioSource.Play();
        }

        if (alarmAudioSource != null && alarmBeepClip != null)
        {
            alarmAudioSource.clip = alarmBeepClip;
            alarmAudioSource.loop = true;
            alarmAudioSource.volume = 0.6f; // Sabit net duyulabilir bir alarm volümü
        }
    }

    void Update()
    {
        if (stopped)
            return;

        fanSpeed -= fanDecayRate * Time.deltaTime;
        fanSpeed = Mathf.Clamp(fanSpeed, 0f, maxFanSpeed);

        if (fanSpeed > 0f)
        {
            currentTemp -= decreaseSpeed * fanSpeed * Time.deltaTime;
        }
        else
        {
            currentTemp += increaseSpeed * Time.deltaTime;
        }

        currentTemp = Mathf.Clamp01(currentTemp);

        // --- YENİ EKLENDİ: DİNAMİK FAN SESİ OTOMASYONU ---
        if (fanAudioSource != null && fanAudioSource.isPlaying)
        {
            float normalizedSpeed = fanSpeed / maxFanSpeed; // 0 ile 1 arası değer
            
            // Volüm ve Pitch tamamen fannın anlık dönüş hızına (devrine) bağlandı
            fanAudioSource.volume = normalizedSpeed; 
            fanAudioSource.pitch = Mathf.Lerp(minFanPitch, maxFanPitch, normalizedSpeed);
        }

        // --- YENİ EKLENDİ: ALARM (BEEP BEEP) KONTROLÜ ---
        if (alarmAudioSource != null && alarmBeepClip != null)
        {
            // Turuncu (0.4) veya Kırmızı (0.7) seviyedeyse alarm çalsın
            if (currentTemp >= 0.4f)
            {
                if (!alarmAudioSource.isPlaying)
                    alarmAudioSource.Play();
            }
            else
            {
                if (alarmAudioSource.isPlaying)
                    alarmAudioSource.Stop();
            }
        }

        // Critical temperature check
        if (currentTemp >= criticalTemp)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                TimeManager.Instance.ApplyPenalty(3);
                Debug.Log("Too hot! User loses points!");
            }
        }
        else
        {
            damageTimer = 0f;
        }

        UpdateVisuals();
        UpdateArrow();

        if(currentTemp == 0)
        {
            stopped = true;

            // YENİ EKLENDİ: Başarı durumunda tüm döngüleri kapatıp jingle patlatıyoruz
            if (fanAudioSource != null) fanAudioSource.Stop();
            if (alarmAudioSource != null) alarmAudioSource.Stop();

            if (fanAudioSource != null && successSound != null)
            {
                fanAudioSource.pitch = 1f; // Pitch'i normale çek
                fanAudioSource.PlayOneShot(successSound);
            }

            EndPuzzle();
        }
    }

    void OnFanClick()
    {
        fanSpeed = Mathf.Min(fanSpeed + clickPower, maxFanSpeed);
        fanButtonImage.sprite = fanSpeed > 0f ? fanOnSprite : fanOffSprite;
    }

    void UpdateArrow()
    {
        float t = fanSpeed / maxFanSpeed;
        arrow.anchoredPosition = arrowStartPos + Vector2.right * ((arrowEnd.anchoredPosition - arrowStartPos) * t);
        fanButtonImage.sprite = fanSpeed > 0f ? fanOnSprite : fanOffSprite;
    }

    void UpdateVisuals()
    {
        float targetFill = Mathf.Lerp(0.24f, 0.89f, currentTemp);
        float fill = Mathf.Lerp(tempImage.fillAmount, targetFill, Time.deltaTime * visualLerpSpeed);
        tempImage.fillAmount = fill;

        Sprite targetSprite;
        if (currentTemp >= 0.7f)
            targetSprite = redSprite;
        else if (currentTemp >= 0.4f)
            targetSprite = orangeSprite;
        else
            targetSprite = greenSprite;

        if (targetSprite != currentSprite)
        {
            if (fadingOutImage != null)
                Destroy(fadingOutImage.gameObject);

            GameObject copy = Instantiate(tempImage.gameObject, tempImage.transform.parent);
            fadingOutImage = copy.GetComponent<Image>();
            fadingOutImage.sprite = currentSprite;
            fadingOutImage.color = new Color(1f, 1f, 1f, 1f);
            copy.transform.SetSiblingIndex(tempImage.transform.GetSiblingIndex());

            currentSprite = targetSprite;
            tempImage.sprite = currentSprite;
            tempImage.color = new Color(1f, 1f, 1f, 0f);
            tempImage.transform.SetAsLastSibling();

            crossfadeTimer = 0f;
        }

        if (crossfadeTimer < 1f)
        {
            crossfadeTimer += Time.deltaTime * visualLerpSpeed;
            float alpha = Mathf.Clamp01(crossfadeTimer);

            tempImage.color = new Color(1f, 1f, 1f, alpha);

            if (fadingOutImage != null)
                fadingOutImage.color = new Color(1f, 1f, 1f, 1f - alpha);

            if (crossfadeTimer >= 1f)
            {
                if (fadingOutImage != null)
                {
                    Destroy(fadingOutImage.gameObject);
                    fadingOutImage = null;
                }
                tempImage.color = new Color(1f, 1f, 1f, 1f);
            }
        }

        if (fadingOutImage != null)
            fadingOutImage.fillAmount = fill;
    }

    protected override void OnStartPuzzle() { }
    protected override void OnEndPuzzle() { }
}