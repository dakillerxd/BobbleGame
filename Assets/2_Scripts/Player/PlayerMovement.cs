using System;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Serialization;
using VInspector;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 9f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -15f; // -9.81f;

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
    [EndFoldout]
    
    
    private CharacterController _controller;
    private CinemachinePanTilt _panTilt;
    private Vector3 _velocity;
    public bool isGrounded {get; private set;}
    public bool isRunning {get; private set;}
    public bool isJumping {get; private set;}
    public bool isFalling {get; private set;}

    
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        SetupCamera();
    }
    
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
    
    private void Update()
    {
        HandleMovement();
        HandleJumping();
        HandleGravity();
        UpdateFov();
    }

#region Movement //---------------------------------------------------------------------------------------

    private void HandleMovement()
    {
        // Check input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
    
        // Get the camera direction
        Vector3 cameraForward = GetMovementDirection();
        Vector3 cameraRight = Quaternion.Euler(0, 90, 0) * cameraForward;
    
        // Calculate move direction relative to camera
        Vector3 moveDir = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;
    
        // Check if running
        float targetMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        isRunning = Input.GetKey(KeyCode.LeftShift);
    
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
    
    public void SetVelocity(Vector3 newVelocity)
    {
        _velocity = newVelocity;
    }

    public float Gravity()
    {
        return gravity;
    }

#endregion Movement //---------------------------------------------------------------------------------------
    


#region Camera //---------------------------------------------------------------------------------------

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
        float targetFov = isRunning ? baseFov * runFovMultiplier : baseFov;
        
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