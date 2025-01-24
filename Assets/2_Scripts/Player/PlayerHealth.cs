using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement), typeof(CharacterController))]
public class PlayerHealth : MonoBehaviour
{
    private Vector3 _spawnPoint;

    private void Awake()
    {
        _spawnPoint = transform.position;
    }

    private void OnEnable()
    {
        SessionManager.OnSessionStart.AddListener(MoveToSpawnPoint);
    }

    private void OnDisable()
    {
        SessionManager.OnSessionStart.RemoveListener(MoveToSpawnPoint);
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Water"))
        {
            MoveToSpawnPoint();
        }
    }
    
    public void MoveToSpawnPoint()
    {
        transform.position = _spawnPoint;
    }
}