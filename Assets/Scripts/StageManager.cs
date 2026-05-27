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

    public Transform spawnPosition;

    [Header("Tüm Senaryolar ve Use Case Görevleri")]
    public ScenarioData[] allScenarios = new ScenarioData[3];

    public GameObject markerCanvas;

    [HideInInspector] public int selectedScenarioIndex = 0;

    // Takip deðiþkenleri
    private int currentUseCaseIndex = 0;
    private int currentQuestIndex = 0;

    [Header("Cinematic Sequence Settings")]
    [Tooltip("Drag and drop empty GameObjects here in the order you want the player to move.")]
    public Transform[] cinematicSteps;
    [Tooltip("How long it takes to move from one step to the next (in seconds).")]
    public float timePerStep = 0.25f;
    [Tooltip("How fast the player rotates to face the next step.")]
    public float turnSpeed = 10f; // New variable for rotation speed

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (markerCanvas != null) markerCanvas.SetActive(false);

        player.transform.position = new Vector3(spawnPosition.position.x, player.transform.position.y, spawnPosition.position.z);
        player.GetComponent<CapsuleCollider>().enabled = false;
        player.GetComponent<PlayerMovement>().disablePlayerMovement();
        currCamera.GetComponent<CameraFollow>().enabled = false;

        StartCoroutine(startAnimation());
    }

    private IEnumerator startAnimation()
    {
        // 1. Initial door opening sequence
        yield return new WaitForSeconds(0.8f);
        door.GetComponent<DoorScript>().ToggleDoor(true);
        yield return new WaitForSeconds(0.7f);

        // 2. Enable camera and grab the Animator
        currCamera.GetComponent<CameraFollow>().enabled = true;
        Animator playerAnim = player.GetComponentInChildren<Animator>();

        // 3. Loop through each step for movement and rotation
        foreach (Transform step in cinematicSteps)
        {
            Vector3 startPos = player.transform.position;
            Vector3 endPos = step.position;

            // Ensure we don't rotate up/down if the steps are at different heights
            endPos.y = startPos.y;

            // Calculate direction for Animator and Rotation
            Vector3 moveDirection = (endPos - startPos).normalized;

            Quaternion targetRotation = player.transform.rotation;
            if (moveDirection != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(moveDirection);
            }

            // Update Animator parameters
            playerAnim.SetBool("isWalking", true);
         

            // Move and rotate smoothly over 'timePerStep'
            float elapsedTime = 0f;
            while (elapsedTime < timePerStep)
            {
                float completionPercentage = elapsedTime / timePerStep;

                // Move position
                player.transform.position = Vector3.Lerp(startPos, endPos, completionPercentage);

                // Rotate facing direction
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Snap to exact position and rotation at the end of the current step
            player.transform.position = endPos;
            player.transform.rotation = targetRotation;
        }

        // 4. Movement finished: Stop the animation
        playerAnim.SetBool("isWalking", false);
        

        // Reset rotation to default (change Quaternion.identity to Quaternion.Euler(x, y, z) if your default is different)
        player.transform.rotation = Quaternion.identity;

        // 6. Enable interactivity and UI
        player.GetComponent<CapsuleCollider>().enabled = true;

        yield return new WaitForSeconds(0.5f);

        if (markerCanvas != null)
            markerCanvas.SetActive(true);
    }

    public void StartScenarioQuests()
    {
        
        if (markerCanvas != null)
        {
            UIFader uf = markerCanvas.GetComponent<UIFader>();
            if (uf != null)
                uf.FadeOut();
        }
        player.GetComponent<PlayerMovement>().enablePlayerMovement();

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
