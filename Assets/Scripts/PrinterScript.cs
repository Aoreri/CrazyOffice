using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrinterScript : Puzzle
{
    [Header("Animation Settings")]
    [SerializeField] private float fillAnimationDuration = 0.25f; // How long the initial fill takes

    [Header("Audio Settings")]
    // YENİ EKLENDİ: Ses bileşenleri
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Mürekkep doldurulurken çalacak gluk gluk sesi (Loop)")]
    [SerializeField] private AudioClip fillingLoopSound;
    [Tooltip("Tüm tanklar dolduğunda çalacak olan yazıcı/çıktı sesi")]
    [SerializeField] private AudioClip successSound;

    private TankFill[] allTanks;
    private bool isFinished = false;
    private bool isSpawning = false; // Blocks completion checks during the start animation
    
    // YENİ EKLENDİ: Sinyal takibi için bir önceki karenin doluluk oranı
    private float _lastTotalFill = 0f; 

    protected override void OnEndPuzzle() { }
    protected override void OnStartPuzzle() { }

    void Start()
    {
        // 1. Find all TankFill objects active in the scene
        allTanks = FindObjectsByType<TankFill>(FindObjectsInactive.Exclude);
        if (allTanks == null || allTanks.Length == 0) return;

        // 2. Decide randomly if 1 or 2 tanks will be empty
        int numEmpty = Random.Range(1, 3);
        numEmpty = Mathf.Min(numEmpty, allTanks.Length);

        // 3. Create a list of available tank indices to pick from
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < allTanks.Length; i++)
        {
            availableIndices.Add(i);
        }

        // 4. Randomly select which indices will represent the empty tanks
        List<int> emptyIndices = new List<int>();
        for (int i = 0; i < numEmpty; i++)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            emptyIndices.Add(availableIndices[randomIndex]);
            availableIndices.RemoveAt(randomIndex);
        }

        // 5. Start the animated fill process
        StartCoroutine(AnimateInitialFill(emptyIndices));
    }

    private IEnumerator AnimateInitialFill(List<int> emptyIndices)
    {
        isSpawning = true;
        float elapsedTime = 0f;

        // Array to hold the target fill goals for each tank
        float[] targetFills = new float[allTanks.Length];

        // First, set EVERY tank to minimum (empty) visually
        for (int i = 0; i < allTanks.Length; i++)
        {
            TankFill tank = allTanks[i];
            targetFills[i] = emptyIndices.Contains(i) ? tank.minFill : tank.maxFill;
            tank.fillAmount = tank.minFill;
            tank.AddFill(0f);
        }

        // Animate the fill over time
        while (elapsedTime < fillAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fillAnimationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < allTanks.Length; i++)
            {
                if (!emptyIndices.Contains(i))
                {
                    TankFill tank = allTanks[i];
                    tank.fillAmount = Mathf.Lerp(tank.minFill, targetFills[i], t);
                    tank.AddFill(0f); 
                }
            }
            yield return null;
        }

        // Ensure all tanks are exactly at their final target values just in case
        float initialTotalFill = 0f;
        for (int i = 0; i < allTanks.Length; i++)
        {
            TankFill tank = allTanks[i];
            tank.fillAmount = targetFills[i];
            tank.AddFill(0f);
            
            // YENİ EKLENDİ: Başlangıçtaki toplam doluluğu kaydediyoruz
            initialTotalFill += tank.fillAmount;
        }

        _lastTotalFill = initialTotalFill;
        isSpawning = false;
    }

    void Update()
    {
        // Do not check for win condition if we are still playing the opening animation
        if (isFinished || isSpawning || allTanks == null || allTanks.Length == 0) return;

        bool allTanksFull = true;
        float currentTotalFill = 0f;

        // Check if every tank has reached its maximum fill limit
        foreach (TankFill tank in allTanks)
        {
            currentTotalFill += tank.fillAmount; // Toplam doluluğu hesapla
            
            if (tank.fillAmount < tank.maxFill - 0.001f)
            {
                allTanksFull = false;
            }
        }

        // --- YENİ EKLENDİ: SES TETİKLEME MANTIĞI ---
        // Eğer şu anki doluluk, bir önceki kareden büyükse oyuncu fareye basıp dolduruyordur
        if (currentTotalFill > _lastTotalFill + 0.0001f)
        {
            if (audioSource != null && fillingLoopSound != null && !audioSource.isPlaying)
            {
                audioSource.clip = fillingLoopSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            // Eğer doluluk artmıyorsa (oyuncu fareyi bıraktıysa) sesi anında duraklat
            if (audioSource != null && audioSource.isPlaying && audioSource.clip == fillingLoopSound)
            {
                audioSource.Pause(); // Stop yerine Pause yapıyoruz ki tekrar bastığında ses baştan değil kaldığı yerden organik aksın
            }
        }

        // Bir sonraki karede karşılaştırmak için güncel değeri hafızaya al
        _lastTotalFill = currentTotalFill;
        // -------------------------------------------

        // If all are full, destroy the target object
        if (allTanksFull)
        {
            isFinished = true;
            
            // YENİ EKLENDİ: Tüm tanklar dolduysa loop sesini tamamen kes ve çıktı sesini çal
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.loop = false;
                
                if (successSound != null)
                {
                    audioSource.PlayOneShot(successSound);
                }
            }

            EndPuzzle();
        }
    }
}