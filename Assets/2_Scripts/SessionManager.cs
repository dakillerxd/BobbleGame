using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VInspector;

public class SessionManager : MonoBehaviour
{
    public static  SessionManager Instance { get; private set; }


    
    public static int CurrentCombo { get; private set; }
    public static int CurrentScore { get; private set; }
    public static float CurrentTime {get; private set;}
    public static int CurrentWave {get; private set;}
    public static List<BubbleObject> BubblesLeft { get; private set; } = new List<BubbleObject>();
    public static List<BubbleSpawner> BubbleSpawners { get; private set; } = new List<BubbleSpawner>();
    
    public static UnityEvent<int> OnScoreUpdate = new UnityEvent<int>();
    public  static UnityEvent<int> OnComboUpdate = new UnityEvent<int>();
    public static UnityEvent<int> OnBubbleLeftUpdate = new UnityEvent<int>();
    public static UnityEvent<float> OnTimeUpdate = new UnityEvent<float>();
    public static UnityEvent<int> OnWaveUpdate = new UnityEvent<int>();
    public static UnityEvent OnSessionStart = new UnityEvent();

    [Header("Session Settings")]
    [SerializeField] private int waveTime = 30;
    [SerializeField] private float comboTime = 3f;
    [SerializeField] private float maxCombo = 5f;
    private float _comboTimer = 0f;
    private float _waveTimer = 0f;
    
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

        } else {
            Instance = this;
        }
    }

    private void Start()
    {
        StartNewSession();
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.F1))
        {
            StartNewSession();
        }
        
        
        
        
        // Combo timer
        if (_comboTimer > 0)
        {
            _comboTimer -= Time.deltaTime;
            if (_comboTimer <= 0)
            {
                CurrentCombo = 0;
            }
        }
        
        // Wave timer
        if (_waveTimer > 0)
        {
            _waveTimer -= Time.deltaTime;
            if (_waveTimer <= 0)
            {
                SpawnNewWave();
            }
        } else {
            _waveTimer = waveTime;
        }
    }

    

    public void UpdateScore(int score)
    {
        CurrentScore += score;
        OnScoreUpdate?.Invoke(CurrentScore);

        
        // Update combo
        _comboTimer = comboTime;
        if (CurrentCombo < maxCombo)
        {
            CurrentCombo += 1;
            OnComboUpdate?.Invoke(CurrentCombo);
        }

    }
    
    
    
    private void FindAllBubblesInLevel()
    {
        BubblesLeft.Clear();
        // Find all BubbleObjects in the scene
        BubbleObject[] bubbles = FindObjectsByType<BubbleObject>(FindObjectsSortMode.None);
        BubblesLeft.AddRange(bubbles);
        OnBubbleLeftUpdate?.Invoke(BubblesLeft.Count);
    }

    private void FindAllBubbleSpawners()
    {
        BubbleSpawners.Clear();
        // Find all BubbleSpawners in the scene
        BubbleSpawner[] spawners = FindObjectsByType<BubbleSpawner>(FindObjectsSortMode.None);
        BubbleSpawners.AddRange(spawners);
    }
    
    
    [Button] private void ResetSession()
    {
        CurrentScore = 0;
        CurrentTime = 0;
        CurrentWave = 0;
        CurrentCombo = 0;
        _comboTimer = 0f;
        _waveTimer = waveTime;
        BubbleSpawners.Clear();
        BubblesLeft.Clear();
        BubbleObject[] bubbles = FindObjectsByType<BubbleObject>(FindObjectsSortMode.None);
        foreach (BubbleObject bubble in bubbles)
        {
            Destroy(bubble.gameObject);
        }
        
        
        OnScoreUpdate?.Invoke(CurrentScore);
        OnTimeUpdate?.Invoke(CurrentTime);
        OnWaveUpdate?.Invoke(CurrentWave);
        OnComboUpdate?.Invoke(CurrentCombo);
    }

    [Button] private void StartNewSession()
    {
        ResetSession();
        FindAllBubbleSpawners();
        
        if (BubbleSpawners.Count > 0)
        {
            foreach (BubbleSpawner bubbleSpawner in BubbleSpawners)
            {
                bubbleSpawner.SpawnBubbles();
            }
        }

        FindAllBubblesInLevel();
        OnSessionStart?.Invoke();
    }

    private void SpawnNewWave()
    {
        CurrentWave += 1;
        OnWaveUpdate?.Invoke(CurrentWave);
        _waveTimer = waveTime;
        
        if (BubbleSpawners.Count > 0)
        {
            foreach (BubbleSpawner bubbleSpawner in BubbleSpawners)
            {
                bubbleSpawner.AddBubbles();
            }
        }

        FindAllBubblesInLevel();
        
        Debug.Log("New wave spawned");
    }

    

}
