using UnityEngine;

/// <summary>
/// Singleton manager — world-space bubble edition.
/// Usage:  DialogueManager.Instance.chatStart(myGameObject, "Hello!\nLine two.");
///
/// Scene setup:
///   1. Create a Canvas, set Render Mode to "World Space", assign it to worldCanvas.
///   2. Assign the DialogueBubble prefab to bubblePrefab.
///   3. Tune worldOffset so the bubble appears above your NPCs.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Refs")]
    [Tooltip("Canvas with Render Mode = World Space")]
    public Canvas worldCanvas;

    [Tooltip("DialogueBubble prefab (RectTransform root, DialogueBubble component)")]
    public GameObject bubblePrefab;

    [Header("Positioning")]
    public Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);

    [Header("Billboard")]
    [Tooltip("Keep bubble facing the camera every frame")]
    public bool faceCamera = true;

    private DialogueBubble _activeBubble;
    private Transform _targetTransform;

    // ── lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (_activeBubble == null) { _targetTransform = null; return; }

        TrackTarget();

        bool interacted = Input.GetKeyDown(KeyCode.Space)
                       || Input.GetKeyDown(KeyCode.Return)
                       || Input.GetMouseButtonDown(0);
        if (interacted)
            _activeBubble.OnInteract();
    }

    // ── public API ───────────────────────────────────────────────────────────

    public void chatStart(GameObject target, string dialogue, System.Action onEnd = null)
        => chatStart(target, dialogue.Split('\n'), onEnd);

    public void chatStart(GameObject target, string[] lines, System.Action onEnd = null)
    {
        if (_activeBubble != null)
            Destroy(_activeBubble.gameObject);

        _targetTransform = target.transform;

        GameObject go = Instantiate(bubblePrefab, worldCanvas.transform);
        _activeBubble = go.GetComponent<DialogueBubble>();

        SnapToTarget();

        _activeBubble.Init(lines, onEnd);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private void TrackTarget()
    {
        if (_targetTransform == null || _activeBubble == null) return;
        SnapToTarget();
    }

    private void SnapToTarget()
    {
        Transform t = _activeBubble.transform;

        // World position above the NPC
        t.position = _targetTransform.position + worldOffset;

        // Billboard: face the camera
        // Forward = from bubble toward camera (so the front of the UI faces us)
        if (faceCamera && Camera.main != null)
        {
            Vector3 toCamera = Camera.main.transform.position - t.position;
            // For orthographic / 2D-style cameras, use the camera's forward directly
            // so the bubble doesn't tilt when the camera is far away.
            if (Camera.main.orthographic)
                t.rotation = Quaternion.LookRotation(-Camera.main.transform.forward,
                                                      Camera.main.transform.up);
            else
                t.rotation = Quaternion.LookRotation(toCamera.normalized,
                                                      Camera.main.transform.up);
        }
    }
}