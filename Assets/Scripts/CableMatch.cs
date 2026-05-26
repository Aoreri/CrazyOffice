using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CableMatch : Puzzle
{
    public GameObject[] hubs = new GameObject[4];
    public GameObject[] connects = new GameObject[4];
    public Color[] colors = new Color[4];

    [Header("Audio Settings")]
    public AudioSource audioSource;
    [Tooltip("Doğru kablo bağlantıları için rastgele seçilecek elektrik çıtırtısı sample havuzu")]
    public AudioClip[] connectionSounds;       
    [Tooltip("Yanlış renk eşleştiğinde çalacak olan darbe/ark sesi")]
    public AudioClip electricImpactSound;      
    [Tooltip("Tüm kablolar doğru eşleşip mini game bittiğinde çalacak mutlu ses")]
    public AudioClip successNotificationSound; 

    private bool isFinished = false;
    private bool[] lastConnectedStates;        // Kabloların bir önceki frame'deki bağlantı durumunu hafızada tutar

    void Start()
    {
        AssignRandomUniqueColors(hubs);
        AssignRandomUniqueColors(connects);

        for (int i = 0; i < hubs.Length; i++)
        {
            hubs[i].GetComponent<WireDrag>().enabled = true;
        }

        PreConnectWires();

        // Harita ilk yüklendiğinde önceden bağlanan kabloların ses tetiklememesi için
        // hafıza dizimizi PreConnectWires işleminden SONRA, Start'ın en sonunda dolduruyoruz.
        lastConnectedStates = new bool[hubs.Length];
        for (int i = 0; i < hubs.Length; i++)
        {
            WireDrag wire = hubs[i].GetComponent<WireDrag>();
            if (wire != null)
            {
                lastConnectedStates[i] = wire.isConnected;
            }
        }
    }

    void Update()
    {
        if (isFinished) return;

        bool allMatched = true;

        for (int i = 0; i < hubs.Length; i++)
        {
            WireDrag wire = hubs[i].GetComponent<WireDrag>();
            if (wire != null)
            {
                // SİNYAL TAKİBİ: Eğer kablo geçen frame bağlı değildiyse ama BU frame bağlıysa (Yeni takıldıysa)
                if (wire.isConnected && !lastConnectedStates[i])
                {
                    // Doğru renkli kutuya mı takılmış kontrol et
                    if (wire.connectedTarget != null && wire.wireColor == wire.connectedTarget.targetColor)
                    {
                        PlayRandomConnectionSound();
                    }
                    else
                    {
                        // Yanlış bağlantı! Kıvılcım/Darbe sesini (Electric Impact) çal
                        if (audioSource != null && electricImpactSound != null)
                        {
                            audioSource.PlayOneShot(electricImpactSound);
                        }
                    }
                }

                // Bir sonraki frame için mevcut durumu hafızaya kaydet
                lastConnectedStates[i] = wire.isConnected;

                // CHANGED: Fail the check if it's NOT connected, OR if it's connected to the WRONG color
                if (!wire.isConnected || wire.connectedTarget == null || wire.wireColor != wire.connectedTarget.targetColor)
                {
                    allMatched = false;
                }
            }
        }

        if (allMatched && hubs.Length > 0)
        {
            isFinished = true;
            Debug.Log("All cables matched successfully!");
            
            // Bölüm bitti, o tatlı notification sesini patlat
            if (audioSource != null && successNotificationSound != null)
            {
                audioSource.pitch = 1f; // Önceki seslerden kalan pitch modülasyonu varsa sıfırla
                audioSource.PlayOneShot(successNotificationSound);
            }

            EndPuzzle();
        }
    }

    void PlayRandomConnectionSound()
    {
        if (audioSource != null && connectionSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, connectionSounds.Length);
            
            // İnce bir ses tasarımı dokunuşu: Her kıvılcım sesinin frekansı milimetrik değişsin ki robotik tınlamasın
            audioSource.pitch = Random.Range(0.93f, 1.07f);
            audioSource.PlayOneShot(connectionSounds[randomIndex]);
        }
    }

    void AssignRandomUniqueColors(GameObject[] objects)
    {
        List<Color> availableColors = new List<Color>(colors);

        for (int i = 0; i < objects.Length; i++)
        {
            int randomIndex = Random.Range(0, availableColors.Count);
            Color selectedColor = availableColors[randomIndex];

            if (objects[i] != null)
            {
                // CHANGED: If it's a dragging hub, update the wire pieces. Otherwise, just update the Image.
                WireDrag dragScript = objects[i].GetComponent<WireDrag>();
                if (dragScript != null)
                {
                    dragScript.UpdateWireColor(selectedColor);
                }
                else
                {
                    objects[i].GetComponent<Image>().color = selectedColor;
                }

                // Set the target logic color (applies to the connection points)
                WireTarget targetScript = objects[i].GetComponent<WireTarget>();
                if (targetScript != null)
                {
                    targetScript.targetColor = selectedColor;
                }
            }

            availableColors.RemoveAt(randomIndex);
        }
    }

    // NEW METHOD: Randomly pre-connects 1 or 2 wires to ANY target (right or wrong)
    void PreConnectWires()
    {
        int amountToConnect = Random.Range(1, 3);

        // Keep track of available hubs so we don't pick the same one twice
        List<int> availableHubs = new List<int>();
        for (int i = 0; i < hubs.Length; i++)
        {
            availableHubs.Add(i);
        }

        // Keep track of available targets so we don't plug two wires into the same spot
        List<int> availableTargets = new List<int>();
        for (int i = 0; i < connects.Length; i++)
        {
            availableTargets.Add(i);
        }

        for (int i = 0; i < amountToConnect; i++)
        {
            // Pick a random un-connected hub
            int rHubIndex = Random.Range(0, availableHubs.Count);
            int hubIndex = availableHubs[rHubIndex];
            availableHubs.RemoveAt(rHubIndex);

            // Pick a random open target (no color checking!)
            int rTargetIndex = Random.Range(0, availableTargets.Count);
            int targetIndex = availableTargets[rTargetIndex];
            availableTargets.RemoveAt(rTargetIndex);

            WireDrag wireScript = hubs[hubIndex].GetComponent<WireDrag>();
            WireTarget randomTarget = connects[targetIndex].GetComponent<WireTarget>();

            // Force the connection visually and logically
            if (wireScript != null && randomTarget != null)
            {
                wireScript.Connect(randomTarget);
            }
        }
    }

    protected override void OnStartPuzzle() { }
    protected override void OnEndPuzzle() { }
}