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

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Settings")]
    [SerializeField] private SOGameModeConfig defaultSoGameMode;
    [SerializeField] private SOGameModeConfig[] availableGameModes;
    
    public static GameState CurrentGameState { get; private set; } = GameState.WaitingToStart;
    public static int CurrentScore { get; private set; }
    public static float CurrentTime { get; private set; }
    public static int CurrentWave { get; private set; }
    public static int CurrentDeaths { get; private set; }
    public static float CurrentWaveTimer { get; private set; }
    public static int CurrentWaveScore { get; private set; }
    public static List<BubbleObject> BubblesLeft { get; private set; } = new List<BubbleObject>();
    public static List<BubbleSpawner> BubbleSpawners { get; private set; } = new List<BubbleSpawner>();
    public static SOGameModeConfig CurrentGameMode { get; private set; }
    
    public static UnityEvent<GameState> OnGameStateChanged = new UnityEvent<GameState>();
    public static UnityEvent<int> OnScoreUpdate = new UnityEvent<int>();
    public static UnityEvent<int> OnBubbleLeftUpdate = new UnityEvent<int>();
    public static UnityEvent<float> OnTimeUpdate = new UnityEvent<float>();
    public static UnityEvent<int> OnWaveUpdate = new UnityEvent<int>();
    public static UnityEvent OnSessionStart = new UnityEvent();
    public static UnityEvent<SOGameModeConfig> OnGameModeChanged = new UnityEvent<SOGameModeConfig>();
    public static UnityEvent<int> OnDeathUpdate = new UnityEvent<int>();
    public static UnityEvent<float> OnWaveTimerUpdate = new UnityEvent<float>();
    public static UnityEvent<int> OnWaveScoreUpdate = new UnityEvent<int>();

    private int _lastScoreCheckForTimeBoost;
    private List<BubbleSpawner> _previousWaveSpawners = new List<BubbleSpawner>();
    private float _waveSpawnTimer = 0f;
    private bool _isWaitingForNextWave = false;



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
    
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            StartNewSession();
        }
        
        if (CurrentGameState == GameState.Playing)
        {
            UpdateGameTime();
            UpdateWaveTimer();
            CheckLoseConditions();
            CheckEndConditions();
            
            if (_isWaitingForNextWave)
            {
                _waveSpawnTimer -= Time.deltaTime;
                
                if (_waveSpawnTimer <= 0)
                {
                    _isWaitingForNextWave = false;
                    StartNewWave();
                }
            }
        }
    }

    
    
#region Game State Management // -------------------------------------------------------------------------------------

    private void SetGameState(GameState newState)
    {
        if (CurrentGameState != newState)
        {
            CurrentGameState = newState;
            OnGameStateChanged?.Invoke(CurrentGameState);
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
        OnGameModeChanged?.Invoke(CurrentGameMode);
    }
    
#endregion Game State Management // -------------------------------------------------------------------------------------


#region Game Logic // -------------------------------------------------------------------------------------
    
    private void UpdateWaveTimer()
    {
        if (CurrentGameMode.EnableWaveClearFailCondition && !_isWaitingForNextWave)
        {
            CurrentWaveTimer -= Time.deltaTime;
            OnWaveTimerUpdate?.Invoke(CurrentWaveTimer);
        }
    }

    private void CheckLoseConditions()
    {
        if (CurrentGameMode.HasReachedLoseCondition(
                CurrentTime,
                CurrentScore,
                CurrentDeaths,
                BubblesLeft.Count,
                CurrentWaveTimer,
                CurrentWaveScore))
        {
            HandleGameEnd(false);
        }
    }
    
    public void PlayerDied()
    {
        CurrentDeaths++;
        OnDeathUpdate?.Invoke(CurrentDeaths);
    }
    
    private void StartNewWave()
    {
        CurrentWave++;
        OnWaveUpdate?.Invoke(CurrentWave);
        
        // Reset wave-specific variables
        CurrentWaveTimer = CurrentGameMode.TimeToCompleteWave;
        CurrentWaveScore = 0;
        OnWaveTimerUpdate?.Invoke(CurrentWaveTimer);
        OnWaveScoreUpdate?.Invoke(CurrentWaveScore);
        
        SpawnBubblesForCurrentWave();
        FindAllBubblesInLevel();
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
        
        // Update total score
        CurrentScore += multipliedScore;
        OnScoreUpdate?.Invoke(CurrentScore);
        
        // Update wave score
        CurrentWaveScore += multipliedScore;
        OnWaveScoreUpdate?.Invoke(CurrentWaveScore);
        
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
    
#endregion Game Logic // -------------------------------------------------------------------------------------


#region Session Management // -------------------------------------------------------------------------------------

    [Button]
    private void ResetSession()
    {
        CurrentScore = 0;
        CurrentTime = CurrentGameMode.IsCountUp ? 0 : CurrentGameMode.TargetTime;
        CurrentWave = 0;
        CurrentDeaths = 0;
        CurrentWaveTimer = CurrentGameMode.TimeToCompleteWave;
        CurrentWaveScore = 0;
        _lastScoreCheckForTimeBoost = 0;
        _isWaitingForNextWave = false;
        _waveSpawnTimer = 0f;
        
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
        OnDeathUpdate?.Invoke(CurrentDeaths);
        OnWaveTimerUpdate?.Invoke(CurrentWaveTimer);
        OnWaveScoreUpdate?.Invoke(CurrentWaveScore);
        
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
    
#endregion Session Management // -------------------------------------------------------------------------------------


#region Bubble Management // -------------------------------------------------------------------------------------
    
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

    public void OnBubblePopped(BubbleObject bubble)
    {
        
        if (CurrentGameState != GameState.Playing) return;
            
        BubblesLeft.Remove(bubble);
        OnBubbleLeftUpdate?.Invoke(BubblesLeft.Count);

        if (BubblesLeft.Count == 0)
        {
            _isWaitingForNextWave = true;
            _waveSpawnTimer = CurrentGameMode.WaveInterval;
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
    
#endregion Bubble Management // -------------------------------------------------------------------------------------
    

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