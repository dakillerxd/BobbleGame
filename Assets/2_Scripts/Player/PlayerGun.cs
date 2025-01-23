
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using VInspector;

[RequireComponent(typeof(PlayerCamera))]
public class PlayerGun : MonoBehaviour
{
    [Header("Shooting Settings")] 
    [SerializeField] private float shotForce = 10;
    
    
    [Foldout("References")]
    [SerializeField] private SOInputReader inputReader;
    [SerializeField] private TextMeshPro scoreText;
    [SerializeField] private Transform bubbleSpawnPoint;
    [SerializeField] private Transform currentBubbleTransform;
    [SerializeField] private Transform nextBubbleTransform;
    [SerializeField] private SOBubbleManager bubbleManager;
    [EndFoldout]


    private PlayerCamera _playerCamera;
    private BubbleAmmo _currentBubble;
    private BubbleAmmo _nextBubble;
    
    
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ShootBubble();
        }
        
        
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SceneManager.SetActiveScene(SceneManager.GetActiveScene());
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
        BubbleAmmo bubbleAmmo = Instantiate(bubbleManager.BubbleAmmoPrefab, currentBubbleTransform.position, Quaternion.identity, currentBubbleTransform);
        bubbleAmmo.SetBubbleColor(_nextBubble.BubbleColor());
        _currentBubble = bubbleAmmo;
        
        
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


    private void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }
    

}
