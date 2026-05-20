using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 10, -6);
    public float smoothTime = 0.15f;

    [Tooltip("Check this if your player moves using a Rigidbody or FixedUpdate")]
    public bool useFixedUpdate = false;

    private Vector3 velocity = Vector3.zero;
    public Quaternion fixedRotation = Quaternion.Euler(40f, 0f, 0f);

    // Runs every frame (good for standard movement)
    void LateUpdate()
    {
        if (!useFixedUpdate)
        {
            FollowTarget();
        }
    }

    // Runs in sync with the physics engine
    void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            FollowTarget();
        }
    }

    // The core movement logic moved into its own function
    private void FollowTarget()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        transform.rotation = fixedRotation;
    }
}