using System;
using TMPro;
using UnityEngine;
using VInspector;
using PrimeTween;



[RequireComponent(typeof(PlayerCamera))]
public class PlayerGun : MonoBehaviour
{
    [Header("Shooting Settings")] 
    [SerializeField] private float currentBubbleScale = 0.7f;
    [SerializeField] private float nextBubbleScale = 0.3f;
    [SerializeField] private float shotForce = 10;
    
    
    [Foldout("References")]
    [SerializeField] private SOInputReader inputReader;
    [SerializeField] private TextMeshPro scoreText;
    [SerializeField] private TextMeshPro bubblesText;
    [SerializeField] private Transform bubbleSpawnPoint;
    [SerializeField] private Transform currentBubbleTransform;
    [SerializeField] private Transform nextBubbleTransform;
    [SerializeField] private SOBubbleManager bubbleManager;
    [EndFoldout]


    private PlayerCamera _playerCamera;
    private BubbleAmmo _currentBubble;
    private BubbleAmmo _nextBubble;
    private Sequence _updateScoreSequence;
    private Sequence _updateBubbleSequence;
    
    
    private void Awake()
    {
        _playerCamera = GetComponent<PlayerCamera>();
        
        
        // Check transforms
        if (!bubbleSpawnPoint || !currentBubbleTransform || !nextBubbleTransform)
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
        
        ClearCurrentBubble();
        ClearNextBubble();
        SetNewNextBubble();
        SetCurrentBubble();
    }

    private void OnEnable()
    {
        SessionManager.OnScoreUpdate.AddListener(SetScoreText);
        SessionManager.OnBubbleLeftUpdate.AddListener(SetBubbleText);
    }
    
    private void OnDisable()
    {
        SessionManager.OnScoreUpdate.RemoveListener(SetScoreText);
        SessionManager.OnBubbleLeftUpdate.RemoveListener(SetBubbleText);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ShootBubble();
        }
    }

    private void ShootBubble()
    {
        if (!_currentBubble) return;
        BubbleBullet bubbleBullet = Instantiate(bubbleManager.BubbleBulletPrefab, bubbleSpawnPoint.position, Quaternion.identity);
        bubbleBullet.SetBubbleColor(_currentBubble.BubbleColor());
        bubbleBullet.ShootInDirection(_playerCamera.GetAimDirection(), shotForce);
        
        ClearCurrentBubble();
        SetCurrentBubble();
    }

    private void SetCurrentBubble()
    {
        _currentBubble = Instantiate(bubbleManager.BubbleAmmoPrefab, currentBubbleTransform.position, Quaternion.identity, currentBubbleTransform);
        _currentBubble.SetBubbleColor(_nextBubble.BubbleColor());
        
        
        ClearNextBubble();
        SetNewNextBubble();
    }

    private void SetNewNextBubble()
    {
        _nextBubble = Instantiate(bubbleManager.BubbleAmmoPrefab, nextBubbleTransform.position, Quaternion.identity, nextBubbleTransform);
        _nextBubble.SelectRandomBubbleColor();
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
