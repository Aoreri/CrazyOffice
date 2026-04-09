using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 moveInput;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 targetVelocity = new Vector3(moveInput.x * speed, rb.linearVelocity.y, moveInput.y * speed);

        rb.linearVelocity = targetVelocity;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}