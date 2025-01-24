using System;
using TMPro;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(PlayerMovement), typeof(CharacterController), typeof(AudioSource))]
public class PlayerHealth : MonoBehaviour
{
    [Foldout("References")]
    [SerializeField] private SOAudioEvent deathSfx;
    [EndFoldout]
    
    private AudioSource _audioSource;
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
            deathSfx?.Play(_audioSource);
        }
    }
    
    public void MoveToSpawnPoint()
    {
        transform.position = _spawnPoint;
    }
}