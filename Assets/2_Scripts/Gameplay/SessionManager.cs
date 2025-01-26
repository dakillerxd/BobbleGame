using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using VInspector;

public enum GameState
{
    WaitingToStart,
    Playing,
    GameOver
}

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    // Static properties for game state
    public static int CurrentScore { get; private set; }
    public static float CurrentTime { get; private set; }
    public static int CurrentWave { get; private set; }
    public static List<BubbleObject> BubblesLeft { get; private set; } = new List<BubbleObject>();
    public static List<BubbleSpawner> BubbleSpawners { get; private set; } = new List<BubbleSpawner>();
    public static SOGameModeConfig CurrentGameMode { get; private set; }

    // Events
    public static UnityEvent<GameState> OnGameStateChanged = new UnityEvent<GameState>();
    public static UnityEvent<int> OnScoreUpdate = new UnityEvent<int>();
    public static UnityEvent<int> OnBubbleLeftUpdate = new UnityEvent<int>();
    public static UnityEvent<float> OnTimeUpdate = new UnityEvent<float>();
    public static UnityEvent<int> OnWaveUpdate = new UnityEvent<int>();
    public static UnityEvent OnSessionStart = new UnityEvent();
    
    [SerializeField] private SOGameModeConfig defaultSoGameMode;
    private GameState _currentGameState = GameState.WaitingToStart;
    private int _lastScoreCheckForTimeBoost;
    private List<BubbleSpawner> _previousWaveSpawners = new List<BubbleSpawner>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            SetGameMode(defaultSoGameMode);
        }
    }

    public static void SetGameMode(SOGameModeConfig newSoGameMode)
    {
        if (newSoGameMode == null)
        {
            Debug.LogError("Attempted to set null game mode!");
            return;
        }
        
        CurrentGameMode = newSoGameMode;
    }

    private void Start()
    {
        StartNewSession();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) && _currentGameState != GameState.Playing)
        {
            StartNewSession();
        }
        
        if (_currentGameState == GameState.Playing)
        {
            UpdateGameTime();
            CheckEndConditions();
        }
    }

    private void UpdateGameTime()
    {
        float deltaTime = CurrentGameMode.IsCountUp ? Time.deltaTime : -Time.deltaTime;
        CurrentTime += deltaTime;
        OnTimeUpdate?.Invoke(CurrentTime);
    }

    private void CheckEndConditions()
    {
        if (CurrentGameMode.HasReachedEndCondition(CurrentTime, CurrentScore, CurrentWave))
        {
            bool isWin = CurrentGameMode.IsWinCondition(CurrentTime, CurrentScore, CurrentWave);
            HandleGameEnd(isWin);
        }
    }

    public void UpdateScore(int score)
    {
        int multipliedScore = Mathf.RoundToInt(score * CurrentGameMode.ScoreMultiplier);
        CurrentScore += multipliedScore;
        OnScoreUpdate?.Invoke(CurrentScore);
        
        if (CurrentGameMode.EnableTimeBoosts)
        {
            int scoreDifference = CurrentScore - _lastScoreCheckForTimeBoost;
            if (scoreDifference >= CurrentGameMode.ScoreRequiredForTimeBoost)
            {
                CurrentTime += CurrentGameMode.TimeBoostAmount;
                _lastScoreCheckForTimeBoost = CurrentScore;
                OnTimeUpdate?.Invoke(CurrentTime);
            }
        }
    }
    
    private void HandleGameEnd(bool isWin)
    {
        Debug.Log(isWin ? "Game Won!" : "Game Lost!");
        SetGameState(GameState.GameOver);
        if (!isWin)
        {
            ResetSession();
        }
    }
    
    private void SetGameState(GameState newState)
    {
        if (_currentGameState != newState)
        {
            _currentGameState = newState;
            OnGameStateChanged?.Invoke(_currentGameState);
        }
    }
    
    private void FindAllBubblesInLevel()
    {
        BubblesLeft.Clear();
        BubbleObject[] bubbles = FindObjectsByType<BubbleObject>(FindObjectsSortMode.None);
        BubblesLeft.AddRange(bubbles);
        OnBubbleLeftUpdate?.Invoke(BubblesLeft.Count);
    }

    private void FindAllBubbleSpawners()
    {
        BubbleSpawners.Clear();
        BubbleSpawner[] spawners = FindObjectsByType<BubbleSpawner>(FindObjectsSortMode.None);
        BubbleSpawners.AddRange(spawners);
    }
    
    [Button]
    private void ResetSession()
    {
        CurrentScore = 0;
        CurrentTime = CurrentGameMode.IsCountUp ? 0 : CurrentGameMode.TargetTime;
        CurrentWave = 0;
        _lastScoreCheckForTimeBoost = 0;
        
        BubbleSpawners.Clear();
        BubblesLeft.Clear();
        _previousWaveSpawners.Clear();
        
        // Destroy all existing bubbles
        BubbleObject[] bubbles = FindObjectsByType<BubbleObject>(FindObjectsSortMode.None);
        foreach (BubbleObject bubble in bubbles)
        {
            Destroy(bubble.gameObject);
        }
        
        OnScoreUpdate?.Invoke(CurrentScore);
        OnTimeUpdate?.Invoke(CurrentTime);
        OnWaveUpdate?.Invoke(CurrentWave);
        
        SetGameState(GameState.WaitingToStart);
    }

    [Button]
    private void StartNewSession()
    {
        ResetSession();
        FindAllBubbleSpawners();
        SpawnBubblesForCurrentWave();
        FindAllBubblesInLevel();
        
        SetGameState(GameState.Playing);
        OnSessionStart?.Invoke();
    }

    public void OnBubblePopped(BubbleObject bubble)
    {
        if (_currentGameState != GameState.Playing) return;
        
        BubblesLeft.Remove(bubble);
        OnBubbleLeftUpdate?.Invoke(BubblesLeft.Count);

        if (BubblesLeft.Count == 0)
        {
            CurrentWave++;
            OnWaveUpdate?.Invoke(CurrentWave);
            
            SpawnBubblesForCurrentWave();
            FindAllBubblesInLevel();
        }
    }
    
    private void SpawnBubblesForCurrentWave()
    {
        var activeSpawners = CurrentGameMode.GetActiveSpawners(BubbleSpawners, CurrentWave, _previousWaveSpawners);
        _previousWaveSpawners = new List<BubbleSpawner>(activeSpawners);

        foreach (BubbleSpawner spawner in activeSpawners)
        {
            spawner.SpawnBubbles();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (defaultSoGameMode == null)
        {
            Debug.LogWarning("Default GameModeConfigSO is not assigned!");
        }
    }
#endif
}