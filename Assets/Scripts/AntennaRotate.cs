using UnityEngine;
using UnityEngine.EventSystems;

public class AntennaRotate : MonoBehaviour,
    IPointerDownHandler, IDragHandler
{
    [Header("Rotation Settings")]
    public float minRotationZ = -30f;
    public float maxRotationZ = 30f;
    public float sensitivity = 0.1f;

    public float antennaDelta = 0;

    private float _startMouseX;
    private float _baseRotZ;
    private float _currentRotZ;
    private bool _isDragging;

    public void Setup()
    {
        _currentRotZ = minRotationZ + (maxRotationZ - minRotationZ) * antennaDelta;
        
        if (_currentRotZ > 180f) _currentRotZ -= 360f;

        Vector3 euler = transform.localEulerAngles;
        euler.z = _currentRotZ;
        transform.localEulerAngles = euler;
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

        antennaDelta = (_currentRotZ - minRotationZ) / (maxRotationZ - minRotationZ);

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

}