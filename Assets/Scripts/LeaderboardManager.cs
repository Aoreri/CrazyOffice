using System.Linq;
using TMPro;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public GameObject userObject;


    void Start()
    {
        for(int i = 1; i <= 10; i++)
        {
            AddUser("none", "00:00", i);
        }
    }

    void AddUser(string name, string time, int number)
    {
        GameObject newUser = Instantiate(userObject, userObject.transform.parent);

        foreach (Transform child in newUser.transform)
        {
            if(child.name == "Number")
            {
                child.GetComponent<TextMeshProUGUI>().text = $"{number}.";
            } else if(child.name == "Name")
            {
                child.GetComponent<TextMeshProUGUI>().text = name;
            }
            else if (child.name == "Time")
            {
                child.GetComponent<TextMeshProUGUI>().text = time;
            }
        }

            newUser.SetActive(true);
    }
}
