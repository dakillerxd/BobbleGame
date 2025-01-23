using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BubbleBullet : BubbleBase
{

    private Rigidbody _rigidbody;



    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody>();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        collision.gameObject.TryGetComponent(out BubbleObject bubble);
        
        if (bubble)
        {
            if (bubble.BubbleColor() == BubbleColor())
            {
                bubble.DestroyBubble();
                DestroyBubble();
            }
            else
            {
                BubbleObject bubbleObject = Instantiate(bubbleManager.BubbleObjectPrefab, transform.position, Quaternion.identity);
                bubbleObject.SetBubbleColor(BubbleColor());
                Destroy(gameObject);
            }
            

        }
        else
        {
            
            BubbleObject bubbleObject = Instantiate(bubbleManager.BubbleObjectPrefab, transform.position, Quaternion.identity);
            bubbleObject.SetBubbleColor(BubbleColor());
            Destroy(gameObject);
        }

    }


    public void ShootInDirection(Vector3 direction, float force)
    {
        _rigidbody.AddForce(direction * force, ForceMode.Impulse);
    }
    
}
