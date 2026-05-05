using UnityEngine;
using UnityEngine.EventSystems;

public class AntennaRotate : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Rotation Settings")]
    public float minRotationZ = -11f;
    public float maxRotationZ = 11f;
    public float sensitivity = 0.1f;

    private float _startMouseX;
    private float _baseRotZ;
    private float _currentRotZ;
    private bool _isDragging;

    private void Start()
    {
       
        _currentRotZ = transform.localEulerAngles.z;

        
        if (_currentRotZ > 180f) _currentRotZ -= 360f;
    }

    public void OnPointerDown(PointerEventData e)
    {
        _startMouseX = e.position.x;
        _baseRotZ = _currentRotZ;
        _isDragging = true;
    }

    public void OnDrag(PointerEventData e)
    {
        if (!_isDragging) return;

        float delta = e.position.x - _startMouseX;
        _currentRotZ = Mathf.Clamp(_baseRotZ - delta * sensitivity, minRotationZ, maxRotationZ);

        ApplyRotation();
    }

    public void OnPointerUpHandler(PointerEventData e)
    {
        _isDragging = false;
    }

    private void ApplyRotation()
    {
        Vector3 euler = transform.localEulerAngles;
        euler.z = _currentRotZ;
        transform.localEulerAngles = euler;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}