using UnityEngine;
using VInspector;
using PrimeTween;

[RequireComponent(typeof(AudioSource))]
public class BubbleBase : MonoBehaviour
{
    
    
    [Foldout("References")]
    [SerializeField] protected SOAudioEvent bubblePopSfx;
    [SerializeField] protected SOBubbleManager bubbleManager;
    [SerializeField] protected MeshRenderer meshRenderer;
    [SerializeField] protected AudioSource audioSource;
    [EndFoldout]
    
    [SerializeField] [ReadOnly] protected Material bubbleColor;
    protected PlayerGun PlayerGun;
    private Sequence _popSequence;
    private Sequence _spawnSequence;
    
    protected virtual void Awake()
    {
        if(!meshRenderer) meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer && meshRenderer.material)
        {
            bubbleColor = meshRenderer.material;
        }
        
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }
    

    public void PopBubble(float delay = 0)
    {
        if (_popSequence.isAlive) return;
        _spawnSequence.Stop();
        
        _popSequence = Sequence.Create()
            .ChainDelay(delay)
            .Group(Tween.PunchScale(gameObject.transform, strength: new Vector3(1.2f, 1.2f, 1.2f), duration: 0.1f, frequency: 1, easeBetweenShakes: Ease.OutQuad))
            .ChainCallback(() => { if (bubblePopSfx) bubblePopSfx.PlayAtPoint(transform.position); })
            .ChainCallback(() => Destroy(gameObject));
    }
    
    protected void PlaySpawnEffect()
    {
        if (_popSequence.isAlive || _spawnSequence.isAlive) return;

        _spawnSequence = Sequence.Create()
            .Group(Tween.PunchScale(gameObject.transform, strength: new Vector3(1.1f, 1.1f, 1.1f), duration: 0.1f, frequency: 1, easeBetweenShakes: Ease.OutQuad));
    }
    
    public void SetBubbleColor(Material material)
    {
        if (!meshRenderer) return;
        bubbleColor = material;
        meshRenderer.material = material;
        gameObject.name = material.name;
    }
    
    public Material BubbleColor()
    {
        return bubbleColor;
    }
    
    public void SelectRandomBubbleColor()
    {
        if (!bubbleManager) return;
        SetBubbleColor(bubbleManager.RandomColor());

    }
    
    public void SetPlayerGun(PlayerGun gun)
    {
        PlayerGun = gun;
    }
    
}
