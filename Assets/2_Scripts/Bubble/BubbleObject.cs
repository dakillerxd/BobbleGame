using UnityEngine;
using VInspector;

[RequireComponent(typeof(Rigidbody))]
public class BubbleObject : BubbleBase
{

    private Rigidbody _rigidbody;
    
    
    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody>();
    }



    [Button]
    private void SetRandomColor()
    {
        SelectRandomBubbleColor();
    }
}
