using System;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Serialization;
using VInspector;


[RequireComponent(typeof(PlayerMovement))]
public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("The range of the horizontal axis.")]
    [SerializeField] private Vector2 panAxisRange = new Vector2(-180, 180);
    [Tooltip("The range of the vertical axis.")]
    [SerializeField] private Vector2 tiltAxisRange = new Vector2(-70, 70);
    [SerializeField] private float baseFov = 60f;
    [SerializeField] private float runFovMultiplier = 1.2f;


    [Foldout("References")]
    [SerializeField] private SOInputReader inputReader;
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachinePanTilt panTilt;
    [SerializeField] private PlayerMovement playerMovement;
    [EndFoldout]
    
    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (!playerMovement)
        {
            Debug.LogError("PlayerMovement component not found!");
            return;
        }
        
        // Setup cinemachine
        virtualCamera = GetComponentInChildren<CinemachineCamera>();
        if (!virtualCamera)
        {
            Debug.LogError("Virtual Camera not found!");
            return;
        }
        
        // Get or add the PanTilt component
        panTilt = virtualCamera.GetComponent<CinemachinePanTilt>();
        if (!panTilt)
        {
            panTilt = virtualCamera.gameObject.AddComponent<CinemachinePanTilt>();
        }
        
        
        SetupFov();
        SetupCameraRange();
        SetupCursor();
    }

    private void Update()
    {
        UpdateFov();
    }


    private void SetupCameraRange()
    {

        if (!panTilt) return;
        
        panTilt.PanAxis.Range = panAxisRange;
        panTilt.TiltAxis.Range = tiltAxisRange;
        
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
        if (!virtualCamera || !playerMovement) return;
        float targetFov = playerMovement.isRunning ? baseFov * runFovMultiplier : baseFov;
        
        virtualCamera.Lens.FieldOfView = Mathf.Lerp(virtualCamera.Lens.FieldOfView, targetFov, Time.deltaTime * 10f);
    }
    
    public Vector3 GetMovementDirection()
    {
        if (!panTilt) return Vector3.zero;
        Vector3 direction = Quaternion.Euler(0, panTilt.PanAxis.Value, 0) * Vector3.forward;
        return direction.normalized;
    }


    public Vector3 GetAimDirection()
    {
        if (!panTilt) return Vector3.zero;
        return Quaternion.Euler(panTilt.TiltAxis.Value, panTilt.PanAxis.Value, 0) * Vector3.forward;
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        SetupFov();
        SetupCameraRange();
    }
#endif

    
}