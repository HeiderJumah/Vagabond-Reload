using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Information")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    private Camera mainCamera;
    private Vector3 input;
    private Rigidbody rb;

    [Header("bools")]
    private bool isJumping = false;
    private bool isGrounded = false;

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();

        // Lock cursor to the game window
        Cursor.lockState = CursorLockMode.Confined;
    }
    /// <summary>
    /// always reads player input   
    /// </summary>
    void Update()
    {
        ReadInput();
    }
    /// <summary>
    /// Updates camera, check for ground and moves or rotates player at fixed intervals
    /// </summary>
    private void FixedUpdate()
    {
        CheckGrounded();
        MovePlayer();
        UpdateCamera();
        RotatePlayer();
    }
    /// <summary>
    /// Reads keyboard input for movement
    /// </summary>
    private void ReadInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        // Determine movement direction based on WASD keys
        float horizontal = 0f;
        float vertical = 0f;

        if(keyboard.aKey.isPressed) horizontal -= 1f;
        if(keyboard.dKey.isPressed) horizontal += 1f;
        if(keyboard.wKey.isPressed) vertical += 1f;
        if(keyboard.sKey.isPressed) vertical -= 1f;

        input = new Vector3(horizontal, 0f, vertical).normalized;

        // Handle jump input
        if (keyboard.spaceKey.wasPressedThisFrame && isGrounded && !isJumping)
        {
            isJumping = true;
        }

        // Handle escape key to unlock cursor
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
    }

    /// <summary>
    /// Moves the player based on input or goes idle if no input is detected
    /// </summary>
    private void MovePlayer()
    {
        if(input != Vector3.zero)
        {
            // Move the player
            Vector3 moveDirection = input * moveSpeed * Time.fixedDeltaTime;
            transform.position += moveDirection;
        }
        else 
        {
            // Idle state
        }

        if (isJumping)
        {
            Jump();
        }
    }
    /// <summary>
    /// Handles player jumping 
    /// </summary>
    private void Jump()
    { 
        if (rb != null)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        isJumping = false;
    }
    /// <summary>
    /// always checks if the player is grounded
    /// </summary>
    private void CheckGrounded()
    {
        // Raycast down to check if the player is grounded
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, 1.1f, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    /// <summary>
    /// Handles player rotation to face the mouse cursor
    /// </summary>
    private void RotatePlayer() 
    {
        // Ensure mouse is available
        if (Mouse.current == null)
            return;

        // Get mouse position
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        //Convert mouse position to world point
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCamera.transform.position.y));
        
        // Calculate direction from player to mouse position
        Vector3 direction = mouseWorldPos - transform.position;
        
        // Keep only horizontal rotation
        direction.y = 0f;
        // Rotate player towards mouse position
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

    }
    /// <summary>
    /// Update the camera position to follow the player from above
    /// </summary>
    private void UpdateCamera()
    {
        // Set camera position above the player and slightly behind
        Vector3 camPos = transform.position + new Vector3(0f, 10f, -3f);
        // Maintain camera's current horizontal position
        mainCamera.transform.position = camPos;
        // Set camera rotation to look down in a fixed angle
        mainCamera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
    }
}
