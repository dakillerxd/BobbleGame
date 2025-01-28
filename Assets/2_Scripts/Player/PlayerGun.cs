using System;
using TMPro;
using UnityEngine;
using VInspector;
using PrimeTween;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerGun : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private float shotForce = 15;
    [SerializeField] private float currentBubbleScale = 0.6f;
    [SerializeField] private float nextBubbleScale = 0.2f;
    [SerializeField] private float loadBubbleTime = 0.7f;
    [SerializeField] private float gunAnimationTime = 0.3f;

    
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
    [SerializeField] private float pulseEffectDuration = 0.5f;
    
    
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


    private PlayerMovement _playerMovement;
    private AudioSource _audioSource;
    private BubbleAmmo _currentBubble;
    private BubbleAmmo _nextBubble;
    private int _currentCombo;
    private float _comboTimeLeft;
    private bool _isComboActive;


    
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
        _playerMovement = GetComponent<PlayerMovement>();
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
        GameManager.OnScoreUpdate.AddListener(SetScoreText);
        GameManager.OnBubbleLeftUpdate.AddListener(SetBubbleText);
    }
    
    private void OnDisable()
    {
        GameManager.OnScoreUpdate.RemoveListener(SetScoreText);
        GameManager.OnBubbleLeftUpdate.RemoveListener(SetBubbleText);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) // Shooting
        {
            ShootBubble();
        }
        
        if (Input.GetKeyDown(KeyCode.Mouse1)) // Bubble pulse
        {
            TriggerPulseEffect();
        }

        UpdateCombo();
    }
    

    public void OnBubblesPopped(int amount)
    {
        if (amount >= 3)  // Only increment combo for 3+ bubble pops
        {
            IncrementCombo();
        }
    }

    
    
#region Shooting // -----------------------------------------------------------------------------------------------------------------

    private void ShootBubble()
    {
        if (!_currentBubble) return;
        BubbleBullet bubbleBullet = Instantiate(bubbleManager.BubbleBulletPrefab, bubbleSpawnPoint.position, Quaternion.identity);
        bubbleBullet.SetBubbleColor(_currentBubble.BubbleColor());
        bubbleBullet.ShootInDirection(_playerMovement.GetAimDirection(), shotForce, this);
        _gunShootSequence = GunShootAnimation();
        gunShotSfx?.Play(_audioSource);
        
        ClearCurrentBubble();
        SetCurrentBubble();
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
        // Clear all children of the current bubble transform
        _currentBubble = null;
        foreach (Transform child in currentBubbleTransform)
        {
            Destroy(child.gameObject);
        }
    }

    private void ClearNextBubble()
    {
        // Clear all children of the next bubble transform
        _nextBubble = null;
        foreach (Transform child in nextBubbleTransform)
        {
            Destroy(child.gameObject);
        }
    }
    
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
    
    private void TriggerPulseEffect()
    {
        
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
        
            // Get bubble spawn position (slightly in front of the gun)
            Vector3 pulsePosition = bubbleSpawnPoint.position;
        
            // Spawn pulse effect
            var pulseEffect = Instantiate(bubbleManager.BubbleEffectPrefab, pulsePosition, Quaternion.identity);
            pulseEffect.Initialize(pulseRadius, pulseEffectDuration);
        }
    
        // Animation
        Sequence.Create()
            .Group(Tween.PunchLocalPosition(gunTransform, strength: new Vector3(0, 0.1f, -1f), duration: gunAnimationTime, frequency: 1f))
            .Group(Tween.ShakeLocalPosition(gunTransform, strength: new Vector3(0, 0.1f, -1f), duration: gunAnimationTime, frequency: 1f));
    
        
        // Set the next bubble
        ResetCombo();
        SetCurrentBubble();
    }



#endregion Shooting // -----------------------------------------------------------------------------------------------------------------


#region GunUI // -----------------------------------------------------------------------------------------------------------------
    
    private void SetScoreText(int score)
    {
        if (!scoreText) return;
        
        _updateScoreSequence = Sequence.Create()
                .Group(Tween.PunchScale(scoreText.transform, strength: scoreText.transform.localScale * 1.5f, duration: 0.5f, frequency: 5f))
            // .Group(Tween.ShakeLocalPosition(scoreText.transform, strength: new Vector3(scoreText.transform.position.x, 0.02f, 0.02f), duration: 0.5f, frequency: 3f))
            ;
        scoreText.text = $"<sketchy>{score}</>";

    }

    private void SetBubbleText(int amount)
    {
        if (!bubblesText) return;
        
        
        _updateBubbleSequence = Sequence.Create()
                .Group(Tween.PunchScale(bubblesText.transform, strength: scoreText.transform.localScale * 1.5f, duration: 0.5f, frequency: 5f))
            // .Group(Tween.ShakeLocalPosition(bubblesText.transform, strength: new Vector3(scoreText.transform.position.x, 0.02f, 0.02f), duration: 0.5f, frequency: 3f))
            ;
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
    

#endregion GunUI // -----------------------------------------------------------------------------------------------------------------
    
    
#region Animations // -----------------------------------------------------------------------------------------------------------------
    
    private Sequence GunShootAnimation()
    {
        return Sequence.Create()
                .Group(Tween.PunchLocalPosition(gunTransform, strength: new Vector3(0, 0.1f, -1f), duration: gunAnimationTime, frequency: 1f))
            ;
    }
    
    private Sequence GunPulseAnimation()
    {
        return Sequence.Create()
                .Group(Tween.PunchLocalPosition(gunTransform, strength: new Vector3(0, 0.1f, -1f), duration: gunAnimationTime, frequency: 1f))
                .Group(Tween.ShakeLocalPosition(gunTransform, strength: new Vector3(0, 0.1f, -1f), duration: gunAnimationTime, frequency: 1f))
            ;
    }
    private Sequence LoadBubble()
    {
        
        return Sequence.Create()
                .Group(Tween.LocalPosition(currentBubbleTransform, startValue: _defaultNextBubbleTransformPosition, endValue: _defaultCurrentBubbleTransformPosition, duration: loadBubbleTime, Ease.OutBack))
                .Group(Tween.Scale(currentBubbleTransform, startValue: nextBubbleScale /2, endValue: currentBubbleScale, duration: loadBubbleTime * 2, Ease.OutBack))
            ;
    }
    
    private Sequence LoadNextBubble()
    {
        return Sequence.Create()
                .Group(Tween.Scale(nextBubbleTransform, startValue: 0.1f, endValue: nextBubbleScale, duration: loadBubbleTime, Ease.OutBack))
            ;
    }
    

#endregion Animations // -----------------------------------------------------------------------------------------------------------------
    
    

    
#if UNITY_EDITOR
    
    private void OnValidate()
    {
        if (currentBubbleTransform)
        {
            currentBubbleTransform.localScale = new Vector3(currentBubbleScale,currentBubbleScale,currentBubbleScale);
        }
        
        if (nextBubbleTransform)
        {
            nextBubbleTransform.localScale = new Vector3(nextBubbleScale,nextBubbleScale,nextBubbleScale);
        }
    }
#endif
}
