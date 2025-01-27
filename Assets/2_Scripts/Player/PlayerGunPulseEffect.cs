using UnityEngine;
using PrimeTween;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class PlayerGunPulseEffect : MonoBehaviour
{
    private float _targetRadius;
    
    public void Initialize(float radius, float duration)
    {
        transform.localScale = Vector3.one * 0.1f;
        
        Sequence.Create()
            .Group(Tween.Scale(transform, startValue: Vector3.one * 0.1f, endValue: Vector3.one * radius, duration: duration, Ease.OutElastic))
            .OnComplete(() => Destroy(gameObject));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out BubbleObject bubble))
        {
            bubble.PopBubble();
        }
    }
}