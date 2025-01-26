using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BubbleManager", menuName = "Scriptable Objects/BubbleManager")]
public class SOBubbleManager : ScriptableObject
{
    
    public BubbleBullet BubbleBulletPrefab;
    public BubbleAmmo BubbleAmmoPrefab;
    public BubbleObject BubbleObjectPrefab;
    public Material[] BubbleColors;
    public Material BubbleNull;
    
    public Material RandomColor()
    {
        if (BubbleColors.Length <= 0) return null;
        return BubbleColors[Random.Range(0, BubbleColors.Length)];
        
    }

    
}
