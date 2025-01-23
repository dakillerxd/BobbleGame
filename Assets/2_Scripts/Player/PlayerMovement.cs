using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerCamera))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;
    
    
    private CharacterController _controller;
    private PlayerCamera _playerCamera;
    private Vector3 _velocity;
    public bool isGrounded {get; private set;}
    public bool isRunning {get; private set;}
    public bool isJumping {get; private set;}
    public bool isFalling {get; private set;}

    
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerCamera = GetComponent<PlayerCamera>();
        
        if (!_playerCamera || !_playerCamera)
        {
            Debug.LogError("Missing components");
            return;
        }
    }
    
    private void Update()
    {
        HandleMovement();
        HandleJumping();
        HandleGravity();
    }
    
    private void HandleMovement()
    {
        // Check input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
    
        // Get the camera direction
        Vector3 cameraForward = _playerCamera.GetMovementDirection();
        Vector3 cameraRight = Quaternion.Euler(0, 90, 0) * cameraForward;
    
        // Calculate move direction relative to camera
        Vector3 moveDir = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;
    
        // Check if running
        float targetMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        isRunning = Input.GetKey(KeyCode.LeftShift) && isGrounded;
    
        // Move
        _controller.Move(moveDir * (targetMoveSpeed * Time.deltaTime));
    }

    private void HandleJumping()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
        
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
        isJumping = _velocity.y > 0;
    }
    
    private void HandleGravity()
    {
        // Gravity handling remains the same
        isGrounded = _controller.isGrounded;
        isFalling = _velocity.y < 0;
        
        if (isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }
        

    }
}