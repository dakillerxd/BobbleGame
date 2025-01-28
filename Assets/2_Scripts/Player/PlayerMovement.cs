using System;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using VInspector;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PlayerPowerUps))]
[RequireComponent(typeof(PlayerGun))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 9f;
    [SerializeField] private bool canRun = false;
    [ShowIf("canRun")][SerializeField] private float runSpeed = 11f;[EndIf]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float coyoteTime = 0.1f;
    
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 50f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private AnimationCurve dashSpeedCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Camera Settings")]
    [Tooltip("The range of the horizontal axis.")]
    [SerializeField] private Vector2 panAxisRange = new Vector2(-180, 180);
    [Tooltip("The range of the vertical axis.")]
    [SerializeField] private Vector2 tiltAxisRange = new Vector2(-70, 70);
    [SerializeField] private float baseFov = 60f;
    [SerializeField] private float runFovMultiplier = 1.3f;

    [Foldout("References")] 
    [SerializeField] private SOInputReader inputReader;
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private SOAudioEvent deathSfx;
    [EndFoldout]
    
    private AudioSource _audioSource;
    private CharacterController _controller;
    private CinemachinePanTilt _panTilt;
    private Vector3 _velocity;
    private Vector3 _spawnPoint;
    private float _dashTimeRemaining;
    private float _dashCooldownRemaining;
    private Vector3 _dashDirection;
    private float _jumpBufferCounter;
    private float _coyoteTimeCounter;
    
    // Input state variables
    private Vector2 _moveInput;
    private bool _runInput;
    private bool _wantsToDash;
    
    // Public state properties
    public bool isGrounded { get; private set; }
    public bool isRunning { get; private set; }
    public bool isJumping { get; private set; }
    public bool isFalling { get; private set; }
    public bool isDashing { get; private set; }
    
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        _spawnPoint = transform.position;
        SetupCamera();
    }
    
    private void OnEnable()
    {
        inputReader.MoveEvent += GetMovementInput;
        inputReader.RunEvent += GetRunningInput;
        inputReader.JumpEvent += GetJumpInput;
        inputReader.DashEvent += GetDashInput;
        GameManager.OnGameStateChanged.AddListener(MoveToSpawnPoint);
    }

    private void OnDisable()
    {
        inputReader.MoveEvent -= GetMovementInput;
        inputReader.RunEvent -= GetRunningInput;
        inputReader.JumpEvent -= GetJumpInput;
        inputReader.DashEvent -= GetDashInput;
        GameManager.OnGameStateChanged.RemoveListener(MoveToSpawnPoint);
    }
    
    private void Update()
    {
        HandleMovement();
        HandleJump();
        HandleGravity();
        HandleDashing();
        UpdateFov();
    }
    
    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Water"))
        {
            MoveToSpawnPoint();
            GameManager.Instance?.PlayerDied();
            deathSfx?.Play(_audioSource);
            return;
        }
        
        if (other.TryGetComponent(out BubbleObject bubbleObject))
        {
            if (isDashing)
            {
                bubbleObject.PopBubble();
            }
        }
    }

#region Input // ---------------------------------------------------------------------------------------

    private void GetMovementInput(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }
        
    private void GetRunningInput(InputAction.CallbackContext context)
    {
        _runInput = context.phase == InputActionPhase.Started || context.phase == InputActionPhase.Performed;
    }
        
    private void GetJumpInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            _jumpBufferCounter = jumpBufferTime;
        }
    }
    
    private void GetDashInput(InputAction.CallbackContext context)
    {
        _wantsToDash = context.phase == InputActionPhase.Performed;
    }

