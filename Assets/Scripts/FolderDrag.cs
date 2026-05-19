using UnityEngine;
using UnityEngine.EventSystems;

public class FolderDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drop Settings")]
    public Transform dropZoneCenter;
    public float dropRadius = 50f;

    private Vector3 _startPosition;
    private bool _isSolved = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isSolved) return;
        _startPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isSolved) return;
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isSolved) return;

        float distance = Vector2.Distance(transform.position, dropZoneCenter.position);

        if (distance <= dropRadius)
        {
            // Snap to center and mark as solved so it can't be dragged again
            transform.position = dropZoneCenter.position;
            _isSolved = true;
            Debug.Log("Folder dropped in the correct zone. Starting upload...");
            transform.GetComponent<RectTransform>().localScale = Vector3.zero;
            // Trigger the upload sequence instead of instantly ending the puzzle
            PresentationUpload uploadScript = gameObject.GetComponentInParent<PresentationUpload>();
            if (uploadScript != null)
            {
                uploadScript.StartUploadSequence();
            }
        }
        else
        {
            // Snap back if they missed
            transform.position = _startPosition;
            Debug.Log("Missed the zone. Snapping back.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (dropZoneCenter != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(dropZoneCenter.position, dropRadius);
        }
    }
}