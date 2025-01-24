using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VInspector;
using PrimeTween;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class BubbleObject : BubbleBase
{
    
    [SerializeField] [ReadOnly] private bool wasShot = false;
    [SerializeField] [ReadOnly] private List<BubbleObject> touchingBubbles = new List<BubbleObject>();
    [SerializeField] [ReadOnly] private List<BubbleObject> touchingSameColorBubbles = new List<BubbleObject>();
    private Rigidbody _rigidbody;


    
    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        PlaySpawnEffect();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent(out BubbleObject bubbleObject)) return;
        
        // If the bubble is already in our list, skip
        if (touchingBubbles.Contains(bubbleObject)) return;
            
        // Add to touching list
        touchingBubbles.Add(bubbleObject);
        
        // Update same color bubbles list
        UpdateTouchingSameColorBubbles();
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent(out BubbleObject bubbleObject)) return;

        // Remove from touching list
        touchingBubbles.Remove(bubbleObject);
        
        // Update same color bubbles list
        UpdateTouchingSameColorBubbles();
        
        // Clean any null references
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
        // Clear the list and rebuild it
        touchingSameColorBubbles.Clear();
    
        // Only add bubbles that are actually touching and of the same color
        foreach (BubbleObject bubble in touchingBubbles)
        {
            if (bubble != null && bubble.BubbleColor() == BubbleColor())
            {
                touchingSameColorBubbles.Add(bubble);
            }
        }

        // Only proceed with popping if this bubble was shot or is touching a shot bubble
        bool shouldPop = wasShot || touchingSameColorBubbles.Any(bubble => bubble.wasShot);
    
        // If we have 3 or more same color bubbles (including this one), and should pop, trigger pop
        if (shouldPop && touchingSameColorBubbles.Count >= 2)  // 2 others + this one = 3 total
        {
            // Pop all touching same color bubbles
            foreach (BubbleObject bubble in touchingSameColorBubbles)
            {
                if (bubble != null)
                {
                    if (!bubble.wasShot)  // Only update score for non-shot bubbles
                    {
                        SessionManager.Instance?.UpdateScore(1);
                    }
                    bubble.PopBubble(Random.Range(0, 0.2f));
                }
            }
        
            // Pop this bubble and update score if it wasn't shot
            if (!wasShot)
            {
                SessionManager.Instance?.UpdateScore(1);
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
    

    public void SetFrozenState(bool state)
    {
        _rigidbody.constraints = state ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
        _rigidbody.useGravity = !state;
    }
    
}