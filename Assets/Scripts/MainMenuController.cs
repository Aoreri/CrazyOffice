using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private int currentPage = 0;

    public GameObject[] pages;
    public GameObject backButton;

    public void OnPlay()
    {
        SceneManager.LoadScene("MapScene");
        Debug.Log("Play clicked");
    }



    public void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ChangeScene(int id)
    {
        pages[currentPage].SetActive(false);
        pages[id].SetActive(true);

        if (id != 0)
            backButton.SetActive(true);
        else
            backButton.SetActive(false);

        currentPage = id;
    }

}
