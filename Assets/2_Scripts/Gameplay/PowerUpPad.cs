using System;
using System.Collections;
using UnityEngine;
using VInspector;
using Random = UnityEngine.Random;


public class PowerUpPad : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float cooldownTime = 5;
    [SerializeField] private bool randomPowerUp = false;
    
    [ShowIf("randomPowerUp")][SerializeField] private SOPowerUp[] availablePowerUps;[EndIf]
    [HideIf("randomPowerUp")][SerializeField] private SOPowerUp selectedPowerUp;[EndIf]
    
    [Header("References")] 
    [SerializeField] private Transform powerUpEffectTransform;
    [SerializeField] private GameObject randomPowerUpPrefab;

    private bool _triggered = false;
    
    
    private void Start()
    {
        if (!randomPowerUpPrefab || !selectedPowerUp.padEffect) return;
        Instantiate(randomPowerUp ? selectedPowerUp.padEffect : randomPowerUpPrefab, powerUpEffectTransform);
    }


    private SOPowerUp SelectRandomPowerUp()
    {

        if (availablePowerUps.Length == 0)
        {
            Debug.LogError("No power-ups available.");
            return null;
        }
            
        _triggered = true;
        powerUpEffectTransform.gameObject.SetActive(false);
        StartCoroutine(StartCooldown());
        
        int index = Random.Range(0, availablePowerUps.Length);
        return availablePowerUps[index];
    }
  
    private SOPowerUp SelectedPowerUp()
    {

        if (selectedPowerUp == null)
        {
            Debug.LogError("No power-up selected.");
            return null;
        }
            
        _triggered = true;
        powerUpEffectTransform.gameObject.SetActive(false);
        StartCoroutine(StartCooldown());
        
        return selectedPowerUp;
    }

    private IEnumerator StartCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        _triggered = false;
        powerUpEffectTransform.gameObject.SetActive(true);
        
    }
    
    public SOPowerUp GetPowerUp()
    {
        if (_triggered) return null;
        
        if (randomPowerUp) {
            return SelectRandomPowerUp();
            
        } else {
            
            return SelectedPowerUp();
        }
    }
    
}
