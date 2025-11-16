using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;

    void Start()
    {
        // Get or add CharacterController
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            Debug.LogWarning("Added CharacterController automatically!");
        }

        // Create ground check if it doesn't exist
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.parent = transform;
            groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = groundCheckObj.transform;
            Debug.LogWarning("Created GroundCheck automatically!");
        }
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to stay grounded
        }

        // Get input
        Vector2 moveInput = Vector2.zero;

        // WASD
        if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;

        // Arrow Keys
        if (Keyboard.current.upArrowKey.isPressed) moveInput.y += 1f;
        if (Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1f;
        if (Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1f;
        if (Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1f;

        // Normalize diagonal movement
        if (moveInput.magnitude > 1f)
            moveInput.Normalize();

        // Sprint
        bool isSprinting = Keyboard.current.leftShiftKey.isPressed;
        currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Calculate movement direction relative to where player is looking
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Debug
        if (moveInput.magnitude > 0.01f)
        {
            Debug.Log($"Moving! Input: {moveInput} | Speed: {currentSpeed}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}