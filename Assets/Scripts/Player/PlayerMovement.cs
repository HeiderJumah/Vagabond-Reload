using System.Collections;
using System.Runtime.CompilerServices;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement Information")]
    private float moveSpeed;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float dodgeCooldown = 1f;
    [SerializeField] private float dodgeSpeed = 2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    private Camera mainCamera;
    private Vector3 input;
    private Rigidbody rb;
    private PlayerAnimation playerAnimation;
    private Coroutine idleCoroutine;
    private PlayerActions playerActions;
    [SerializeField] private PlayerStats playerStats;

    [Header("bools")]
    public bool isJumping = false;
    public bool canJump = true;
    private bool isGrounded = false;
    public bool canDodge = true;
    private bool isDodging = false;
    private bool isIdleRoutineRunning;
    public bool canMove = true;
    private bool isSlowed = false;
    private bool isConfused = false;   
    public bool isParalyzed { get; private set; } = false;
    private bool isLocked = false;  

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
    public bool lockCamera { get; private set; } = false;
    private float scrollCamera;
    private float cameraYaw;

    private readonly PlayerAnimationState[] idleVarients =
    {
        PlayerAnimationState.IdleTwo,
        PlayerAnimationState.IdleThree,
        PlayerAnimationState.IdleFour
    };

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        playerAnimation = GetComponent<PlayerAnimation>();
        playerActions = GetComponent<PlayerActions>();
        // set initial camera position for smooth scrolling 
        scrollCamera = cameraDistance; 

        moveSpeed = playerStats.speed;
    }
    /// <summary>
    /// always reads player input   
    /// </summary>
    void Update()
    {
        ReadInput();
        MovePlayer();
        CheckGrounded();

        if (lockCamera)
            UpdateFixedCamera();
        else
            UpdateCamera();

        RotatePlayer();


    }

    public void SetCamera(Camera camera)
    {
        if(camera != null)
        {
            mainCamera = camera;
        }
    }
    /// <summary>
    /// Updates camera, check for ground and moves or rotates player at fixed intervals
    /// </summary>
    private void FixedUpdate()
    {
       /* CheckGrounded();
        MovePlayer();

        if (lockCamera)
             UpdateFixedCamera();
        else
             UpdateCamera();

        RotatePlayer();*/
    }

    public void SetCameraLock(bool value)
    {
        lockCamera = value;
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

        if(!isConfused)
        {
            if(keyboard.aKey.isPressed) horizontal -= 1f;
            if(keyboard.dKey.isPressed) horizontal += 1f;
            if(keyboard.wKey.isPressed) vertical += 1f;
            if(keyboard.sKey.isPressed) vertical -= 1f;
        }
        else
        {
            if (keyboard.aKey.isPressed) horizontal += 1f;
            if (keyboard.dKey.isPressed) horizontal -= 1f;
            if (keyboard.wKey.isPressed) vertical -= 1f;
            if (keyboard.sKey.isPressed) vertical += 1f;
        }

        input = new Vector3(horizontal, 0f, vertical).normalized;

        // Handle jump input
        if (keyboard.spaceKey.wasPressedThisFrame && isGrounded && !isJumping && canJump && !playerActions.isAttacking)
        {
            isJumping = true;
            StartCoroutine(JumpCoroutine());
        }

        if (keyboard.leftShiftKey.wasPressedThisFrame)
        {
            Dodge();
        }

        if (keyboard.digit5Key.wasPressedThisFrame)
        {
            HandleEmotes(PlayerAnimationState.EmoteOne);
        }
        else if(keyboard.digit6Key.wasPressedThisFrame)
        {
            HandleEmotes(PlayerAnimationState.EmoteTwo); 
        }
        else if(keyboard.digit7Key.wasPressedThisFrame)
        {
            HandleEmotes(PlayerAnimationState.EmoteThree); 
        }
        else if(keyboard.digit8Key.wasPressedThisFrame)
        {
            HandleEmotes(PlayerAnimationState.EmoteFour); 
        }

    }
    private void Dodge()
    {
        if (!canMove || playerActions.isPoisoned || isLocked || !playerActions.IsAlive)
        return;

        // Dodge in the direction of movement
        if (isGrounded && !isJumping && canDodge)
        {
            // cancel all IdleAnimations
            CancelIdle();
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
        if(playerActions.isPoisoned || isLocked || !playerActions.IsAlive)
        {
            isJumping = false;
            yield break;
        }
        // Cancel all idle Animations
        CancelIdle();
        playerAnimation.SetAnimationState(PlayerAnimationState.Jump);
        yield return new WaitForSeconds(0.2f); // Small delay to sync with animation
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        yield return new WaitForSeconds(0.1f); // Allow some time before checking for grounded state

        yield return new WaitUntil(() => isGrounded );
        isJumping = false;
    }

    private IEnumerator DodgeCooldownCoroutine()
    {
        if(playerActions.isPoisoned || isLocked)
        {
            isDodging = false;
            yield break;
        }
        yield return new WaitForSeconds(0.8f); // Duration of dodge
        isDodging = false;
        Debug.Log(isDodging);
        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    /// <summary>
    /// Moves the player based on input or goes idle if no input is detected
    /// </summary>
    private void MovePlayer()
    {
        if (isDodging || !canMove || isLocked || !playerActions.IsAlive) 
            return; // Skip movement during dodge
        if (input != Vector3.zero)
        {
            // cancel all idle Animations
            CancelIdle();
            // Move the player
         //   Vector3 moveDirection = input * moveSpeed * Time.fixedDeltaTime;
           // transform.position += moveDirection;
           Vector3 moveDirection = transform.right * input.x + transform.forward * input.z;
            moveDirection *= moveSpeed * Time.deltaTime; // Time.fixedDeltaTime;
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
            if (isGrounded && !isJumping && !isDodging && !isIdleRoutineRunning)
            {
                if(playerAnimation.CurrentState == PlayerAnimationState.EmoteOne || playerAnimation.CurrentState == PlayerAnimationState.EmoteTwo
                    || playerAnimation.CurrentState == PlayerAnimationState.EmoteThree || playerAnimation.CurrentState == PlayerAnimationState.EmoteFour)
                    return;

                playerAnimation.SetAnimationState(PlayerAnimationState.Idle);
                isIdleRoutineRunning = true;
                idleCoroutine = StartCoroutine(IdleAnimationRoutine());
            }
        }
    }

    #region Idle/Emotes
    private IEnumerator IdleAnimationRoutine()
    {
        // wait a few seconds before playing random idle animation
        yield return new WaitForSeconds(Random.Range(5f,15f));

        // check if still idle
        if (playerAnimation.CurrentState != PlayerAnimationState.Idle)
        {
            isIdleRoutineRunning = false;
            yield break;
        }

        // Pick random idle animation
        PlayerAnimationState randomIdle = GetRandomIdle();
        playerAnimation.SetAnimationState(randomIdle);
    }
    private PlayerAnimationState GetRandomIdle()
    {
        return idleVarients[Random.Range(0,idleVarients.Length)];
    }

    /// <summary>
    /// animation clip event
    /// </summary>
    private void OnRandomIdleEnded()
    {
        playerAnimation.SetAnimationState(PlayerAnimationState.Idle);
        isIdleRoutineRunning = false;
    }
    private void CancelIdle()
    {
        if(idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }
        isIdleRoutineRunning = false;
    }

    private void HandleEmotes(PlayerAnimationState emote)
    {
        if(playerAnimation.CurrentState == PlayerAnimationState.Idle || playerAnimation.CurrentState == PlayerAnimationState.EmoteOne ||
           playerAnimation.CurrentState == PlayerAnimationState.EmoteTwo || playerAnimation.CurrentState == PlayerAnimationState.EmoteThree
           || playerAnimation.CurrentState == PlayerAnimationState.EmoteFour)
        {
             playerAnimation.SetAnimationState(emote);
        }
    }

    #endregion

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
        if(!isGrounded)
        {
            CancelIdle();
        }
    }

    /// <summary>
    /// Handles player rotation to face the mouse cursor
    /// </summary>
    private void RotatePlayer() 
    {
        if (!canMove)
            return;
        // Ensure mouse is available
        if (Mouse.current == null)
            return;
        if(playerAnimation.CurrentState == PlayerAnimationState.EmoteOne || playerAnimation.CurrentState == PlayerAnimationState.EmoteTwo
            || playerAnimation.CurrentState == PlayerAnimationState.EmoteThree || playerAnimation.CurrentState == PlayerAnimationState.EmoteFour)
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

    #region Camera Settings
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

        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            ResetCamera();
        }

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

    private void ResetCamera()
    {
        // Reset camera values to default
        cameraYaw = 0f;
        cameraAngle = 60f;
        scrollCamera = 3f;
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

    #endregion

    #region Status effects

    public void SlowStatus(float duration)
    {
        if(isSlowed || !playerActions.IsAlive)
        {
            Debug.Log(isSlowed + " already slowed; returning");
            return;
        }
        StartCoroutine(SlowedRoutine(duration));
    }

    private IEnumerator SlowedRoutine(float duration)
    {
        isSlowed = true;
        // active slowed status ui
        OnSlowedStatusChanged?.Invoke(true);
        Debug.Log("Slowed: " + isSlowed);

        // track slowed status duration 
        float timeElapsed = 0f;
        while (timeElapsed <= duration)
        {
            moveSpeed = playerStats.speed / 2;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        isSlowed = false;
        moveSpeed = playerStats.speed;
        // deactivate slowed status ui 
        OnSlowedStatusChanged?.Invoke(false);
        Debug.Log("Slowed: " + isSlowed);

    }

    public event System.Action<bool> OnSlowedStatusChanged;

    public void ConfusedStatus(float duration)
    {
        if(isConfused || !playerActions.IsAlive)
        {
            Debug.Log("Confused: " + isConfused);
            return;
        }
        StartCoroutine(ConfusedRoutine(duration));

    }

    private IEnumerator ConfusedRoutine(float duration)
    {
        isConfused = true;
        // activate confused status UI
        OnConfusedStatusChanged?.Invoke(true);
        Debug.Log("Confused: " + isConfused);

        // track duration of confused status
        float timeElapsed = 0f;
        while (timeElapsed <= duration)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        isConfused = false;
        // deactivate confused status UI
        OnConfusedStatusChanged?.Invoke(false);
        Debug.Log("confused: " + isConfused);
    }

    public event System.Action<bool> OnConfusedStatusChanged;

    public void ParalyzedStatus(float duration)
    {
        if(isParalyzed || !playerActions.IsAlive)
        {
            Debug.Log("Paralyzed: " + isParalyzed);
            return;
        }
        StartCoroutine(ParalyzedRoutine(duration));
    }

    private IEnumerator ParalyzedRoutine(float duration)
    {
        isParalyzed = true;
        // activate paralyzed status UI
        OnParalyzedStatusChanged?.Invoke(true);
        Debug.Log("Paralyzed: " + isParalyzed);

        // paralyzed ticks invervals 
        float tickInterval = 2f;
        float stunDuration = 1f; 

        // get more accurate paralyze duration 
        float endTime =  Time.time + duration;

        // instant first paralyze
        DeactivateControls();
        while (Time.time < endTime)
        {
            yield return new WaitForSeconds(stunDuration);
            ActivateControls();
            // activate para tick after tick Interval
            yield return new WaitForSeconds(tickInterval);

            DeactivateControls();
        }
        isParalyzed= false;
        ActivateControls();
        OnParalyzedStatusChanged?.Invoke(false);
    }

    public event System.Action<bool> OnParalyzedStatusChanged;

    #endregion

    #region  Deactivate / Activate Controls
    private void ActivateControls()
    {
        if(!playerActions.IsAlive)
            return;

        canMove = true;
        canJump = true;
        canDodge = true;
        playerActions.canAttack = true;
        isLocked = false;
    }

    private void DeactivateControls()
    {
        if(!playerActions.IsAlive)
            return ;

        canMove = false;
        canJump = false;
        canDodge = false;
        playerActions.canAttack = false;
        isLocked= true;
        playerAnimation.SetAnimationState(PlayerAnimationState.Idle);

    }
#endregion
}
