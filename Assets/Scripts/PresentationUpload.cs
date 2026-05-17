using UnityEngine;
using System.Collections; 

public class PresentationUpload : MonoBehaviour
{
    [Header("USB Settings")]
    public Transform usbObject;
    public Transform usbEnd;      
    public float moveTime = 1.0f;  

    [Header("Folder Settings")]
    public GameObject folderObject;

    private bool _isUSBMoving = false;

    private void Start()
    {
        // Ensure folder starts hidden
        if (folderObject != null)
        {
            folderObject.SetActive(false);
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
            Debug.Log("USB plugged in! Folder is now active.");
        }
    }
}