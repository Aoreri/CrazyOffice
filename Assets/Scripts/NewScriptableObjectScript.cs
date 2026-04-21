using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
  public void PlayGame()
  {
    SceneManager.LoadScene("SampleScene");
  }

  public void OpenOptions()
  {
    Debug.Log("Options açılacak");
  }

  public void QuitGame()
  {
    Debug.Log("Oyun kapatılıyor");
    Application.Quit();
  }
}
