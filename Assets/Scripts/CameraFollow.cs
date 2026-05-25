using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 10, -6);
    public float smoothTime = 0.15f;

    [Header("Dynamic Shift & Rotation (A/D Keys)")]
    [Tooltip("How much the X position shifts when moving left/right.")]
    public float xShiftAmount = 2f;
    [Tooltip("How many degrees the camera rotates on the Y axis. Moving left rotates right.")]
    public float yRotationAmount = 10f;
    [Tooltip("How smoothly the camera returns to normal when releasing the keys.")]
    public float transitionSpeed = 5f;

    [Header("Base Camera Rotation")]
    [Tooltip("Set the exact X, Y, Z rotation you want the camera to maintain at rest.")]
    public Vector3 fixedRotationAngles = new Vector3(40f, 0f, 0f);

    private Vector3 velocity = Vector3.zero;

    // Tracks the current active shifts
    private float currentShiftX = 0f;
    private float currentRotY = 0f;

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Get Input (-1 for A, 1 for D, 0 for none)
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. Calculate Targets
        // Position shifts the SAME direction as input (A = left shift)
        float targetShiftX = horizontalInput * xShiftAmount;

        // Rotation rotates the OPPOSITE direction (Notice the minus sign: A = right turn)
        float targetRotY = horizontalInput * -yRotationAmount;

        // 3. Smoothly interpolate current shifts to the target shifts
        currentShiftX = Mathf.Lerp(currentShiftX, targetShiftX, Time.deltaTime * transitionSpeed);
        currentRotY = Mathf.Lerp(currentRotY, targetRotY, Time.deltaTime * transitionSpeed);

        // 4. Apply Dynamic Position
        Vector3 desiredPosition = target.position + offset;
        desiredPosition.x += currentShiftX;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        // 5. Apply Dynamic Rotation
        Vector3 finalRotation = fixedRotationAngles;
        finalRotation.y += currentRotY; // Add our dynamic Y rotation to the base rotation
        transform.rotation = Quaternion.Euler(finalRotation);
    }
}