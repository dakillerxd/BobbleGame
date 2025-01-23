using UnityEngine;
using UnityEngine.Serialization;

public class BubbleBase : MonoBehaviour
{
    [SerializeField] protected SOBubbleManager bubbleManager;
    [SerializeField] private MeshRenderer meshRenderer;
    private Material _bubbleColor;
    
    protected virtual void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    
    public void DestroyBubble()
    {
        Destroy(gameObject);
    }
    
    
    public void SetBubbleColor(Material material)
    {
        if (!meshRenderer) return;
        _bubbleColor = material;
        meshRenderer.material = material;
        gameObject.name = material.name;
    }
    
    
    public Material BubbleColor()
    {
        return _bubbleColor;
    }
    
    public void SelectRandomBubbleColor()
    {
        if (!bubbleManager) return;
        SetBubbleColor(bubbleManager.SelectRandomColor());

    }
    
}
