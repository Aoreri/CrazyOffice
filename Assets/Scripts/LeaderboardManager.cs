using TMPro;
using UnityEngine;
using System.Threading.Tasks; // Required for Async/Await
using Unity.Services.Core; // Unity Services
using Unity.Services.Authentication; // Authentication
using Unity.Services.Leaderboards; // Leaderboards

public class LeaderboardManager : MonoBehaviour
{
    [Tooltip("Drag your UI template prefab/object here")]
    public GameObject userObject;

    [Header("Unity Leaderboard Settings")]
    [Tooltip("The Leaderboard ID you created on the Unity Dashboard. Must match ConsentManager!")]
    public string leaderboardId = "leaderboard";

    async void Start()
    {
        // Hide the original template object so it doesn't show up as an empty row
        userObject.SetActive(false);

        // Populate the leaderboard asynchronously
        await PopulateLeaderboardAsync();
    }

    async Task PopulateLeaderboardAsync()
    {
        try
        {
            // 1. Ensure Unity Services are initialized
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }

            // 2. Ensure the player is signed in anonymously
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            // 3. Fetch the top 10 scores from the Unity Leaderboard
            var options = new GetScoresOptions { Limit = 10 };
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, options);

            // 4. Loop through the results and build the UI
            int rank = 1;
            foreach (var entry in scoresResponse.Results)
            {
                // --- NEW: Filter out scores that are 0 (or negative) ---
                if (entry.Score <= 0)
                {
                    continue; // Skip this entry and move to the next one
                }

                // Get the name saved via UpdatePlayerNameAsync in ConsentManager.
                // Fallback to "Anonymous_Player" if it's null or empty.
                string playerName = string.IsNullOrEmpty(entry.PlayerName) ? "Anonymous_Player" : entry.PlayerName;

                // entry.Score comes as a double. Cast to float for our FormatTime method.
                string formattedTime = FormatTime((float)entry.Score);

                // Add to UI
                AddUser(playerName, formattedTime, rank);

                // Only increase the rank number if a valid score was actually added to the UI
                rank++;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to fetch leaderboard data: " + e.Message);
        }
    }

    // Helper method to convert seconds (float) into a "00:00" string
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
        int seconds = Mathf.FloorToInt(timeInSeconds - minutes * 60);

        // This formats the integers to always have two digits (e.g., 05:09)
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void AddUser(string name, string time, int number)
    {
        // Instantiate a new row
        GameObject newUser = Instantiate(userObject, userObject.transform.parent);

        // Loop through the children to find the correct Text elements
        foreach (Transform child in newUser.transform)
        {
            if (child.name == "Number")
            {
                child.GetComponent<TextMeshProUGUI>().text = $"{number}.";
            }
            else if (child.name == "Name")
            {
                child.GetComponent<TextMeshProUGUI>().text = name;
            }
            else if (child.name == "Time")
            {
                child.GetComponent<TextMeshProUGUI>().text = time;
            }
        }

        // Turn the new row on
        newUser.SetActive(true);
    }
}