#endregion Input // ---------------------------------------------------------------------------------------
    
    
#region Movement //---------------------------------------------------------------------------------------

    private void HandleMovement()
    {
        if (isDashing) return;
    
        // Get the camera direction
        Vector3 cameraForward = GetMovementDirection();
        Vector3 cameraRight = Quaternion.Euler(0, 90, 0) * cameraForward;
    
        // Calculate move direction relative to camera
        Vector3 moveDir = (cameraForward * _moveInput.y + cameraRight * _moveInput.x).normalized;
    
        // Apply movement speed
        isRunning = _runInput && canRun;
        float targetMoveSpeed = isRunning ? runSpeed : walkSpeed;
        
    
        // Move
        _controller.Move(moveDir * (targetMoveSpeed * Time.deltaTime));
    }
    
    private void HandleJump()
    {
        if (isDashing) return;

        // Update jump buffer timer
        if (_jumpBufferCounter > 0f)
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
    
        // Handle jump if we have buffered input and either have coyote time or are grounded
        if (_jumpBufferCounter > 0f && (_coyoteTimeCounter > 0f || isGrounded))
        {
            _velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            _jumpBufferCounter = 0f;  // Reset jump buffer since we used it
            _coyoteTimeCounter = 0f;  // Reset coyote time since we used it
        }
    
        // Apply gravity
        _velocity.y += gravity * Time.deltaTime;
    
        // Move the character
        _controller.Move(_velocity * Time.deltaTime);
    
        // Update jumping state
        isJumping = _velocity.y > 0;
    }
    
    private void HandleDashing()
    {
        // Update cooldown
        if (_dashCooldownRemaining > 0)
        {
            _dashCooldownRemaining -= Time.deltaTime;
        }

        // Start new dash
        if (_wantsToDash && _dashCooldownRemaining <= 0 && !isDashing)
        {
            StartDash();
        }

        // Handle ongoing dash
        if (isDashing)
        {
            UpdateDash();
        }
    }
    
    private void StartDash()
    {
        isDashing = true;
        _dashTimeRemaining = dashDuration;
        _dashCooldownRemaining = dashCooldown;

        // Use movement input if available, otherwise use forward direction
        if (_moveInput.magnitude > 0.1f)
        {
            _dashDirection = (GetMovementDirection() * _moveInput.y + 
                            Quaternion.Euler(0, 90, 0) * GetMovementDirection() * _moveInput.x).normalized;
        }
        else
        {
            _dashDirection = GetMovementDirection();
        }

        _velocity.y = 0; // Zero out vertical velocity for a clean dash
    }

    private void UpdateDash()
    {
        if (_dashTimeRemaining > 0)
        {
            // Calculate dash progress (0 to 1)
            float dashProgress = 1 - (_dashTimeRemaining / dashDuration);
            
            // Use animation curve to control dash speed over time
            float currentDashSpeed = dashSpeed * dashSpeedCurve.Evaluate(dashProgress);
            
            // Move the character
            _controller.Move(_dashDirection * (currentDashSpeed * Time.deltaTime));
            
            _dashTimeRemaining -= Time.deltaTime;
        }
        else
        {
            isDashing = false;
        }
    }
    
    private void HandleGravity()
    {
        if (isDashing) return;

        // Store previous grounded state to detect when we leave the ground
        bool wasGrounded = isGrounded;
    
        // Update current grounded state
        isGrounded = _controller.isGrounded;
        isFalling = _velocity.y < 0;

        // Handle coyote time
        if (isGrounded)
        {
            // Reset coyote time when we're grounded
            _coyoteTimeCounter = coyoteTime;
        
            // Apply a small downward force when grounded to keep us grounded
            if (_velocity.y < 0)
            {
                _velocity.y = -2f;
            }
        }
        else if (wasGrounded)
        {
            // Start coyote time when we just left the ground
            _coyoteTimeCounter = coyoteTime;
        }
        else
        {
            // Count down coyote time when in the air
            _coyoteTimeCounter -= Time.deltaTime;
        }
    }
    
    public void SetVelocity(Vector3 newVelocity)
    {
        _velocity = newVelocity;
    }

    public float Gravity()
    {
        return gravity;
    }
    
    private void MoveToSpawnPoint()
    {
        transform.position = _spawnPoint;
    }

#endregion Movement //---------------------------------------------------------------------------------------

#region Camera //---------------------------------------------------------------------------------------

    private void SetupCamera()
    {
        if (!virtualCamera) virtualCamera = GetComponentInChildren<CinemachineCamera>();
        if (!virtualCamera)
        {
            Debug.LogError("Virtual Camera not found!");
            return;
        }
            
        _panTilt = virtualCamera.GetComponent<CinemachinePanTilt>();
        if (!_panTilt)
        {
            _panTilt = virtualCamera.gameObject.AddComponent<CinemachinePanTilt>();
        }
            
        SetupFov();
        SetupCameraRange();
        SetupCursor();
    }

    private void SetupCameraRange()
    {
        if (!_panTilt) return;
        
        _panTilt.PanAxis.Range = panAxisRange;
        _panTilt.TiltAxis.Range = tiltAxisRange;
    }
    
    private void SetupCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetupFov()
    {
        if (!virtualCamera) return;
        virtualCamera.Lens.FieldOfView = baseFov;
    }
    
    private void UpdateFov()
    {
        if (!virtualCamera) return;
        float targetFov = isRunning || isDashing ? baseFov * runFovMultiplier : baseFov;
        
        virtualCamera.Lens.FieldOfView = Mathf.Lerp(virtualCamera.Lens.FieldOfView, targetFov, Time.deltaTime * 10f);
    }
    
    public Vector3 GetMovementDirection()
    {
        if (!_panTilt) return Vector3.zero;
        Vector3 direction = Quaternion.Euler(0, _panTilt.PanAxis.Value, 0) * Vector3.forward;
        return direction.normalized;
    }

    public Vector3 GetAimDirection()
    {
        if (!_panTilt) return Vector3.zero;
        return Quaternion.Euler(_panTilt.TiltAxis.Value, _panTilt.PanAxis.Value, 0) * Vector3.forward;
    }

#endregion Camera //---------------------------------------------------------------------------------------

#if UNITY_EDITOR
    private void OnValidate()
    {
        SetupFov();
        SetupCameraRange();
    }
#endif
}