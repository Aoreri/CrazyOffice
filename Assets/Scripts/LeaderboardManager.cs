using System.Linq;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    [Tooltip("Drag your UI template prefab/object here")]
    public GameObject userObject;

    void Start()
    {
        // Hide the original template object so it doesn't show up as an empty row
        userObject.SetActive(false);

        PopulateLeaderboard();
    }

    void PopulateLeaderboard()
    {
        // 1. Ensure our DataManager exists in the scene
        if (DataManager.Instance == null)
        {
            Debug.LogError("StudentDataManager not found! Make sure you started from the Main Menu.");
            return;
        }

        // 2. Get the list of all students
        List<StudentData> allStudents = DataManager.Instance.database.students;

        // 3. Use LINQ to sort the top 10 students
        var topStudents = allStudents
            .Where(s => s.finishTime > 0f) // Ignore students who haven't finished (time is 0)
            .OrderBy(s => s.finishTime)    // Sort ascending (lowest time = fastest = 1st place)
            .Take(10)                      // Take only the top 10
            .ToList();

        // 4. Loop exactly 10 times to build the leaderboard UI
        for (int i = 0; i < 10; i++)
        {
            if (i < topStudents.Count)
            {
                // If a student exists for this rank, format their time and display them
                string formattedTime = FormatTime(topStudents[i].finishTime);
                AddUser(topStudents[i].fullName, formattedTime, i + 1);
            }
            else
            {
                // If we have fewer than 10 students, fill the remaining slots with dummies
                //AddUser("none", "00:00", i + 1);
            }
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