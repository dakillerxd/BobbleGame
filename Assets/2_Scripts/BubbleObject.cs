using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VInspector;
using PrimeTween;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class BubbleObject : BubbleBase
{
    [SerializeField] [ReadOnly] private bool wasShot = false;
    [SerializeField] [ReadOnly] private List<BubbleObject> touchingBubbles = new List<BubbleObject>();
    [SerializeField] [ReadOnly] private List<BubbleObject> touchingSameColorBubbles = new List<BubbleObject>();
    private Rigidbody _rigidbody;
    
    // Score settings
    private int baseScore = 1;
    private CommonGameSettings commonSettings;
    
    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody>();
        
        // Register this bubble with the SessionManager
        if (SessionManager.Instance != null)
        {
            SessionManager.BubblesLeft.Add(this);
            SessionManager.OnBubbleLeftUpdate?.Invoke(SessionManager.BubblesLeft.Count);
        }
    }

    private void Start()
    {
        PlaySpawnEffect();
        
        // Get common settings from current game mode
        if (SessionManager.Instance != null)
        {
            var settings = SessionManager.Instance.GetCurrentGameModeSettings();
            if (settings != null)
            {
                commonSettings = settings.commonSettings;
            }
        }
    }

    private void OnDestroy()
    {
        // Unregister from SessionManager when destroyed
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.OnBubblePopped(this);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check for ground contact
        if (collision.gameObject.CompareTag("NoBubblesAlowed"))
        {
            PopBubble();
            return;
        }
        
        if (!collision.gameObject.TryGetComponent(out BubbleObject bubbleObject)) return;
        
        // If the bubble is already in our list, skip
        if (touchingBubbles.Contains(bubbleObject)) return;
            
        touchingBubbles.Add(bubbleObject);
        UpdateTouchingSameColorBubbles();
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent(out BubbleObject bubbleObject)) return;

        touchingBubbles.Remove(bubbleObject);
        UpdateTouchingSameColorBubbles();
        CleanLists();
    }
    
    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Water"))
        {
            PopBubble();
        }
    }

    private void UpdateTouchingSameColorBubbles()
    {
        touchingSameColorBubbles.Clear();
    
        foreach (BubbleObject bubble in touchingBubbles)
        {
            if (bubble != null && bubble.BubbleColor() == BubbleColor())
            {
                touchingSameColorBubbles.Add(bubble);
            }
        }

        bool shouldPop = wasShot || touchingSameColorBubbles.Any(bubble => bubble.wasShot);
    
        if (shouldPop && touchingSameColorBubbles.Count >= 2)
        {
            float chainBonus = CalculateChainBonus();
            
            // Pop connected bubbles and award points
            foreach (BubbleObject bubble in touchingSameColorBubbles)
            {
                if (bubble != null)
                {
                    if (!bubble.wasShot)
                    {
                        AwardPoints(chainBonus);
                    }
                    bubble.PopBubble(Random.Range(0, 0.2f));
                }
            }
        
            // Pop this bubble and award points if not shot
            if (!wasShot)
            {
                AwardPoints(chainBonus);
            }
            PopBubble();
        }
    }

    private float CalculateChainBonus()
    {
        if (commonSettings == null || !commonSettings.allowCombos)
            return 1f;

        float comboMultiplier = Mathf.Min(SessionManager.CurrentCombo, commonSettings.maxCombo);
        return 1f + (comboMultiplier * 0.1f);
    }

    private void AwardPoints(float chainBonus)
    {
        if (SessionManager.Instance == null || commonSettings == null)
            return;

        int score = Mathf.RoundToInt(baseScore * commonSettings.scoreMultiplier * chainBonus);
        SessionManager.Instance.UpdateScore(score);
    }
    
    private void CleanLists()
    {
        touchingBubbles.RemoveAll(bubble => bubble == null);
        touchingSameColorBubbles.RemoveAll(bubble => bubble == null);
    }
    
    public void MarkAsShot()
    {
        wasShot = true;
    }

    public void SetFrozenState(bool state)
    {
        _rigidbody.constraints = state ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
        _rigidbody.useGravity = !state;
    }
}