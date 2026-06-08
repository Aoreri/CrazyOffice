using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }

    public GameObject assignedNPC;
    public Transform player;

    [Header("Debounce Ayarları")]
    // YENİ EKLENDİ: F tuşu spam engelleme (Cooldown)
    public float interactCooldown = 0.5f;
    private float nextInteractTime = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (player == null)
        {
            PlayerController pScript = FindAnyObjectByType<PlayerController>();
            if (pScript != null)
                player = pScript.gameObject.transform;
        }
    }

    void Update()
    {
        // 1. Check for Input and Distance
        // GÜNCELLENDİ: Araya "Time.time >= nextInteractTime" kilit şartı eklendi
        if (player != null && assignedNPC != null && Input.GetKeyDown(KeyCode.F) && Time.time >= nextInteractTime)
        {
            float distance = Vector3.Distance(assignedNPC.transform.position, player.position);

            if (distance <= 5)
            {
                // Etkileşim başarılı! Spam engellemek için kilidi yarım saniyeliğine kapatıyoruz
                nextInteractTime = Time.time + interactCooldown;

                QuestManager.Instance.OnNPCTalkedTo(assignedNPC);
            }
        }
    }
}