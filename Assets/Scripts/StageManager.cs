using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// --- EKLENEN YENÝ SINIFLAR ---
[System.Serializable]
public class UseCaseTasks
{
    public string useCaseName; // Sadece senin okuman için (Örn: "Scan Label")
    public Quest[] quests;     // Bu Use Case'in içereceði özel görevler
}

[System.Serializable]
public class ScenarioData
{
    public string scenarioName; // Sadece senin okuman için (Örn: "Food Delivery")
    public UseCaseTasks[] useCases = new UseCaseTasks[3]; // Her senaryonun 3 adet Use Case'i
}
// -----------------------------

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    public GameObject player;
    public Camera currCamera;
    public GameObject door;

    public Transform startPosition;
    public Transform spawnPosition;

    [Header("Tüm Senaryolar ve Use Case Görevleri")]
    public ScenarioData[] allScenarios = new ScenarioData[3];

    public GameObject markerCanvas;

    [HideInInspector] public int selectedScenarioIndex = 0;

    // Takip deðiþkenleri
    private int currentUseCaseIndex = 0;
    private int currentQuestIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (markerCanvas != null) markerCanvas.SetActive(false);

        player.transform.position = new Vector3(spawnPosition.position.x, player.transform.position.y, spawnPosition.position.z);
        player.GetComponent<CapsuleCollider>().enabled = false;
        player.GetComponent<PlayerMovement>().disableMovement = true;
        currCamera.GetComponent<CameraFollow>().enabled = false;

        StartCoroutine(startAnimation());
    }

    private IEnumerator startAnimation()
    {
        yield return new WaitForSeconds(0.8f);
        door.GetComponent<DoorScript>().ToggleDoor(true);
        yield return new WaitForSeconds(0.7f);

        player.GetComponent<PlayerMovement>().moveInput = new Vector2(0, -1);
        yield return new WaitForSeconds(0.42f);
        currCamera.GetComponent<CameraFollow>().enabled = true;
        yield return new WaitForSeconds(1f);
        player.GetComponent<PlayerMovement>().moveInput = new Vector2(0, 0);
        yield return new WaitForSeconds(0.5f);

        player.transform.position = new Vector3(startPosition.position.x, player.transform.position.y, startPosition.position.z);

        player.GetComponent<CapsuleCollider>().enabled = true;

        yield return new WaitForSeconds(0.5f);

        if (markerCanvas != null)
            markerCanvas.SetActive(true);
    }

    public void StartScenarioQuests()
    {
        if (markerCanvas != null) markerCanvas.SetActive(false);
        player.GetComponent<PlayerMovement>().disableMovement = false;

        // Sayaçlarý sýfýrla
        currentUseCaseIndex = 0;
        currentQuestIndex = 0;

        StartNextQuest();
    }

    public void StartNextQuest()
    {
        // Güvenlik kontrolü
        if (selectedScenarioIndex >= allScenarios.Length) return;

        ScenarioData currentScenario = allScenarios[selectedScenarioIndex];

        // 1. Eðer bu senaryodaki tüm Use Case'ler bittiyse OYUN BÝTER
        if (currentUseCaseIndex >= currentScenario.useCases.Length)
        {
            if (DataManager.Instance == null)
            {
                Debug.Log("No data found!");
                return;
            }

            DataManager.Instance.EndGame(TimeManager.Instance.timeElapsed);
            SceneManager.LoadScene("MainMenu");
            return;
        }

        UseCaseTasks currentUseCase = currentScenario.useCases[currentUseCaseIndex];

        // 2. Eðer bulunduðumuz Use Case'in görevleri devam ediyorsa sýradakini ver
        if (currentQuestIndex < currentUseCase.quests.Length)
        {
            Quest nextQuest = currentUseCase.quests[currentQuestIndex];
            currentQuestIndex++; // Sýradakine hazýrlan

            // Eðer inspector'da bir kutuyu boþ unuttuysan hata vermesin diye kontrol
            if (nextQuest != null)
            {
                QuestManager.Instance.StartQuest(nextQuest);
            }
            else
            {
                StartNextQuest(); // Boþsa sýradakine atla
            }
        }
        // 3. Eðer bulunduðumuz Use Case'in görevleri bittiyse DÝÐER Use Case'e geç
        else
        {
            currentUseCaseIndex++;
            currentQuestIndex = 0;
            StartNextQuest(); // Döngüyü tekrar tetikle
        }
    }
}