using System.Collections;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public class UserID : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Reference")]
    [Tooltip("Drag the TextMeshProUGUI element here that will show the Player ID")]
    public TextMeshProUGUI userIdText;

    // Cache the ID so we can easily copy and restore it
    private string currentPlayerId = "";
    private bool isCopied = false;

    async void Start()
    {
        // 1. Make sure Unity Services are initialized
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        // 2. Check if the player is already signed in
        if (AuthenticationService.Instance.IsSignedIn)
        {
            UpdateIdText();
        }
        else
        {
            // 3. If not signed in yet, wait for the SignedIn event to fire
            AuthenticationService.Instance.SignedIn += UpdateIdText;
        }
    }

    private void UpdateIdText()
    {
        if (userIdText != null)
        {
            currentPlayerId = AuthenticationService.Instance.PlayerId;
            userIdText.text = "Player ID: " + currentPlayerId + " (Click To Copy)";
        }
    }

    // --- NEW: CLick & Copy Logic ---

    // This method fires automatically when the user clicks the object this script is attached to
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(currentPlayerId) && !isCopied)
        {
            // 1. Copy the raw ID to the device's clipboard
            GUIUtility.systemCopyBuffer = currentPlayerId;

            // 2. Show visual feedback to the player
            StartCoroutine(ShowCopiedFeedback());
        }
    }

    private IEnumerator ShowCopiedFeedback()
    {
        isCopied = true;
        userIdText.text = "<color=green>ID Copied to Clipboard!</color>";

        // Wait for 1.5 seconds
        yield return new WaitForSeconds(1.5f);

        // Revert back to the original ID text
        if (userIdText != null)
        {
            userIdText.text = "Player ID: " + currentPlayerId + " (Click To Copy)";
        }
        isCopied = false;
    }

    private void OnDestroy()
    {
        // Clean up the event listener to prevent memory leaks when changing scenes
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            AuthenticationService.Instance.SignedIn -= UpdateIdText;
        }
    }
}