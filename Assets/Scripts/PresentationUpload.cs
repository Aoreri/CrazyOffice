using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

public class PresentationUpload : Puzzle
{
    [Header("USB Settings")]
    public Transform usbObject;
    public Transform usbEnd;
    public float moveTime = 1.0f;

    [Header("Folder Settings")]
    public GameObject folderObject;

    [Header("Upload Settings")]
    public Image uploadFillImage; 

    [Header("Audio Settings")]
    // YENİ EKLENDİ: Ses yöneticisi ve dosyaları
    public AudioSource audioSource;
    [Tooltip("Yükleme barı dolarken çalacak, kendi içinde dönen (loop) işlem sesi")]
    public AudioClip loadingSound;
    [Tooltip("Yükleme %100 olduğunda çalacak olan başarı sesi")]
    public AudioClip successSound;

    private bool _isUSBMoving = false;

    public float minUploadTime = 1f;
    public float maxUploadTime = 4f;

    private void Start()
    {
        // Ensure folder starts hidden
        if (folderObject != null)
        {
            folderObject.SetActive(false);
        }

        // Ensure the fill image is reset and hidden at the start
        if (uploadFillImage != null)
        {
            uploadFillImage.fillAmount = 0f;
            uploadFillImage.transform.parent.gameObject.SetActive(false);
        }
    }

    public void StartUSBPlugIn()
    {
        if (!_isUSBMoving)
        {
            StartCoroutine(MoveUSBRoutine());
        }
    }

    private IEnumerator MoveUSBRoutine()
    {
        _isUSBMoving = true;

        Vector3 startPosition = usbObject.position;
        Vector3 targetPosition = usbEnd.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveTime)
        {
            usbObject.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        usbObject.position = targetPosition;

        if (folderObject != null)
        {
            folderObject.SetActive(true);
            Debug.Log("USB plugged in! Folder is now active and ready to be dragged.");
        }
    }

    // Called by the FolderDrag script once the folder is successfully dropped
    public void StartUploadSequence()
    {
        StartCoroutine(UploadRoutine());
    }

    private IEnumerator UploadRoutine()
    {
        if (uploadFillImage != null)
        {
            uploadFillImage.transform.parent.gameObject.SetActive(true);
            uploadFillImage.fillAmount = 0f;

            // YENİ EKLENDİ: Yükleme başladığında o dijital işlem sesini döngüye alarak başlatıyoruz
            if (audioSource != null && loadingSound != null)
            {
                audioSource.clip = loadingSound;
                audioSource.loop = true; // Bar dolana kadar aralıksız dönsün
                audioSource.Play();
            }

            // Pick a random time between 1 and 3 seconds
            float randomDuration = Random.Range(minUploadTime, maxUploadTime);
            float elapsedTime = 0f;

            // Fill the image over the random duration
            while (elapsedTime < randomDuration)
            {
                uploadFillImage.fillAmount = elapsedTime / randomDuration;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure it hits exactly 100% at the end
            uploadFillImage.fillAmount = 1f;
            
            // YENİ EKLENDİ: Bar tam dolduğunda yükleme sesini bıçak gibi kes ve başarı sesini patlat
            if (audioSource != null)
            {
                audioSource.Stop(); // Döngüyü durdur
                audioSource.loop = false; // Diğer seslerin döngüye girmemesi için kapat
                
                if (successSound != null)
                {
                    audioSource.PlayOneShot(successSound); // Mutlu son!
                }
            }
        }

        // Trigger the end of the puzzle once the upload finishes
        EndPuzzle();
    }

    protected override void OnStartPuzzle() { }

    protected override void OnEndPuzzle() { }
}