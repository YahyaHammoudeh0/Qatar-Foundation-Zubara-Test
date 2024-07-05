using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class FPSController : MonoBehaviour
{
    [SerializeField]
    public Camera playerCamera; // Ensure this is the camera inside the capsule
    public float moveSpeed = 6f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float gravity = 9.81f;
    public float jumpForce = 5f; // Jump force
    public float groundCheckDistance = 0.1f;

    private PlayerControls inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private bool isGrounded;
    private float rotationX = 0;

    void Awake()
    {
        inputActions = new PlayerControls();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();

        // Subscribe to input actions
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLookCanceled;
        inputActions.Player.Jump.performed += OnJump; // Subscribe to jump action
    }

    void OnDisable()
    {
        inputActions.Player.Disable();

        // Unsubscribe from input actions
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLookCanceled;
        inputActions.Player.Jump.performed -= OnJump; // Unsubscribe from jump action
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;  // Ensure Rigidbody rotation is frozen
    }

    void FixedUpdate()
    {
        HandleMovement();
        ApplyGravity();
    }

    void Update()
    {
        HandleRotation();
    }

    private void HandleMovement()
    {
        // Check if the player is grounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + groundCheckDistance);

        Debug.DrawRay(transform.position, Vector3.down * (capsuleCollider.bounds.extents.y + groundCheckDistance), Color.red);

        // Apply movement
        if (moveInput != Vector2.zero)
        {
            Vector3 move = transform.forward * moveInput.y + transform.right * moveInput.x;
            rb.velocity = new Vector3(move.x * moveSpeed, rb.velocity.y, move.z * moveSpeed);
        }
        else
        {
            // Maintain current vertical velocity while stopping horizontal movement
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            // Apply gravity downward
            rb.velocity += Vector3.up * Physics.gravity.y * Time.fixedDeltaTime;
        }
        else
        {
            // Reset vertical velocity if grounded
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y > 0 ? rb.velocity.y : 0, rb.velocity.z);
        }
    }

    private void HandleRotation()
    {
        if (lookInput != Vector2.zero)
        {
            // Horizontal rotation affects the player body
            float rotationY = lookInput.x * lookSpeed * Time.deltaTime;
            transform.Rotate(0, rotationY, 0);

            // Vertical rotation affects the camera
            rotationX += -lookInput.y * lookSpeed * Time.deltaTime;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;  // Reset move input when stick is released
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        lookInput = Vector2.zero;  // Reset look input when stick is released
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            // Apply jump force if grounded
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Sqrt(jumpForce * 2f * gravity), rb.velocity.z);
        }
    }
}
