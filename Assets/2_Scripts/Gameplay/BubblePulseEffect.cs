using UnityEngine;
using PrimeTween;


public class BubblePulseEffect : BubbleBase
{
    
    public void Initialize(float radius, float duration)
    {
        transform.localScale = Vector3.one * 0.1f;
        
        Sequence.Create()
            .Group(Tween.Scale(transform, startValue: Vector3.one * 0.1f, endValue: Vector3.one * radius, duration: duration, Ease.OutElastic))
            .OnComplete(() => PopBubble());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out BubbleObject bubble))
        {
            bubble.PopBubble();
        }
    }
}