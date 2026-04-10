using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
  public float speed = 10f;

  private Vector2 moveInput;
  private Rigidbody rb;

  void Awake()
  {
    rb = GetComponent<Rigidbody>();
  }

  void FixedUpdate()
  {
    Vector3 velocity = new Vector3(
        moveInput.x * speed,
        rb.linearVelocity.y,
        moveInput.y * speed
    );

    rb.linearVelocity = velocity;
  }

  public void OnMove(InputValue value)
  {
    moveInput = value.Get<Vector2>();
  }
}
