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
        if (collision.gameObject.CompareTag("NoBubblesAlowed"))
        {
            PopBubble();
            return;
        } 
        
        if (collision.gameObject.TryGetComponent(out BubbleSetter bubbleSetter)) {
            
            PopBubble();
            bubbleSetter.SetPlayerBubbleColor(PlayerGun);
            return;
        } 
        
        
        
        ContactPoint contact = collision.GetContact(0);
        float bubbleRadius = bubbleManager.BubbleObjectPrefab.transform.localScale.x / 2f;
        Vector3 spawnPosition = contact.point + (contact.normal * bubbleRadius);
    
        TurnIntoBubbleObject(spawnPosition);
        
        
    }
    
    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Water"))
        {
            PopBubble();
        }
    }

    private void TurnIntoBubbleObject(Vector3 spawnPosition)
    {
        BubbleObject bubbleObject = Instantiate(
            bubbleManager.BubbleObjectPrefab, 
            spawnPosition, 
            Quaternion.identity
        );
        
        bubbleObject.SetBubbleColor(BubbleColor());
        bubbleObject.MarkAsShot();
        bubbleObject.SetPlayerGun(PlayerGun);
        Destroy(gameObject);
    }
    
    public void ShootInDirection(Vector3 direction, float force, PlayerGun player)
    {
        _rigidbody.AddForce(direction * force, ForceMode.Impulse);
        PlayerGun = player;
    }
}