using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public Transform player;

    [Header("Current Progress")]
    public Quest activeQuest;
    public int currentStepIndex = 0;

    // Stores the randomly selected GameObject if the current step requires it
    private GameObject targetNPCForCurrentStep;
    private string targetPuzzleForCurrentStep;
    private GameObject targetItemForCurrentStep;

    private int stepCount;
    private int totalStepCount;

    [Header("UI")]
    public GameObject questUI;
    public Image progressBar;
    public TextMeshProUGUI counter;
    public TextMeshProUGUI questText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("Player not found! Please assign the Player tag or drag the Player into the script.");
        }
    }

    public void StartQuest(Quest newQuest)
    {
        Debug.Log("Quest Started! " + newQuest.transform.name);

        totalStepCount = 0;
        stepCount = 0;

        activeQuest = newQuest;

        questUI.SetActive(true);

        for (int i = 0; i < newQuest.steps.Count; i++)
        {
            if (newQuest.steps[i].shouldCount)
                totalStepCount++;
        }

        questText.text = newQuest.description;
        counter.text = $"0/{totalStepCount}";
        progressBar.fillAmount = 0;

        currentStepIndex = 0;
        InitializeStep();
    }

    private void InitializeStep()
    {
        // GÖREVÝN BÝTTÝÐÝ YER BURASI
        if (currentStepIndex >= activeQuest.steps.Count)
        {
            Debug.Log($"Quest {activeQuest.questName} Complete!");
            activeQuest = null;
            questUI.SetActive(false);

            // --- YENÝ EKLENEN KISIM: Görev bitince StageManager'a sýradaki görevi baþlatmasýný söyle ---
            if (StageManager.Instance != null)
            {
                StageManager.Instance.StartNextQuest();
            }

            return;
        }

        QuestStep currentStep = activeQuest.steps[currentStepIndex];
        Debug.Log("New Step: " + currentStep.stepDescription);

        if (currentStep.shouldCount)
        {
            questText.text = currentStep.stepDescription;
        }

        if (currentStep.objectiveType == QuestObjectiveType.ShowDialogue)
        {
            if (currentStep.dialogueArea.Length != 0 && currentStep.target != null)
            {
                int randomIndex = Random.Range(0, currentStep.dialogueArea.Length);
                string dialogue = currentStep.dialogueArea[randomIndex];

                DialogueManager.Instance.chatStart(currentStep.target, dialogue, () =>
                {
                    AdvanceQuest();
                    Debug.Log("Dialogue ended.");
                });

            }
            else Debug.Log("NO DIALOGUE FOUND!");
        }

        if (currentStep.objectiveType == QuestObjectiveType.TalkToNPC)
        {
            if (currentStep.possibleNPCs.Length > 0)
            {
                int randomIndex = Random.Range(0, currentStep.possibleNPCs.Length);
                targetNPCForCurrentStep = currentStep.possibleNPCs[randomIndex];
                Debug.Log("Assigned target NPC: " + targetNPCForCurrentStep.name);

                ChatManager.Instance.assignedNPC = targetNPCForCurrentStep;

                AddHighlightLayer(targetNPCForCurrentStep);
            }
            else Debug.Log("NO NPC FOUND!");
        }

        if (currentStep.objectiveType == QuestObjectiveType.SolvePuzzle)
        {
            if (currentStep.puzzleNames.Length > 0)
            {
                int randomIndex = Random.Range(0, currentStep.puzzleNames.Length);
                targetPuzzleForCurrentStep = currentStep.puzzleNames[randomIndex];
                Debug.Log("Assigned puzzle: " + targetPuzzleForCurrentStep);

                PuzzleManager.StartPuzzle(targetPuzzleForCurrentStep);

            }
            else Debug.Log("NO PUZZLE FOUND!");
        }

        if (currentStep.objectiveType == QuestObjectiveType.CollectItem)
        {
            if (currentStep.itemObjects.Length > 0)
            {
                int randomIndex = Random.Range(0, currentStep.itemObjects.Length);
                targetItemForCurrentStep = currentStep.itemObjects[randomIndex];
                Debug.Log("Assigned item: " + targetItemForCurrentStep);

                AddHighlightLayer(targetItemForCurrentStep);
            }
            else Debug.Log("NO ITEM FOUND!");
        }

        if (currentStep.objectiveType == QuestObjectiveType.ChangeDoorState)
        {
            if (currentStep.doors.Length > 0)
            {
                for (int i = 0; i < currentStep.doors.Length; i++)
                {
                    DoorScript ds = currentStep.doors[i].GetComponent<DoorScript>();
                    ds.openable = !ds.openable;
                    Debug.Log("Door State: " + ds.openable);
                }
            }
            else Debug.Log("NO DOOR FOUND!");
            AdvanceQuest();
        }
    }

    // Call these methods and pass 'this.gameObject' from your interactive scripts
    public void OnNPCTalkedTo(GameObject npc)
    {
        if (activeQuest == null) return;

        QuestStep currentStep = activeQuest.steps[currentStepIndex];
        if (currentStep.objectiveType == QuestObjectiveType.TalkToNPC && npc == targetNPCForCurrentStep)
        {
            RemoveHighlightLayer(targetNPCForCurrentStep);

            AdvanceQuest();
            targetNPCForCurrentStep = null;
        }
    }

    public void OnItemUsed(GameObject item)
    {
        if (activeQuest == null) return;

        QuestStep currentStep = activeQuest.steps[currentStepIndex];
        if (currentStep.objectiveType == QuestObjectiveType.CollectItem && item == targetItemForCurrentStep)
        {
            RemoveHighlightLayer(targetItemForCurrentStep);
            AdvanceQuest();
            targetItemForCurrentStep = null;
        }
    }

    public void OnPuzzleSolved(string puzzle)
    {
        if (activeQuest == null) return;

        QuestStep currentStep = activeQuest.steps[currentStepIndex];
        if (currentStep.objectiveType == QuestObjectiveType.SolvePuzzle && puzzle == targetPuzzleForCurrentStep)
        {
            AdvanceQuest();
            targetPuzzleForCurrentStep = null;
        }
    }

    private void AdvanceQuest()
    {
        if (activeQuest.steps[currentStepIndex].shouldCount)
        {
            stepCount++;

            counter.text = $"{stepCount}/{totalStepCount}";
            progressBar.fillAmount = ((float)stepCount / (float)totalStepCount);
        }

        Debug.Log("Step completed!");
        currentStepIndex++;
        InitializeStep();
    }

    public DirectionArrow navigationArrow;
    private void AddHighlightLayer(GameObject npc)
    {
        if (npc == null) return;

        uint layerBit = 1u << 8;

        Renderer[] renderers = npc.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.renderingLayerMask |= layerBit;
        }

        // TELL THE ARROW TO POINT AT THIS NPC
        if (navigationArrow != null)
        {
            navigationArrow.gameObject.SetActive(true);
            navigationArrow.SetTarget(npc.transform);
        }
    }

    private void RemoveHighlightLayer(GameObject npc)
    {
        if (npc == null) return;

        uint layerBit = 1u << 8;

        Renderer[] renderers = npc.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.renderingLayerMask &= ~layerBit;
        }

        // TELL THE ARROW TO STOP POINTING (Pass null to hide it)
        if (navigationArrow != null)
        {
            navigationArrow.SetTarget(null);
        }
    }

    void Update()
    {
        if (player != null && Input.GetKeyDown(KeyCode.F) && activeQuest != null && targetItemForCurrentStep != null)
        {
            float distance = Vector3.Distance(targetItemForCurrentStep.transform.position, player.position);

            if (distance <= 2f)
            {
                OnItemUsed(targetItemForCurrentStep);
            }
        }
    }
}