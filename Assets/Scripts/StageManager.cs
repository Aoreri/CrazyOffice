using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    public GameObject player;
    public Camera currCamera;
    public GameObject door;

    public Transform startPosition;
    public Transform spawnPosition;

    [Header("Senaryo 1 Görevleri (Örn: Food Delivery)")]
    public Quest[] scenario1Quests;

    [Header("Senaryo 2 Görevleri")]
    public Quest[] scenario2Quests;

    [Header("Senaryo 3 Görevleri")]
    public Quest[] scenario3Quests;

    public GameObject markerCanvas;

    [HideInInspector] public int selectedScenarioIndex = 0;
    private Quest[] currentScenarioQuests;
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
        player.GetComponent<PlayerMovement>().disableMovement = false;
        player.GetComponent<CapsuleCollider>().enabled = true;

        TimeManager.Instance.StartTimer();
        yield return new WaitForSeconds(0.5f);

        if (markerCanvas != null)
            markerCanvas.SetActive(true);
    }

    
    public void StartScenarioQuests()
    {
        if (markerCanvas != null) markerCanvas.SetActive(false);

        
        if (selectedScenarioIndex == 0) currentScenarioQuests = scenario1Quests;
        else if (selectedScenarioIndex == 1) currentScenarioQuests = scenario2Quests;
        else if (selectedScenarioIndex == 2) currentScenarioQuests = scenario3Quests;
        else currentScenarioQuests = scenario1Quests;

        currentQuestIndex = 0; 
        StartNextQuest();
    }

    
    public void StartNextQuest()
    {
        
        if (currentScenarioQuests != null && currentQuestIndex < currentScenarioQuests.Length)
        {
            Quest nextQuest = currentScenarioQuests[currentQuestIndex];
            currentQuestIndex++;

            QuestManager.Instance.StartQuest(nextQuest);
        }
        else
        {
            Debug.Log("BU SENARYONUN TÜM GÖREVLERÝ (USE CASE'LERÝ) BÝTTÝ!");
        }
    }
}