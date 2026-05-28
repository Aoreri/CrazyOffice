using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }

    public GameObject assignedNPC;
    public Transform player;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (player == null)
        {
            PlayerController playerController = PlayerController.Instance;
            if (playerController != null)
            {
                GameObject playerObj = playerController.gameObject;
                if (playerObj != null)
                {
                    PlayerMovement pm = playerObj.GetComponent<PlayerMovement>();
                    if (pm != null)
                        pm.disablePlayerMovement();
                }
            }
        }
    }

    void Update()
    {
        // 1. Check for Input and Distance
        if (player != null && assignedNPC != null && Input.GetKeyDown(KeyCode.F))
        {
            float distance = Vector3.Distance(assignedNPC.transform.position, player.position);

            if (distance <= 5)
            {
                QuestManager.Instance.OnNPCTalkedTo(assignedNPC);
            }
        }

       
    }
}