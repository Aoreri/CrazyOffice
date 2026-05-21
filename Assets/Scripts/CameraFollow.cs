using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 10, -6);
    public float smoothTime = 0.15f; 

    private Vector3 velocity = Vector3.zero;
    public Quaternion fixedRotation = Quaternion.Euler(40f, 0f, 0f);


    void LateUpdate() 
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