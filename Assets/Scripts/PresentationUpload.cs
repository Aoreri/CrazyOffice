using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Required for Image

public class PresentationUpload : Puzzle
{
    [Header("USB Settings")]
    public Transform usbObject;
    public Transform usbEnd;
    public float moveTime = 1.0f;

    [Header("Folder Settings")]
    public GameObject folderObject;

    [Header("Upload Settings")]
    public Image uploadFillImage; // Assign your UI Image here in the inspector

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
        }

        // Trigger the end of the puzzle once the upload finishes
        // (Assuming your base 'Puzzle' class handles the logic via EndPuzzle())
        EndPuzzle();
    }

    protected override void OnStartPuzzle()
    {
    
    }

    protected override void OnEndPuzzle()
    {
        
    }
}
