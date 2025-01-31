using System;
using TMPro;
using UnityEngine;
using VInspector;
using PrimeTween;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCamera))]
public class PlayerGun : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private float shotForce = 18;
    [SerializeField] private float currentBubbleScale = 0.5f;
    [SerializeField] private float nextBubbleScale = 0.2f;
    [SerializeField] private float loadBubbleTime = 0.7f;
    [SerializeField] private float gunAnimationTime = 0.3f;

    [Header("Action Buffering")]
    [SerializeField] private float shootBufferTime = 0.1f;
    [SerializeField] private float pulseBufferTime = 0.1f;
    [SerializeField] private float shootCooldown = 0.3f; // Minimum time between shots when holding
    [SerializeField] private float pulseCooldown = 0.3f; // Minimum time between pulses when holding
    
    [Header("Combo")] 
    [Tooltip("Time in seconds before the combo resets")]
    [SerializeField] private float comboTimeWindow = 7f;
    [Tooltip("Maximum combo multiplier that can be achieved")]
    [SerializeField] private int maxCombo = 10;
    [Tooltip("How quickly the combo bar visual depletes\n >1 make the bar drain more quickly/sharply\n <1 make the bar drain more smoothly/gradually\n 1 means the bar drains at the same rate as the timer")]
    [SerializeField] private float comboBarDrainSpeed = 1f;
    [SerializeField] private Color comboActiveColor = Color.green;
    [SerializeField] private Color comboInactiveColor = Color.gray;
    
    [Header("Bubble Pulse")] 
    [SerializeField] private float basePulseRadius = 3f;
    [SerializeField] private float radiusPerCombo = 0.5f;
    [SerializeField] private float pulseEffectDuration = 1f;
    
    [Foldout("References")]
    [SerializeField] private SOInputReader inputReader;
    [SerializeField] private TextMeshPro scoreText;
    [SerializeField] private TextMeshPro bubblesText;
    [SerializeField] private TextMeshPro comboText;
    [SerializeField] private Image comboBar;
    [SerializeField] private Transform gunTransform;
    [SerializeField] private Transform bubbleSpawnPoint;
    [SerializeField] private Transform currentBubbleTransform;
    [SerializeField] private Transform nextBubbleTransform;
    [SerializeField] private SOBubbleManager bubbleManager;
    [SerializeField] private SOAudioEvent gunShotSfx;
    [EndFoldout]
    
    private PlayerCamera _playerCamera;
    private AudioSource _audioSource;
    private BubbleAmmo _currentBubble;
    private BubbleAmmo _nextBubble;
    private int _currentCombo;
    private float _comboTimeLeft;
    private bool _isComboActive;

    // Input state and buffer variables
    private float _shootBufferCounter;
    private float _pulseBufferCounter;
    private float _shootCooldownCounter;
    private float _pulseCooldownCounter;
    private bool _isShootHeld;
    private bool _isPulseHeld;

    // Animations
    private Sequence _updateScoreSequence;
    private Sequence _updateBubbleSequence;
    private Sequence _loadBubbleSequence;
    private Sequence _loadNextBubbleSequence;
    private Sequence _gunShootSequence;
    private Sequence _updateComboSequence;
    private Vector3 _defaultCurrentBubbleTransformPosition;
    private Vector3 _defaultNextBubbleTransformPosition;
    private Vector3 _defaultGunTransformPosition;

    private void Awake()
    {
        _playerCamera = GetComponent<PlayerCamera>();
        _audioSource = GetComponent<AudioSource>();
        
        // Check transforms
        if (!bubbleSpawnPoint || !currentBubbleTransform || !nextBubbleTransform || !gunTransform)
        {
            Debug.LogError("Missing Transforms!");
            return;
        }
        
        // Check prefabs
        if (!bubbleManager)
        {
            Debug.LogError("Missing bubbleManager!");
            return;
        }
        
        // Check score text
        if (!scoreText)
        {
            Debug.LogError("Missing scoreText!");
            return;
        }
        
        _defaultCurrentBubbleTransformPosition = currentBubbleTransform.localPosition;
        _defaultNextBubbleTransformPosition = nextBubbleTransform.localPosition;
        _defaultGunTransformPosition = gunTransform.localPosition;
        
        ClearCurrentBubble();
        ClearNextBubble();
        SetNewNextBubble();
        SetCurrentBubble();
    }

    private void OnEnable()
    {
        inputReader.ShootEvent += GetShootInput;
        inputReader.ShootSecondEvent += GetBubblePulseInput;
        
        GameManager.OnScoreUpdate.AddListener(SetScoreText);
        GameManager.OnBubbleLeftUpdate.AddListener(SetBubbleText);
    }

    private void OnDisable()
    {
        inputReader.ShootEvent -= GetShootInput;
        inputReader.ShootSecondEvent -= GetBubblePulseInput;
        GameManager.OnScoreUpdate.RemoveListener(SetScoreText);
        GameManager.OnBubbleLeftUpdate.RemoveListener(SetBubbleText);
    }

    private void Update()
    {
        UpdateBufferTimers();
        HandleShooting();
        HandleBubblePulse();
        UpdateCombo();
    }

    private void UpdateBufferTimers()
    {
        // Update shoot buffer
        if (_shootBufferCounter > 0f)
        {
            _shootBufferCounter -= Time.deltaTime;
        }

        // Update pulse buffer
        if (_pulseBufferCounter > 0f)
        {
            _pulseBufferCounter -= Time.deltaTime;
        }

        // Update shoot cooldown
        if (_shootCooldownCounter > 0f)
        {
            _shootCooldownCounter -= Time.deltaTime;
        }

        // Update pulse cooldown
        if (_pulseCooldownCounter > 0f)
        {
            _pulseCooldownCounter -= Time.deltaTime;
        }
    }

    public void OnBubblesPopped(int amount)
    {
        if (amount >= 3)  // Only increment combo for 3+ bubble pops
        {
            IncrementCombo();
        }
    }

    #region Input

    private void GetBubblePulseInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            _pulseBufferCounter = pulseBufferTime;
        }
        _isPulseHeld = context.phase == InputActionPhase.Started || context.phase == InputActionPhase.Performed;
    }

    private void GetShootInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            _shootBufferCounter = shootBufferTime;
        }
        _isShootHeld = context.phase == InputActionPhase.Started || context.phase == InputActionPhase.Performed;
    }

    #endregion

    #region Shooting

    private void HandleShooting()
    {
        if (!_currentBubble) return;

        bool canShoot = (_shootBufferCounter > 0f || _isShootHeld) && _shootCooldownCounter <= 0f;
        
        if (canShoot)
        {
            BubbleBullet bubbleBullet = Instantiate(bubbleManager.BubbleBulletPrefab, bubbleSpawnPoint.position, Quaternion.identity);
            bubbleBullet.SetBubbleColor(_currentBubble.BubbleColor());
            bubbleBullet.ShootInDirection(_playerCamera.GetAimDirection(), shotForce, this);
            _gunShootSequence = GunShootAnimation();
            gunShotSfx?.Play(_audioSource);

            // Reset timers
            _shootBufferCounter = 0f;
            _shootCooldownCounter = shootCooldown;

            ClearCurrentBubble();
            SetCurrentBubble();
        }
    }

    private void SetCurrentBubble()
    {
        _currentBubble = Instantiate(bubbleManager.BubbleAmmoPrefab, currentBubbleTransform.position, Quaternion.identity, currentBubbleTransform);
        _currentBubble.SetBubbleColor(_nextBubble.BubbleColor());
        _loadBubbleSequence = LoadBubble();
        
        ClearNextBubble();
        SetNewNextBubble();
    }
    
    public void ForceCurrentBubble(Material bubbleColor)
    {
        ClearCurrentBubble();
        _currentBubble = Instantiate(bubbleManager.BubbleAmmoPrefab, currentBubbleTransform.position, Quaternion.identity, currentBubbleTransform);
        _currentBubble.SetBubbleColor(bubbleColor);
    }

    private void SetNewNextBubble()
    {
        _nextBubble = Instantiate(bubbleManager.BubbleAmmoPrefab, nextBubbleTransform.position, Quaternion.identity, nextBubbleTransform);
        _nextBubble.SelectRandomBubbleColor();
        _loadNextBubbleSequence = LoadNextBubble();
    }

    private void ClearCurrentBubble()
    {
        _currentBubble = null;
        foreach (Transform child in currentBubbleTransform)
        {
            Destroy(child.gameObject);
        }
    }

    private void ClearNextBubble()
    {
        _nextBubble = null;
        foreach (Transform child in nextBubbleTransform)
        {
            Destroy(child.gameObject);
        }
    }

    private void HandleBubblePulse()
    {
        bool canPulse = (_pulseBufferCounter > 0f || _isPulseHeld) && _pulseCooldownCounter <= 0f;
        
        if (canPulse)
        {
            // Reset pulse buffer and set cooldown
            _pulseBufferCounter = 0f;
            _pulseCooldownCounter = pulseCooldown;

            // Pop current bubble
            _currentBubble?.PopBubble();

            if (_currentCombo == 1)
            {
                // At combo x1, just pop the next bubble too
                _nextBubble?.PopBubble();
            }
            else if (_currentCombo >= 2)
            {
                // Calculate pulse radius based on combo
                float pulseRadius = basePulseRadius + (_currentCombo * radiusPerCombo);
                
                // Get bubble spawn position
                Vector3 pulsePosition = bubbleSpawnPoint.position;
                
                // Spawn pulse effect
                var pulseEffect = Instantiate(bubbleManager.BubbleEffectPrefab, pulsePosition, Quaternion.identity);
                pulseEffect.Initialize(pulseRadius, pulseEffectDuration);
            }

            // Animation
            Sequence.Create()
                .Group(Tween.PunchLocalPosition(gunTransform, strength: new Vector3(0, 0.1f, -1f), duration: gunAnimationTime, frequency: 1f))
                .Group(Tween.ShakeLocalPosition(gunTransform, strength: new Vector3(0, 0.1f, -1f), duration: gunAnimationTime, frequency: 1f));

            // Reset combo and set next bubble
            ResetCombo();
            SetCurrentBubble();
        }
    }

    #endregion

    #region Combo

    private void UpdateCombo()
    {
        if (!_isComboActive) return;
    
        _comboTimeLeft -= Time.deltaTime;
        float normalizedTime = (_comboTimeLeft / comboTimeWindow);
        // Apply the drain speed modifier
        comboBar.fillAmount = Mathf.Lerp(comboBar.fillAmount, normalizedTime, Time.deltaTime * comboBarDrainSpeed);
    
        if (_comboTimeLeft <= 0)
        {
            ResetCombo();
        }
    }

    private void IncrementCombo()
    {
        _currentCombo = Mathf.Min(_currentCombo + 1, maxCombo);
        _comboTimeLeft = comboTimeWindow;
        _isComboActive = true;
    
        UpdateComboUI();
    }

    private void ResetCombo()
    {
        _currentCombo = 0;
        _comboTimeLeft = 0;
        _isComboActive = false;
        comboBar.fillAmount = 0;
    
        UpdateComboUI();
    }

    #endregion

    #region UI

    private void SetScoreText(int score)
    {
        if (!scoreText) return;
        
        _updateScoreSequence = Sequence.Create()
            .Group(Tween.PunchScale(scoreText.transform, strength: scoreText.transform.localScale * 1.5f, duration: 0.5f, frequency: 5f));
        scoreText.text = $"<sketchy>{score}</>";
    }

    private void SetBubbleText(int amount)
    {
        if (!bubblesText) return;
        
        _updateBubbleSequence = Sequence.Create()
            .Group(Tween.PunchScale(bubblesText.transform, strength: scoreText.transform.localScale * 1.5f, duration: 0.5f, frequency: 5f));
        bubblesText.text = $"<sketchy>{amount}</>";
    }
    
    private void UpdateComboUI()
    {
        if (!comboText || !comboBar) return;

        comboText.text = $"<sketchy>{_currentCombo}x</>";
        comboBar.color = _isComboActive ? comboActiveColor : comboInactiveColor;

        _updateComboSequence = Sequence.Create()
            .Group(Tween.PunchScale(comboText.transform, strength: comboText.transform.localScale * 1.5f, duration: 0.5f, frequency: 5f));
    }

    #endregion

    #region Animations
    
    private Sequence GunShootAnimation()
    {
        return Sequence.Create()
            .Group(Tween.PunchLocalPosition(gunTransform, strength: new Vector3(0, 0.1f, -1f), duration: gunAnimationTime, frequency: 1f));
    }
    
    private Sequence LoadBubble()
    {
        currentBubbleTransform.localScale = new Vector3(nextBubbleScale / 3, nextBubbleScale / 3, nextBubbleScale / 3);
        
        return Sequence.Create()
            .Group(Tween.LocalPosition(currentBubbleTransform, startValue: _defaultNextBubbleTransformPosition + new Vector3(0,-0.2f,0), endValue: _defaultCurrentBubbleTransformPosition, duration: loadBubbleTime, Ease.OutBack))
            .Group(Tween.Scale(currentBubbleTransform, startValue: nextBubbleScale /4, endValue: currentBubbleScale, duration: loadBubbleTime * 3, Ease.OutBack));
    }
    
    private Sequence LoadNextBubble()
    {
        return Sequence.Create()
            .Group(Tween.Scale(nextBubbleTransform, startValue: 0.1f, endValue: nextBubbleScale, duration: loadBubbleTime * 4, Ease.OutBack));
    }

    #endregion Animations

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (currentBubbleTransform)
        {
            currentBubbleTransform.localScale = new Vector3(currentBubbleScale, currentBubbleScale, currentBubbleScale);
        }
        
        if (nextBubbleTransform)
        {
            nextBubbleTransform.localScale = new Vector3(nextBubbleScale, nextBubbleScale, nextBubbleScale);
        }
    }
#endif
}