using System;
using UnityEngine;
using VInspector;


[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerGun))]
public class PlayerPowerUps : MonoBehaviour
{
    
    [Header("SpeedBoost")]
    [SerializeField] private float speedBoostMultiplier = 1.5f;
    private float _speedBoostTimer;
    

    [Header("SuperBubble")]
    private  float _superBubbleTimer;
    
    
    
    [field: SerializeField] [ReadOnly] public bool isSpeedBoosted { get; private set; } = false;
    [field: SerializeField] [ReadOnly] public bool isSuperBubble { get; private set; } = false;
    private PlayerMovement _playerMovement;
    private PlayerGun _playerGun;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerGun = GetComponent<PlayerGun>();
    }

    private void Update()
    {
        if (_speedBoostTimer > 0)
        {
            _speedBoostTimer -= Time.deltaTime;
        }
        if (_speedBoostTimer == 0) isSpeedBoosted = false;
        

        if (_superBubbleTimer > 0)
        {
            _superBubbleTimer -= Time.deltaTime;
        }
        if (_superBubbleTimer == 0) isSuperBubble = false;
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.TryGetComponent(out PowerUpPad powerPad))
        {
            ReceivePowerUp(powerPad.GetPowerUp());
            return;
        } 
    }
    
    
    private void ReceivePowerUp(SOPowerUp powerUp)
    {
        switch (powerUp.name)
        {
            case "SpeedBoost":
                isSpeedBoosted = true;
                _speedBoostTimer = powerUp.duration;

                break;

            case "SuperBubble":
                isSuperBubble = true;
                _superBubbleTimer = powerUp.duration;

                break;
            
            default:
                Debug.LogError("PowerUp not found!");
                break;
        }
    }
}
