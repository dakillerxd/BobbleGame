using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BubbleManager", menuName = "SO Manager/Bubble Manager")]
public class SOBubbleManager : ScriptableObject
{
    [Header("Bubble Types")]
    public BubbleBullet BubbleBulletPrefab;
    public BubbleAmmo BubbleAmmoPrefab;
    public BubbleObject BubbleObjectPrefab;
    public BubbleBase BubbleBasePrefab;
    public BubblePulseEffect BubbleEffectPrefab;
    
    [Space(10)]
    public Material[] BubbleColors;
    public Material BubbleBaseColor;
    
    public Material RandomColor()
    {
        if (BubbleColors.Length <= 0) return null;
        return BubbleColors[Random.Range(0, BubbleColors.Length)];
        
    }

    
}
