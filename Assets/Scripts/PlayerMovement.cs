using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private bool disableMovement = false;

    public float speed = 5f;
    public float rotationSpeed = 0.5f; // higher = snappier

    public Vector2 moveInput;
    private Rigidbody rb;
    private Animator animator;

    private Vector3 lastDirection = Vector3.forward; // keeps facing when idle

    int walkingHash;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        walkingHash = Animator.StringToHash("isWalking");
    }

    void FixedUpdate()
    {
        if (disableMovement)
            return;


        // Movement
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 targetVelocity = move * speed;
  
        rb.linearVelocity = targetVelocity;

        // Animation
        animator.SetBool(walkingHash, moveInput.magnitude > 0);

        // Rotation (smooth)
        if (moveInput != Vector2.zero)
        {
            lastDirection = move.normalized;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lastDirection);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void OnMove(InputValue value)
    {
        if (disableMovement)
            return;

        moveInput = value.Get<Vector2>();
    }


    public void disablePlayerMovement()
    {
        disableMovement = true;
        moveInput = Vector2.zero;
    }

    public void enablePlayerMovement()
    {
        disableMovement = false;
        moveInput = Vector2.zero;
    }
}