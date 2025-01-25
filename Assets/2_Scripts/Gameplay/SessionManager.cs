using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

    public static int CurrentCombo { get; private set; }
    public static int CurrentScore { get; private set; }
    public static float CurrentTime { get; private set; }
    public static int CurrentWave { get; private set; }
    public static List<BubbleObject> BubblesLeft { get; private set; } = new List<BubbleObject>();
    public static List<BubbleSpawner> BubbleSpawners { get; private set; } = new List<BubbleSpawner>();
    
    public static UnityEvent<GameState> OnGameStateChanged = new UnityEvent<GameState>();
    public static UnityEvent<int> OnScoreUpdate = new UnityEvent<int>();
    public static UnityEvent<int> OnComboUpdate = new UnityEvent<int>();
    public static UnityEvent<int> OnBubbleLeftUpdate = new UnityEvent<int>();
    public static UnityEvent<float> OnTimeUpdate = new UnityEvent<float>();
    public static UnityEvent<int> OnWaveUpdate = new UnityEvent<int>();
    public static UnityEvent OnSessionStart = new UnityEvent();
    
    [Header("Game Mode Settings")]
    [SerializeField] private GameModeType currentGameMode;
    [SerializeField] private TimeModeSettings timeModeSettings;
    [SerializeField] private WaveModeSettings waveModeSettings;
    [SerializeField] private TargetScoreModeSettings targetScoreModeSettings;
    
    private SpawnerManager spawnerManager = new SpawnerManager();
    private GameState currentGameState = GameState.WaitingToStart;
    private GameModeBase activeGameMode;
    private float _comboTimer = 0f;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        InitializeGameMode();
        StartNewSession();
    }
    
    public GameModeSettings GetCurrentGameModeSettings()
    {
        return activeGameMode?.settings;
    }
    
    private void InitializeGameMode()
    {
        switch (currentGameMode)
        {
            case GameModeType.TimeBased:
                activeGameMode = new TimeBasedMode(this, timeModeSettings);
                break;
            case GameModeType.WaveBased:
                activeGameMode = new WaveBasedMode(this, waveModeSettings);
                break;
            case GameModeType.TargetScore:
                activeGameMode = new TargetScoreMode(this, targetScoreModeSettings);
                break;
        }
        
        activeGameMode.OnGameWin.AddListener(HandleGameWin);
        activeGameMode.OnGameLose.AddListener(HandleGameLose);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) && currentGameState != GameState.Playing)
        {
            StartNewSession();
        }
        
        if (activeGameMode != null && currentGameState == GameState.Playing)
        {
            activeGameMode.Update();
        }
        
        // Combo timer
        if (_comboTimer > 0 && currentGameState == GameState.Playing)
        {
            _comboTimer -= Time.deltaTime;
            if (_comboTimer <= 0)
            {
                CurrentCombo = 0;
                OnComboUpdate?.Invoke(CurrentCombo);
            }
        }
    }

    public void UpdateScore(int score)
    {
        // Apply score multiplier from common settings
        int multipliedScore = Mathf.RoundToInt(score * activeGameMode.settings.commonSettings.scoreMultiplier);
        CurrentScore += multipliedScore;
        OnScoreUpdate?.Invoke(CurrentScore);
        activeGameMode?.OnScoreUpdate(CurrentScore);
        
        // Update combo using common settings
        var commonSettings = activeGameMode.settings.commonSettings;
        if (commonSettings.allowCombos)
        {
            _comboTimer = commonSettings.comboTime;
            if (CurrentCombo < commonSettings.maxCombo)
            {
                CurrentCombo += 1;
                OnComboUpdate?.Invoke(CurrentCombo);
            }
        }
    }
    
    public void UpdateWaveSettings()
    {
        if (activeGameMode is WaveBasedMode waveMode)
        {
            WaveModeSettings settings = (WaveModeSettings)waveMode.settings;
            settings.initialBubblesPerWave = Mathf.RoundToInt(settings.initialBubblesPerWave * settings.difficultyScaling);
            settings.initialBubblesPerWave = Mathf.Min(settings.initialBubblesPerWave, settings.maxBubblesPerWave);
        }
    }
    
    private void HandleGameLose()
    {
        Debug.Log("Game Lost!");
        SetGameState(GameState.GameOver);
        ResetSession();
    }
    
    private void HandleGameWin()
    {
        Debug.Log("Game Won!");
        SetGameState(GameState.GameOver);
    }
    
    private void SetGameState(GameState newState)
    {
        if (currentGameState != newState)
        {
            currentGameState = newState;
            OnGameStateChanged?.Invoke(currentGameState);
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
        CurrentTime = 0;
        CurrentWave = 0;
        CurrentCombo = 0;
        _comboTimer = 0f;
        
        BubbleSpawners.Clear();
        BubblesLeft.Clear();
        
        // Destroy all existing bubbles
        BubbleObject[] bubbles = FindObjectsByType<BubbleObject>(FindObjectsSortMode.None);
        foreach (BubbleObject bubble in bubbles)
        {
            Destroy(bubble.gameObject);
        }
        
        OnScoreUpdate?.Invoke(CurrentScore);
        OnTimeUpdate?.Invoke(CurrentTime);
        OnWaveUpdate?.Invoke(CurrentWave);
        OnComboUpdate?.Invoke(CurrentCombo);
        
        SetGameState(GameState.WaitingToStart);
    }

    [Button]
    private void StartNewSession()
    {
        ResetSession();
        InitializeGameMode();
        activeGameMode.Initialize();
        
        FindAllBubbleSpawners();
        spawnerManager.Initialize(BubbleSpawners);
        
        SpawnBubblesForCurrentWave();
        FindAllBubblesInLevel();
        
        SetGameState(GameState.Playing);
        OnSessionStart?.Invoke();
    }

    public void OnBubblePopped(BubbleObject bubble)
    {
        if (currentGameState != GameState.Playing) return;
        
        BubblesLeft.Remove(bubble);
        OnBubbleLeftUpdate?.Invoke(BubblesLeft.Count);
        activeGameMode?.OnBubblePopped();

        if (BubblesLeft.Count == 0)
        {
            activeGameMode?.OnWaveComplete();
            UpdateWaveSettings();
            CurrentWave++;
            OnWaveUpdate?.Invoke(CurrentWave);
            
            SpawnBubblesForCurrentWave();
            FindAllBubblesInLevel();
        }
    }
    
    private void SpawnBubblesForCurrentWave()
    {
        var activeSpawners = spawnerManager.GetSpawnersForWave(
            CurrentWave, 
            activeGameMode.settings.commonSettings.spawnerSettings
        );

        foreach (BubbleSpawner spawner in activeSpawners)
        {
            spawner.SpawnBubbles();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Validate settings if needed
    }
#endif
}