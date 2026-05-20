using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isOpen = false;
    public bool closeable = false;
    public bool openable = true;
    public float openAngle = 90f;
    public float rotationSpeed = 3f;

    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public Transform player;

    [Header("Auto Close Settings")]
    public bool enableAutoClose = true;
    
    public float autoCloseTime = 3f;
    private float openTimer = 0f;

    private Quaternion targetRotation;
    private Quaternion startRotation;

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

        // Save the initial closed rotation
        startRotation = transform.localRotation;
    }

    void Update()
    {
        // 1. Check for Input and Distance
        if (player != null && Input.GetKeyDown(KeyCode.E))
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= interactionDistance)
            {
                ToggleDoor();
            }
        }

        // 2. Auto-Close Logic
        if (isOpen && enableAutoClose)
        {
            openTimer += Time.deltaTime;

            if (openTimer >= autoCloseTime)
            {
                isOpen = false;
            }
        }

        // 3. Handle the smooth rotation
        if (isOpen)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, startRotation, Time.deltaTime * rotationSpeed);
        }
    }

    public void ToggleDoor()
    {
        if (!openable)
            return;

        if (isOpen && !closeable)
            return;

        isOpen = !isOpen;

        if (isOpen)
        {
            openTimer = 0f; // Reset auto-close timer

            // Determine which side the player is on
            if (player != null)
            {
                // Get the direction from the door to the player
                Vector3 directionToPlayer = (player.position - transform.position).normalized;

                // Compare the door's forward face with the player's direction
                float dotProduct = Vector3.Dot(-transform.right, directionToPlayer);

                float actualAngle = openAngle;

                // If dot product is positive, the player is in front of the door.
                // We flip the angle so it opens away from them.
                if (dotProduct > 0)
                {
                    actualAngle = -openAngle;
                }

                // Set the new target rotation based on the calculation
                targetRotation = Quaternion.Euler(0, actualAngle, 0) * startRotation;
            }
            else
            {
                // Fallback if player isn't assigned
                targetRotation = Quaternion.Euler(0, openAngle, 0) * startRotation;
            }
        }
    }
}