using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Camera Settings")]
    public float lerpDuration = 1.5f;

    private Camera mainCamera;
    private Coroutine lerpCoroutine;

    public GameObject roomsObject;

    private Vector3 enteringPosition;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void OnTriggerEnter(Collider other)
    {
        enteringPosition = gameObject.transform.position;
    }

    void OnTriggerExit(Collider other)
    {
        Vector3 exitDirection = GetExitDirection(other);
    

        Transform nextRoom = GetClosestRoomInDirection(exitDirection);
        if (nextRoom == null) return;

        Debug.Log("Geçilen oda: " + nextRoom.parent.name);

        if (lerpCoroutine != null)
            StopCoroutine(lerpCoroutine);

        lerpCoroutine = StartCoroutine(LerpCameraToRoom(nextRoom.position));
    }


    Transform GetClosestRoomInDirection(Vector3 exitDirection)
    {
        Transform rooms = GameObject.Find("Rooms").transform;
        Transform currentRoom = transform.parent;
        Transform best = null;
        float bestScore = Mathf.Infinity;

        foreach (Transform room in rooms)
        {
            if (room == currentRoom) continue;

            Transform center = room.Find("Center");
            if (center == null) continue;

            Vector3 toRoom = center.position - transform.position;
            float dot = Vector3.Dot(exitDirection, toRoom.normalized);

            if (dot < 0.5f) continue;

            float dist = toRoom.magnitude;
            if (dist < bestScore)
            {
                bestScore = dist;
                best = room.Find("CameraPosition");
            }
        }

        return best;
    }

    Vector3 GetExitDirection(Collider other)
    {
        Vector3 direction = transform.position - enteringPosition;

        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);
        float absZ = Mathf.Abs(direction.z);

        if (absX > absY && absX > absZ)
            return direction.x > 0 ? Vector3.right : Vector3.left;
        else if (absZ > absX && absZ > absY)
            return direction.z > 0 ? Vector3.forward : Vector3.back;
        else
            return direction.y > 0 ? Vector3.up : Vector3.down;
    }

    System.Collections.IEnumerator LerpCameraToRoom(Vector3 targetPosition)
    {
        Vector3 startPosition = mainCamera.transform.position;
        float elapsed = 0f;

        targetPosition = new Vector3(targetPosition.x, startPosition.y, targetPosition.z);

        while (elapsed < lerpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lerpDuration;
            t = t * t * (3f - 2f * t);

            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        mainCamera.transform.position = targetPosition;
    }
}
