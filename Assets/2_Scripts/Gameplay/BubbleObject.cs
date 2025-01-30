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
    public bool WasShot => wasShot;
    
    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody>();
        
        // Register this bubble with the GameManager
        if (GameManager.Instance != null)
        {
            GameManager.BubblesLeft.Add(this);
            GameManager.OnBubbleLeftUpdate?.Invoke(GameManager.BubblesLeft.Count);
        }
    }

    private void Start()
    {
        PlaySpawnEffect();
    }

    private void OnDestroy()
    {
        // Unregister from GameManager when destroyed
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBubblePopped(this);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        // Check for ground contact
        if (other.gameObject.CompareTag("NoBubblesAlowed"))
        {
            PopBubble();
            return;
        }
    }
    

    
    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Water"))
        {
            PopBubble();
        }
        
        if (other.TryGetComponent(out BubbleObject bubbleObject))
        {
            // If the bubble is already in our list, skip
            if (touchingBubbles.Contains(bubbleObject)) return;
            
            touchingBubbles.Add(bubbleObject);
            UpdateTouchingSameColorBubbles();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out BubbleObject bubbleObject))
        {
            touchingBubbles.Remove(bubbleObject);
            UpdateTouchingSameColorBubbles();
            CleanLists();
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
            // Pop connected bubbles
            foreach (BubbleObject bubble in touchingSameColorBubbles)
            {
                if (bubble != null)
                {
                    bubble.PopBubble(Random.Range(0, 0.2f));
                }
            }
        
            // Notify the player gun about popped bubbles
            if (PlayerGun != null)
            {
                PlayerGun.OnBubblesPopped(touchingSameColorBubbles.Count + 1);
            }

            PopBubble();
        }
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
    
}