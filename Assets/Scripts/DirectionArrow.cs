using UnityEngine;

public class DirectionArrow : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player that the arrow will orbit around.")]
    public Transform player;

    [Tooltip("The current object the arrow should point to. Can be null.")]
    public Transform target;

    [Header("Position Settings")]
    [Tooltip("How far away from the player the arrow should float.")]
    public float radius = 1.5f;

    [Tooltip("Height offset so the arrow floats at chest/head level instead of the floor.")]
    public float heightOffset = 1f;

    [Header("Rotation Settings")]
    [Tooltip("If true, the arrow stays flat on the X/Z plane. If false, it will tilt up/down to point at the exact target height.")]
    public bool flattenY = true;

    void Update()
    {
        // If there is no target, hide the arrow and stop calculating
        if (target == null)
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
            return;
        }

        // Make sure the arrow is visible if we have a target
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        if (player != null)
        {
            UpdateArrowPositionAndRotation();
        }
    }

    private void UpdateArrowPositionAndRotation()
    {
        // 1. Calculate the direction from the player to the target
        Vector3 direction = target.position - player.position;

        // Flatten the Y axis so the arrow doesn't point into the sky or ground
        if (flattenY)
        {
            direction.y = 0;
        }

        // Prevent errors if the player is standing exactly inside the target
        if (direction.sqrMagnitude > 0.01f)
        {
            direction.Normalize(); // Make the direction vector exactly 1 unit long

            // 2. Position the arrow in a circle around the player
            Vector3 centerPoint = player.position + (Vector3.up * heightOffset);
            transform.position = centerPoint + (direction * radius);

            // 3. Rotate the arrow to look at the target
            // Note: This assumes your arrow 3D model's "Forward" (Z-axis) is the pointy end
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    // Call this method from your other scripts to change what the arrow is pointing at
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}