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
            transform.position = dropZoneCenter.position;
            _isSolved = true;
            Debug.Log("Puzzle Solved! Folder dropped in the correct zone.");
            Destroy(transform.parent.parent.parent.parent.gameObject); //TEMPORARY I WILL REMOVE IT
        }
        else
        {
           
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