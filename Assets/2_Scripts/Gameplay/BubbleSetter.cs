
using System.Collections;
using UnityEngine;

public class BubbleSetter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Material bubbleColor;
    [SerializeField] private float cooldownTime = 5;
    [SerializeField] private bool randomizeBubbleColor = false;
    
    [Header("References")] 
    [SerializeField] private SOBubbleManager bubbleManager;
    [SerializeField] private MeshRenderer meshRenderer;

    private bool _triggered;

    private void Awake()
    {
        if (!meshRenderer) meshRenderer = GetComponent<MeshRenderer>();
            
        if (!bubbleManager)
        {
            Debug.LogError($"BubbleManager is null");
            return;
        }
        
        if (randomizeBubbleColor || !bubbleColor) SetBubbleColor();
    }
    
    
    private IEnumerator StartCooldown()
    {
        _triggered = true;
        SetBubbleColorBase();
        yield return new WaitForSeconds(cooldownTime);
        _triggered = false;
        SetBubbleColor();
    }

    private void SetBubbleColor()
    {
        meshRenderer.enabled = true;
        
        if (!randomizeBubbleColor)
        {
            meshRenderer.material = bubbleColor;
        }
        else
        {
            bubbleColor = bubbleManager.RandomColor();
            meshRenderer.material = bubbleColor;
        }
    }

    private void SetBubbleColorBase()
    {
        meshRenderer.enabled = false;
    }
    
    public void SetPlayerBubbleColor(PlayerGun player)
    {
        if (!bubbleColor || _triggered) return;
        
        player.ForceCurrentBubble(bubbleColor);
        StartCoroutine(StartCooldown());
    }

#if UNITY_EDITOR
    
    private void OnValidate()
    {
        if (meshRenderer || !bubbleColor)
        {
            SetBubbleColor();
        }
    }
#endif
    
}
