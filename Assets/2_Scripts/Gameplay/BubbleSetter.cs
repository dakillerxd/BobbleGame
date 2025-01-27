
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using VInspector;

public class BubbleSetter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float cooldownTime = 5;
    [SerializeField] private bool randomizeBubbleColor = false;
    [HideIf("randomizeBubbleColor")][SerializeField] private Material bubbleColor;[EndIf]
    
    [Header("References")] 
    [SerializeField] private SOBubbleManager bubbleManager;
    [SerializeField] private Transform bubbleHolderTransform;
    [SerializeField] private Collider collider3D;
    
    

    private BubbleBase _currentBubble;
    private bool _triggered;

    private void Awake()
    {
        
        if (!bubbleManager || !collider3D || !bubbleHolderTransform)
        {
            Debug.LogError($"References are missing!");
            return;
        }

        SetCurrentBubble();
    }
    
    public void SetPlayerBubbleColor(PlayerGun player)
    {
        if (!_currentBubble || _triggered) return;
        
        _triggered = true;
        collider3D.enabled = false;
        player.ForceCurrentBubble(_currentBubble.BubbleColor());
        _currentBubble.PopBubble();
        StartCoroutine(StartCooldown());
        
    }
    
    
    private IEnumerator StartCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        _triggered = false;
        SetCurrentBubble();
    }
    
    private void ClearCurrentBubble()
    {
        _currentBubble = null;
        foreach (Transform child in bubbleHolderTransform)
        {
            Destroy(child.gameObject);
        }
    }

    private void SetCurrentBubble()
    {
        ClearCurrentBubble();
        _currentBubble = Instantiate(bubbleManager.BubbleBasePrefab, bubbleHolderTransform.position, Quaternion.identity, bubbleHolderTransform);
        _currentBubble.SetBubbleColor(randomizeBubbleColor ? bubbleManager.RandomColor() : bubbleColor);
        collider3D.enabled = true;
    }
    
    

#if UNITY_EDITOR
    
    private void OnValidate()
    {


    }
#endif
    
}
