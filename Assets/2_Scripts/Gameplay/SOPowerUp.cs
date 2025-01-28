using UnityEngine;

[CreateAssetMenu(fileName = "PowerUp", menuName = "SO Power Up/New Power Up")]
public class SOPowerUp : ScriptableObject
{
    public new string name = "NewPowerUp";
    public float duration = 5f;
    public GameObject powerUpEffect;
    public GameObject padEffect;
    
}
