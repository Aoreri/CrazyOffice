using UnityEngine;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement; // Required for loading scenes

// Data classes remain the same
[System.Serializable]
public class StudentData
{
    public string studentNumber;
    public string fullName;
    public float totalTimePlayed;
    public float finishTime;
}

[System.Serializable]
public class StudentDatabase
{
    public List<StudentData> students = new List<StudentData>();
}

public class DataManager : MonoBehaviour
{
    // --- SINGLETON SETUP ---
    // This allows any script in any scene to access the manager easily
    public static DataManager Instance;

    [Header("Scene Settings")]
    [Tooltip("Type the exact name of your Game Scene here")]
    public string gameSceneName = "GameScene";

    [Header("UI References")]
    public GameObject loginPanel;
    public TMP_InputField studentNumberInput;

    public GameObject registerPanel;
    public TMP_InputField fullNameInput;

    private string saveFilePath;
    public StudentDatabase database;
    private StudentData currentStudent;

    // Time tracking
    private bool isGameRunning = false;
    private float sessionStartTime;

    void Awake()
    {
        // SINGLETON LOGIC: Ensure only ONE of these ever exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicates if we return to the main menu
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Make this object survive scene loads

        saveFilePath = Application.persistentDataPath + "/studentData.json";
        LoadDatabase();

        // Only set up UI if we are actually in the Main Menu scene
        if (loginPanel != null && registerPanel != null)
        {
            loginPanel.SetActive(true);
            registerPanel.SetActive(false);
        }
    }

    private void LoadDatabase()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            database = JsonUtility.FromJson<StudentDatabase>(json);
        }
        else
        {
            database = new StudentDatabase();
        }
    }

    private void SaveDatabase()
    {
        string json = JsonUtility.ToJson(database, true);
        File.WriteAllText(saveFilePath, json);
    }

    // --- BUTTON METHODS ---

    public void OnSubmitStudentNumber()
    {
        string inputNumber = studentNumberInput.text.Trim();

        if (string.IsNullOrEmpty(inputNumber))
        {
            Debug.LogWarning("Please enter a student number!");
            return;
        }

        currentStudent = database.students.Find(s => s.studentNumber == inputNumber);

        if (currentStudent != null)
        {
            Debug.Log($"Welcome back, {currentStudent.fullName}!");
            StartGame();
        }
        else
        {
            loginPanel.SetActive(false);
            registerPanel.SetActive(true);
        }
    }

    public void OnSubmitFullName()
    {
        string inputName = fullNameInput.text.Trim();

        if (string.IsNullOrEmpty(inputName))
        {
            Debug.LogWarning("Please enter a full name!");
            return;
        }

        currentStudent = new StudentData
        {
            studentNumber = studentNumberInput.text.Trim(),
            fullName = inputName,
            totalTimePlayed = 0f,
            finishTime = 0f,
        };

        database.students.Add(currentStudent);
        SaveDatabase();

        Debug.Log($"New student registered: {currentStudent.fullName}");
        StartGame();
    }

    // --- GAME FLOW LOGIC ---

    private void StartGame()
    {
        // Start tracking time right before we load the scene
        sessionStartTime = Time.time;
        isGameRunning = true;

        Debug.Log("Loading Game Scene and tracking time...");

        // Load the game scene!
        SceneManager.LoadScene(gameSceneName);
    }

    // You can now call this from anywhere using: StudentDataManager.Instance.EndGame();
    public void EndGame(float time)
    {
        if (isGameRunning && currentStudent != null)
        {
            isGameRunning = false;

            float timePlayedThisSession = Time.time - sessionStartTime;
            currentStudent.totalTimePlayed += timePlayedThisSession;

            if(time != 0 && (time < currentStudent.finishTime || currentStudent.finishTime == 0))
            {
                currentStudent.finishTime = time;
            }

            SaveDatabase();

            Debug.Log($"Game Ended. Session time: {timePlayedThisSession}s. Total time: {currentStudent.totalTimePlayed}s");
        }
    }

    private void OnApplicationQuit()
    {
        EndGame(0);
    }
}