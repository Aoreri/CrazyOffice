using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void OnPlay()
    {
        Debug.Log("Play clicked");
    }

    public void OnSettings()
    {
        Debug.Log("Settings clicked");
    }

    public void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
