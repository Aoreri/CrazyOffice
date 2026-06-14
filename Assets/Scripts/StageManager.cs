using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class UseCaseTasks
{
    public string useCaseName;
    public Quest[] quests;
}

[System.Serializable]
public class ScenarioData
{
    public string scenarioName;
    public UseCaseTasks[] useCases = new UseCaseTasks[3];
}

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
        // FIX: Duplicate instance varsa yok et
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        loadingPanel.gameObject.SetActive(true);
        SetPanelAlpha(1f);
    }

    void Start()
    {
        if (markerCanvas != null) markerCanvas.SetActive(false);

        player.transform.position = new Vector3(
            spawnPosition.position.x,
            player.transform.position.y,
            spawnPosition.position.z
        );

        player.GetComponent<CapsuleCollider>().enabled = false;
        player.GetComponent<PlayerMovement>().disablePlayerMovement();
        currCamera.GetComponent<CameraFollow>().enabled = false;

        StartCoroutine(LoadingThenCinematic());
    }

    // ---------------------------------------------------------------
    // LOADING SCREEN
    // ---------------------------------------------------------------

    private IEnumerator LoadingThenCinematic()
    {
        // Phase 1: Shader/texture warm-up
        for (int i = 0; i < warmUpFrames; i++)
            yield return new WaitForEndOfFrame();

        // Phase 2: Fade out the black panel
        float elapsed = 0f;
        while (elapsed < loadingFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / loadingFadeDuration));
            SetPanelAlpha(alpha);
            yield return null;
        }

        // Phase 3: Destroy panel and start cinematic
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
        // 1. Door opening sequence
        yield return new WaitForSeconds(0.8f);
        door.GetComponent<DoorScript>().ToggleDoor(true);
        yield return new WaitForSeconds(0.7f);

        // 2. Enable camera and grab components
        currCamera.GetComponent<CameraFollow>().enabled = true;
        Animator playerAnim = player.GetComponentInChildren<Animator>();
        Rigidbody playerRb = player.GetComponent<Rigidbody>();

        // Physics ve root motion'ı kapat — transform'la çakışmasın
        if (playerRb != null) playerRb.isKinematic = true;
        if (playerAnim != null) playerAnim.applyRootMotion = false;

        // 3. Her step için hareket ve rotasyon
        foreach (Transform step in cinematicSteps)
        {
            Vector3 startPos = player.transform.position;
            Vector3 endPos = step.position;
            endPos.y = startPos.y; // Y eksenini koru

            Vector3 moveDirection = (endPos - startPos).normalized;

            Quaternion targetRotation = player.transform.rotation;
            if (moveDirection != Vector3.zero)
                targetRotation = Quaternion.LookRotation(moveDirection);

            playerAnim.SetBool("isWalking", true);

            float elapsedTime = 0f;
            while (elapsedTime < timePerStep)
            {
                // FIX: Önce artır → completionPercentage 1.0'a ulaşabilir, snap sıçraması olmaz
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / timePerStep);

                player.transform.position = Vector3.Lerp(startPos, endPos, t);

                // FIX: Exp formülü → rotasyon FPS'ten bağımsız, 30/60/120 FPS'te aynı hız
                float rotT = 1f - Mathf.Exp(-turnSpeed * Time.deltaTime);
                player.transform.rotation = Quaternion.Slerp(
                    player.transform.rotation,
                    targetRotation,
                    rotT
                );

                yield return null;
            }

            // Tam pozisyon/rotasyona snap (fark artık çok küçük, sıçrama görünmez)
            player.transform.position = endPos;
            player.transform.rotation = targetRotation;
        }

        // 4. Hareketi durdur
        playerAnim.SetBool("isWalking", false);
        player.transform.rotation = Quaternion.identity;

        // Physics engine'i sync'le, sonra collider'ı aç
        Physics.SyncTransforms();
        player.GetComponent<CapsuleCollider>().enabled = true;

        // Physics ve root motion'ı geri ver
        if (playerRb != null) playerRb.isKinematic = false;
        if (playerAnim != null) playerAnim.applyRootMotion = true;

        // FIX: PlayerMovement'ı aç — StartScenarioQuests beklenmeden hareket edebilsin
        player.GetComponent<PlayerMovement>().enablePlayerMovement();

        yield return new WaitForSeconds(0.5f);

        if (markerCanvas != null)
            markerCanvas.SetActive(true);

        // NOT: Son pozisyon ataması kaldırıldı — foreach zaten endPos'a snap'ledi
    }

    // ---------------------------------------------------------------
    // QUEST FLOW
    // ---------------------------------------------------------------

    public void StartScenarioQuests()
    {
        if (markerCanvas != null)
        {
            UIFader uf = markerCanvas.GetComponent<UIFader>();
            if (uf != null) uf.FadeOut();
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
            return;

        ScenarioData currentScenario = allScenarios[selectedScenarioIndex];

        if (currentUseCaseIndex >= currentScenario.useCases.Length)
        {
            FinishGameSession();
            return;
        }

        UseCaseTasks currentUseCase = currentScenario.useCases[currentUseCaseIndex];
        questText.text = currentUseCase.useCaseName;

        // FIX: Null quest'leri atlamak için while döngüsü — sonsuz recursive döngü riski yok
        while (currentQuestIndex < currentUseCase.quests.Length)
        {
            Quest nextQuest = currentUseCase.quests[currentQuestIndex];
            currentQuestIndex++;

            if (nextQuest != null)
            {
                QuestManager.Instance.StartQuest(nextQuest);
                return; // Quest bulundu, çık
            }
            // null ise döngü devam eder, bir sonraki quest'e bakar
        }

        // Bu UseCase'in tüm quest'leri bitti (veya hepsi null)
        currentUseCaseIndex++;
        currentQuestIndex = 0;
        StartNextQuest();
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

        // 1. MarkerCanvas'ı aç ve fade-in yap
        if (markerCanvas != null)
        {
            markerCanvas.SetActive(true);
            UIFader uf = markerCanvas.GetComponent<UIFader>();
            if (uf != null) uf.FadeIn();

            markerCanvas.GetComponentInChildren<RequirementHighlighter>().enabled = false;
        }

        // 2. PageBackground'ı kapat
        if (pageBackground != null)
            pageBackground.SetActive(false);

        // 3. UMLBackground'ı merkeze taşı
        if (umlBackground != null)
        {
            umlBackground.anchoredPosition = Vector2.zero;
            umlBackground.localScale = umlTargetScale;
        }

        // 4. Timer'ı merkeze animasyonla taşı
        if (timerRect != null)
        {
            // FIX: anchoredPosition kullan — world space anchor hesabı Canvas moduna göre hatalı olabilirdi
            Vector2 startAnchoredPos = timerRect.anchoredPosition;
            Vector2 targetAnchoredPos = new Vector2(0f, startAnchoredPos.y); // X'i merkeze al, Y'yi koru
            Vector3 startScale = timerRect.localScale;

            float elapsed = 0f;
            while (elapsed < timerAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / timerAnimDuration);
                float smoothT = t * t * (3f - 2f * t); // Smoothstep

                timerRect.anchoredPosition = Vector2.Lerp(startAnchoredPos, targetAnchoredPos, smoothT);
                timerRect.localScale = Vector3.Lerp(startScale, timerTargetScale, smoothT);

                yield return null;
            }

            // Tam hedefe snap
            timerRect.anchoredPosition = targetAnchoredPos;
            timerRect.localScale = timerTargetScale;
        }

        yield return new WaitForSeconds(6f);

        // 5. Oyunu bitir
        if (DataManager.Instance == null)
            Debug.Log("No data found!");
        else
            DataManager.Instance.EndGame(TimeManager.Instance.timeElapsed);

        SceneManager.LoadScene("MainMenu");
    }
}