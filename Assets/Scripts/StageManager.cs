using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public float turnSpeed = 10f;

    [Header("Loading Screen")]
    [Tooltip("The full-screen black Image that covers the screen on load.")]
    public Image loadingPanel;
    [Tooltip("Extra frames to render before fading out (lets shaders warm up). 30–60 is ideal.")]
    public int warmUpFrames = 45;
    [Tooltip("Duration of the black screen fade-out in seconds.")]
    public float loadingFadeDuration = 0.8f;

    [Header("End Game UI Elements")]
    public GameObject pageBackground;
    public RectTransform umlBackground;
    public RectTransform timerRect;
    public float timerAnimDuration = 0.5f;
    public Vector3 timerTargetScale = new Vector3(1.5f, 1.5f, 1.5f);
    public Vector3 umlTargetScale = new Vector3(1.2f, 1.2f, 1.2f);

    private void Awake()
    {
        if (Instance == null) Instance = this;

        loadingPanel.gameObject.SetActive(true);
        // Force loading panel fully opaque before anything renders
        SetPanelAlpha(1f);
    }

    void Start()
    {
        if (markerCanvas != null) markerCanvas.SetActive(false);

        player.transform.position = new Vector3(spawnPosition.position.x, player.transform.position.y, spawnPosition.position.z);
        player.GetComponent<CapsuleCollider>().enabled = false;
        player.GetComponent<PlayerMovement>().disablePlayerMovement();
        currCamera.GetComponent<CameraFollow>().enabled = false;

        // Run loading screen first, then cinematic
        StartCoroutine(LoadingThenCinematic());
    }

    // ---------------------------------------------------------------
    // LOADING SCREEN
    // ---------------------------------------------------------------

    private IEnumerator LoadingThenCinematic()
    {
        // Phase 1: Wait for shaders/textures to warm up
        for (int i = 0; i < warmUpFrames; i++)
        {
            yield return new WaitForEndOfFrame();
        }

        // Phase 2: Fade out the black panel
        float elapsed = 0f;
        while (elapsed < loadingFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / loadingFadeDuration));
            SetPanelAlpha(alpha);
            yield return null;
        }

        // Phase 3: Hide/destroy the panel and start the cinematic
        if (loadingPanel != null)
            Destroy(loadingPanel.gameObject);

        StartCoroutine(startAnimation());
    }

    private void SetPanelAlpha(float alpha)
    {
        if (loadingPanel != null)
        {
            Color c = loadingPanel.color;
            c.a = alpha;
            loadingPanel.color = c;
        }
    }

    // ---------------------------------------------------------------
    // CINEMATIC
    // ---------------------------------------------------------------

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

            endPos.y = startPos.y;

            Vector3 moveDirection = (endPos - startPos).normalized;

            Quaternion targetRotation = player.transform.rotation;
            if (moveDirection != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(moveDirection);
            }

            playerAnim.SetBool("isWalking", true);

            float elapsedTime = 0f;
            while (elapsedTime < timePerStep)
            {
                float completionPercentage = elapsedTime / timePerStep;

                player.transform.position = Vector3.Lerp(startPos, endPos, completionPercentage);
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            player.transform.position = endPos;
            player.transform.rotation = targetRotation;
        }

        // 4. Movement finished: Stop the animation
        playerAnim.SetBool("isWalking", false);

        player.transform.rotation = Quaternion.identity;

        // 5. Enable interactivity and UI
        player.GetComponent<CapsuleCollider>().enabled = true;

        yield return new WaitForSeconds(0.5f);

        if (markerCanvas != null)
            markerCanvas.SetActive(true);

        player.transform.position = cinematicSteps[cinematicSteps.Length - 1].position;
    }

    // ---------------------------------------------------------------
    // QUEST FLOW
    // ---------------------------------------------------------------

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

    public TextMeshProUGUI questText;
    public void StartNextQuest()
    {
        if (allScenarios.Length == 0 || selectedScenarioIndex >= allScenarios.Length)
        {
            return;
        }

   

        ScenarioData currentScenario = allScenarios[selectedScenarioIndex];

        if (currentUseCaseIndex >= currentScenario.useCases.Length)
        {
            FinishGameSession();
            return;
        }

        UseCaseTasks currentUseCase = currentScenario.useCases[currentUseCaseIndex];


        questText.text = currentUseCase.useCaseName;

        if (currentQuestIndex < currentUseCase.quests.Length)
        {
            Quest nextQuest = currentUseCase.quests[currentQuestIndex];
            currentQuestIndex++;

            if (nextQuest != null)
            {
                QuestManager.Instance.StartQuest(nextQuest);
            }
            else
            {
                StartNextQuest();
            }
        }
        else
        {
            currentUseCaseIndex++;
            currentQuestIndex = 0;
            StartNextQuest();
        }
    }

    // ---------------------------------------------------------------
    // END GAME SEQUENCE
    // ---------------------------------------------------------------

    void FinishGameSession()
    {
        StartCoroutine(EndGameSequence());
    }

    public PauseMenu pauseManager;
    private IEnumerator EndGameSequence()
    {
        
        pauseManager.enabled = false;
        TimeManager.Instance.StopTimer();
        // 1. Enable marker and fade it in
        if (markerCanvas != null)
        {
            markerCanvas.SetActive(true);
            UIFader uf = markerCanvas.GetComponent<UIFader>();
            if (uf != null) uf.FadeIn();

            markerCanvas.GetComponentInChildren<RequirementHighlighter>().enabled = false;
        }

        // 2. Disable PageBackground instantly
        if (pageBackground != null)
        {
            pageBackground.SetActive(false);
        }

        // 3. Move UMLBackground to the center instantly
        if (umlBackground != null)
        {
            // Assuming the anchor is perfectly centered in your canvas layout
            umlBackground.anchoredPosition = Vector2.zero;
            umlBackground.localScale = umlTargetScale;
        }

        // 4. Move timer to center X and scale it up with an animation
        if (timerRect != null)
        {
            // Get the parent of the timer to calculate the true center
            RectTransform parentRect = timerRect.parent.GetComponent<RectTransform>();

            // Find the horizontal center of the parent in world space
            Vector3 parentCenterWorld = parentRect.TransformPoint(parentRect.rect.center);

            // Animate using world position so anchors don't mess up the math
            Vector3 startPos = timerRect.position;
            Vector3 targetPos = new Vector3(parentCenterWorld.x, startPos.y, startPos.z); // Keep original Y and Z
            Vector3 startScale = timerRect.localScale;

            float elapsed = 0f;
            while (elapsed < timerAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / timerAnimDuration);

                // Adding a smoothstep for a more polished UI animation feel
                float smoothT = t * t * (3f - 2f * t);

                timerRect.position = Vector3.Lerp(startPos, targetPos, smoothT);
                timerRect.localScale = Vector3.Lerp(startScale, timerTargetScale, smoothT);

                yield return null;
            }

            // Snap exactly to target at the end of the loop
            timerRect.position = targetPos;
            timerRect.localScale = timerTargetScale;
        }

        yield return new WaitForSeconds(6f);

        // 6. Finish game logic
        if (DataManager.Instance == null)
        {
            Debug.Log("No data found!");
        }
        else
        {
            DataManager.Instance.EndGame(TimeManager.Instance.timeElapsed);
        }

        SceneManager.LoadScene("MainMenu");
    }
}
