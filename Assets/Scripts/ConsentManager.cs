using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ConsentManager : MonoBehaviour
{
    public static ConsentManager Instance { get; private set; }

    [Header("Panel Settings")]
    public GameObject consentPanel;
    public GameObject agreementGroup;
    public GameObject inputGroup;

    public TextMeshProUGUI userID;

    [Header("Input Fields")]
    public TMP_InputField courseId;
    public TMP_InputField name;

    [Header("Score Data")]
    public float totalTimePlayed;
    public float finishTime;

    [Header("Unity Leaderboard Settings")]
    public string leaderboardId = "leaderboard";

    [System.Serializable]
    private class LeaderboardMetadata
    {
        public string courseId;
        public float finishTime;
        public float totalTimePlayed;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            consentPanel.SetActive(false);

            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    // Oyuncu başta onay butonuna bastığında çalışır (Bunu UI'daki Approve butonuna bağlamaya devam edebilirsin)
    public async void OnApproveClicked()
    {
        await InitializeUnityServicesAsync();

        if (agreementGroup != null) agreementGroup.SetActive(false);
        if (inputGroup != null) inputGroup.SetActive(true);
    }

    private async Task InitializeUnityServicesAsync()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                userID.text = "Player ID: " + AuthenticationService.Instance.PlayerId + " (Click To Copy)";
               
                Debug.Log("Signed into Unity Services Anonymously.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error initializing Unity Services: " + e.Message);
        }
    }




    // Oyun bittiğinde DataManager tarafından OTOMATİK çağırılacak metod
    public async void AutoSubmitScore()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await InitializeUnityServicesAsync();

            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                Debug.LogError("Critical Error: Could not connect to Unity Services.");
                return;
            }
        }

        // UI'daki InputField'lardan verileri çek (Eğer boş bırakıldıysa varsayılan değerleri ata)
        string currentCourseId = string.IsNullOrWhiteSpace(courseId.text) ? "DEFAULT" : courseId.text.ToUpper();
        string rawName = string.IsNullOrWhiteSpace(name.text) ? "Anonymous_Player" : name.text;
        string finalName = rawName.Trim().Replace(" ", "_");

        try
        {
            // Kullanıcı ismini Unity Cloud üzerinde güncelle
            await AuthenticationService.Instance.UpdatePlayerNameAsync(finalName);

            LeaderboardMetadata metadataObj = new LeaderboardMetadata
            {
                courseId = currentCourseId,
                finishTime = finishTime,
                totalTimePlayed = totalTimePlayed,
            };

            var options = new AddPlayerScoreOptions { Metadata = metadataObj };
            double mainScore = (double)finishTime;

            var response = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, mainScore, options);

            Debug.Log($"Skor otomatik olarak gönderildi! Skor: {mainScore}, İsim: {finalName}, Course: {currentCourseId}");

            // İşlem bitince paneli gizle
            if (consentPanel != null) consentPanel.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError("An error occurred while submitting the score: " + e.Message);
        }
    }
}