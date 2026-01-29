using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Information")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float dodgeCooldown = 1f;
    [SerializeField] private float dodgeSpeed = 2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    private Camera mainCamera;
    private Vector3 input;
    private Rigidbody rb;
    private PlayerAnimation playerAnimation;

    [Header("bools")]
    private bool isJumping = false;
    private bool isGrounded = false;
    private bool canDodge = true;
    private bool isDodging = false;

    [Header("CameraSettings")]
    [SerializeField] private float cameraHeight = 5f; // default camera height
    [SerializeField] private float cameraDistance = 3f; // default camera distance
    [SerializeField] private float cameraAngle = 60f; // default camera angle
    [SerializeField] private float cameraRotationSpeed = 70f; // camera rotation speed
    // clamp camera angle movement (up and down)
    [SerializeField] private float minPitch = 45f; 
    [SerializeField] private float maxPitch = 65f;
    [SerializeField] private float cameraSmoothRotate = 10f; // smoothness factor for camera rotation
    [SerializeField] private float cameraZoom = 1f; // camera zoom sensitivity / zoom speed
    public bool lockCamera = false;
    private float scrollCamera;


    private float cameraYaw; 

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        playerAnimation = GetComponent<PlayerAnimation>();
        // set initial camera position for smooth scrolling 
        scrollCamera = cameraDistance; 
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

        if (lockCamera)
             UpdateFixedCamera();
        else
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
            StartCoroutine(JumpCoroutine());
        }

        if (keyboard.leftShiftKey.wasPressedThisFrame)
        {
            Dodge();
        }

        // Handle escape key to unlock cursor
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
           Cursor.lockState = Cursor.lockState != CursorLockMode.None ?  CursorLockMode.None : CursorLockMode.Confined;
        }
    }
    private void Dodge()
    {
        // Dodge in the direction of movement
        if (isGrounded && !isJumping && canDodge)
        {
            Vector3 dodgeDirection;

            if (input == Vector3.zero)
            {
                dodgeDirection = -transform.forward;
            }
            else
            {
                dodgeDirection = transform.right * input.x + transform.forward * input.z;
            }

            dodgeDirection.Normalize();
            canDodge = false;
            isDodging = true;
            rb.linearVelocity = new Vector3(dodgeDirection.x * dodgeSpeed, rb.linearVelocity.y, dodgeDirection.z * dodgeSpeed);
            // Set dodge animation for the player dodge direction
            Vector3 localDodgeDirection = transform.InverseTransformDirection(dodgeDirection);
            if (Mathf.Abs(localDodgeDirection.x) > Mathf.Abs(localDodgeDirection.z))
            {
                // Dodge left or right
                playerAnimation.SetAnimationState(localDodgeDirection.x > 0 ? PlayerAnimationState.DodgeRight : PlayerAnimationState.DodgeLeft);
            }
            else
            {
                // Dodge forward or backward
                playerAnimation.SetAnimationState(localDodgeDirection.z > 0 ? PlayerAnimationState.DodgeUp : PlayerAnimationState.DodgeBackwards);
            }

            StartCoroutine(DodgeCooldownCoroutine());

        }

    }

    private IEnumerator JumpCoroutine()
    {
        isJumping = true;
        playerAnimation.SetAnimationState(PlayerAnimationState.Jump);
        yield return new WaitForSeconds(0.2f); // Small delay to sync with animation
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        yield return new WaitForSeconds(0.1f); // Allow some time before checking for grounded state

        yield return new WaitUntil(() => isGrounded);
        isJumping = false;
    }

    private IEnumerator DodgeCooldownCoroutine()
    {
        yield return new WaitForSeconds(0.8f); // Duration of dodge
        isDodging = false;
        Debug.Log(isDodging);
        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
        Debug.Log(canDodge);
    }

    /// <summary>
    /// Moves the player based on input or goes idle if no input is detected
    /// </summary>
    private void MovePlayer()
    {
        if (isDodging) return; // Skip movement during dodge
        if (input != Vector3.zero)
        {
            // Move the player
         //   Vector3 moveDirection = input * moveSpeed * Time.fixedDeltaTime;
           // transform.position += moveDirection;
           Vector3 moveDirection = transform.right * input.x + transform.forward * input.z;
            moveDirection *= moveSpeed * Time.fixedDeltaTime;
            transform.position += moveDirection;
            if (isGrounded && !isJumping)
            {
                // Set dodge animation for the player dodge direction
                Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection.normalized); 
                if (Mathf.Abs(localMoveDirection.x) > Mathf.Abs(localMoveDirection.z))
                {
                    // Move left or right
                    playerAnimation.SetAnimationState(localMoveDirection.x > 0 ? PlayerAnimationState.RightStrife : PlayerAnimationState.LeftStrife);
                }
                else
                {
                    // Move forward or backward
                    playerAnimation.SetAnimationState(localMoveDirection.z > 0 ? PlayerAnimationState.Walk : PlayerAnimationState.BackwardsWalk);
                }
            }
        }
        else 
        {
            // Idle state
            playerAnimation.SetAnimationState(PlayerAnimationState.Idle);
        }
    }
    /// <summary>
    /// always checks if the player is grounded
    /// </summary>
    private void CheckGrounded()
    {
        // Raycast down to check if the player is grounded
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, 0.5f, groundLayer))
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
        // Additional camera rotation from keyboard input
        float keyboardCameraRotate = 0f;
        // Rotate camera based on keyboard input
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.qKey.isPressed) keyboardCameraRotate -= 1f;
            if (keyboard.eKey.isPressed) keyboardCameraRotate += 1f;
        }

        if(Mouse.current == null) return;
        // Camera zoom with mouse wheel 
        Vector2 scroll = Mouse.current.scroll.ReadValue();

        scrollCamera -= scroll.y * cameraZoom;
        // clamp camera distance so camera doesnt go inside player or too far away
        scrollCamera = Mathf.Clamp(scrollCamera, 2f, 5f);
        // scroll camera distance smoothly 
        cameraDistance = Mathf.Lerp(cameraDistance, scrollCamera, Time.deltaTime * 4f);

        // Rotate camera based on mouse button drag
        if (Mouse.current.rightButton.isPressed)
        {
            // Get mouse delta movement
            Vector3 mouseDelta = Mouse.current.delta.ReadValue();
            // Adjust camera yaw and angle based on mouse movement
            cameraYaw += mouseDelta.x * cameraRotationSpeed * Time.fixedDeltaTime;
            cameraAngle -= mouseDelta.y * cameraRotationSpeed * Time.fixedDeltaTime;
            // Clamp camera angle
            cameraAngle = Mathf.Clamp(cameraAngle, minPitch, maxPitch);
        }
        // Apply keyboard rotation to camera yaw
        cameraYaw += keyboardCameraRotate * cameraRotationSpeed * Time.fixedDeltaTime;
        // Calculate rotation based on yaw and angle
        Quaternion rotation = Quaternion.Euler(cameraAngle, cameraYaw, 0f);
        // Calculate camera offset from player
        Vector3 offset = rotation * new Vector3(0f, 0f, -cameraDistance) + Vector3.up * cameraHeight;
        // Set camera position
        mainCamera.transform.position = transform.position + offset;
        // Set camera rotation to look at the player smoothly
        mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, rotation, Time.deltaTime * cameraSmoothRotate);

       // mainCamera.transform.rotation = rotation;
    }

    private void UpdateFixedCamera()
    {
        // Old fixed camera position code // Might be used for a fixed camera mode in the future
        // I'm planning stupid things again
        // Set camera position above the player and slightly behind
        Vector3 camPos = transform.position + new Vector3(0f, 8f, -3f);
        // Maintain camera's current horizontal position
        mainCamera.transform.position = camPos;
        // Set camera rotation to look down in a fixed angle
        mainCamera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
    }
}
