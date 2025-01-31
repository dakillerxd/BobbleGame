using System;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using VInspector;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCamera : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] [Range(0,0.1f)] private float lookSensitivity = 0.05f;
    [SerializeField] [Range(0,0.1f)] private float lookSmoothing = 0.05f;
    [SerializeField] private Vector2 verticalAxisRange = new Vector2(-90, 90);
    [SerializeField] private float baseFov = 60f;
    [SerializeField] private float runFovMultiplier = 1.3f;
    [SerializeField] private float dashFovMultiplier = 1.5f;
    [SerializeField] private bool invertHorizontal = false;
    [SerializeField] private bool invertVertical = false;
    
    [Foldout("References")] 
    [SerializeField] private Transform playerHead;
    [SerializeField] private SOInputReader inputReader;
    [SerializeField] private CinemachineCamera virtualCamera;
    [EndFoldout]
    
    private PlayerMovement _playerMovement;
    private float _currentPanAngle;
    private float _currentTiltAngle;
    private float _targetPanAngle;
    private float _targetTiltAngle;
    private Vector2 _rotationVelocity;
    
    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        if (!virtualCamera) virtualCamera = GetComponentInChildren<CinemachineCamera>();
        if (!virtualCamera)
        {
            Debug.LogError("Virtual Camera not found!");
            return;
        }
        
        
        // Initialize target angles
        _targetPanAngle = _currentPanAngle;
        _targetTiltAngle = _currentTiltAngle;
        SetupFov();
        SetupCursor();
    }
    
    private void OnEnable()
    {
        inputReader.LookEvent += OnLook;
    }
    
    private void OnDisable()
    {
        inputReader.LookEvent -= OnLook;
    }
    
    private void Update()
    {
        UpdateFov();
        UpdateHeadRotation();
    }
    
    private void OnLook(InputAction.CallbackContext context)
    {
        if (!playerHead) return;
        
        Vector2 lookDelta = context.ReadValue<Vector2>();

        // Apply inversion if enabled
        float horizontalInput = invertHorizontal ? -lookDelta.x : lookDelta.x;
        float verticalInput = invertVertical ? lookDelta.y : -lookDelta.y;
        
        // Update target angles
        _targetPanAngle += horizontalInput * lookSensitivity;
        _targetTiltAngle += verticalInput * lookSensitivity;
        
        // Only clamp tilt angle, allowing full pan rotation
        _targetTiltAngle = Mathf.Clamp(_targetTiltAngle, verticalAxisRange.x, verticalAxisRange.y);

        if (lookSmoothing <= 0) 
        {
            _currentPanAngle = _targetPanAngle;
            _currentTiltAngle = _targetTiltAngle;
        }
    }
    
    private void UpdateHeadRotation()
    {
        if (!playerHead) return;

        if (lookSmoothing > 0)
        {
            // Smoothly interpolate current angles towards target angles
            _currentPanAngle = Mathf.SmoothDampAngle(_currentPanAngle, _targetPanAngle, ref _rotationVelocity.x, lookSmoothing);
            _currentTiltAngle = Mathf.SmoothDamp(_currentTiltAngle, _targetTiltAngle, ref _rotationVelocity.y, lookSmoothing);
        }
        
        // Update player body rotation (Y-axis only)
        transform.rotation = Quaternion.Euler(0, _currentPanAngle, 0);
        
        // Update head rotation (X-axis for looking up/down)
        playerHead.localRotation = Quaternion.Euler(_currentTiltAngle, 0, 0);
    }
    
    private void UpdateFov()
    {
        if (!virtualCamera) return;
        
        float targetFov = baseFov;
        if (_playerMovement.isDashing)
        {
            targetFov *= dashFovMultiplier;
        }
        else if (_playerMovement.isRunning)
        {
            targetFov *= runFovMultiplier;
        }
        
        virtualCamera.Lens.FieldOfView = Mathf.Lerp(virtualCamera.Lens.FieldOfView, targetFov, Time.deltaTime * 10f);
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
    
    public Vector3 GetMovementDirection()
    {
        Vector3 direction = Quaternion.Euler(0, _currentPanAngle, 0) * Vector3.forward;
        return direction.normalized;
    }

    public Vector3 GetAimDirection()
    {
        return Quaternion.Euler(_currentTiltAngle, _currentPanAngle, 0) * Vector3.forward;
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        SetupFov();
    }
#endif
}