using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    // --- SINGLETON SETUP ---
    public static DataManager Instance;

    [Header("Scene Settings")]
    [Tooltip("Type the exact name of your Game Scene here")]
    public string gameSceneName = "GameScene";

    // --- SESSION DATA (Sadece Hafızada Tutulur) ---
    private float totalTimePlayed = 0f;
    private float finishTime = 0f;

    // Time tracking
    private bool isGameRunning = false;
    private float sessionStartTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

  

    // --- GAME FLOW LOGIC ---

    public void StartGame()
    {
       
        sessionStartTime = Time.time;
        isGameRunning = true;

        Debug.Log("Loading Game Scene and tracking time...");

        SceneManager.LoadScene(gameSceneName);
    }

    public void EndGame(float time)
    {
        if (isGameRunning)
        {
            isGameRunning = false;

            float timePlayedThisSession = Time.time - sessionStartTime;
            totalTimePlayed += timePlayedThisSession;

            if (time > 0 && (time < finishTime || finishTime == 0f))
            {
                finishTime = time;
            }

          
            if (ConsentManager.Instance != null)
            {
                ConsentManager.Instance.totalTimePlayed = totalTimePlayed;
                ConsentManager.Instance.finishTime = finishTime;

                ConsentManager.Instance.AutoSubmitScore();
            }

            Debug.Log($"Game Ended. Session time: {timePlayedThisSession}s.");
        }
    }

    private void OnApplicationQuit()
    {
        EndGame(0);
    }
}