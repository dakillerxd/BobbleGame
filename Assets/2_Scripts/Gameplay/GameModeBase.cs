using UnityEngine;
using UnityEngine.Events;

public abstract class GameModeBase
{
    protected SessionManager sessionManager;
    public GameModeSettings settings;
    
    public UnityEvent OnGameWin = new UnityEvent();
    public UnityEvent OnGameLose = new UnityEvent();
    
    public GameModeBase(SessionManager sessionManager, GameModeSettings settings)
    {
        this.sessionManager = sessionManager;
        this.settings = settings;
    }
    
    public abstract void Initialize();
    public abstract void Update();
    public abstract void OnScoreUpdate(int newScore);
    public abstract void OnBubblePopped();
    public abstract void OnWaveComplete();
}

public class TimeBasedMode : GameModeBase
{
    private TimeModeSettings timeSettings;
    private float currentTime;
    private int lastScoreCheck;
    
    public TimeBasedMode(SessionManager sessionManager, TimeModeSettings settings) 
        : base(sessionManager, settings)
    {
        this.timeSettings = settings;
    }
    
    public override void Initialize()
    {
        currentTime = timeSettings.commonSettings.baseTime;
        lastScoreCheck = 0;
        SessionManager.OnTimeUpdate?.Invoke(currentTime);
    }
    
    public override void Update()
    {
        currentTime -= Time.deltaTime;
        SessionManager.OnTimeUpdate?.Invoke(currentTime);
        
        if (currentTime <= 0)
        {
            OnGameLose?.Invoke();
        }
    }
    
    public override void OnScoreUpdate(int newScore)
    {
        int scoreDifference = newScore - lastScoreCheck;
        if (scoreDifference >= timeSettings.scoreRequiredForTimeBoost)
        {
            currentTime += timeSettings.timeBoostAmount;
            lastScoreCheck = newScore;
            SessionManager.OnTimeUpdate?.Invoke(currentTime);
        }
    }
    
    public override void OnBubblePopped() { }
    
    public override void OnWaveComplete() { }
}

public class WaveBasedMode : GameModeBase
{
    private WaveModeSettings waveSettings;
    private float waveTimer;
    private int currentBubbleCount;
    
    public WaveBasedMode(SessionManager sessionManager, WaveModeSettings settings) 
        : base(sessionManager, settings)
    {
        this.waveSettings = settings;
    }
    
    public override void Initialize()
    {
        waveTimer = waveSettings.waveTimeout;
        currentBubbleCount = SessionManager.BubblesLeft.Count;
    }
    
    public override void Update()
    {
        waveTimer -= Time.deltaTime;
        SessionManager.OnTimeUpdate?.Invoke(waveTimer);
        
        if (waveTimer <= 0)
        {
            if (currentBubbleCount > 0)
            {
                OnGameLose?.Invoke();
            }
            else
            {
                waveTimer = waveSettings.waveTimeout;
                waveSettings.initialBubblesPerWave = Mathf.RoundToInt(waveSettings.initialBubblesPerWave * waveSettings.difficultyScaling);
                waveSettings.initialBubblesPerWave = Mathf.Min(waveSettings.initialBubblesPerWave, waveSettings.maxBubblesPerWave);
            }
        }
    }
    
    public override void OnScoreUpdate(int newScore) { }
    
    public override void OnBubblePopped()
    {
        currentBubbleCount = SessionManager.BubblesLeft.Count;
    }
    
    public override void OnWaveComplete() { }
}

public class TargetScoreMode : GameModeBase
{
    private TargetScoreModeSettings targetSettings;
    private float remainingTime;
    
    public TargetScoreMode(SessionManager sessionManager, TargetScoreModeSettings settings) 
        : base(sessionManager, settings)
    {
        this.targetSettings = settings;
    }
    
    public override void Initialize()
    {
        remainingTime = targetSettings.timeLimit;
        SessionManager.OnTimeUpdate?.Invoke(remainingTime);
    }
    
    public override void Update()
    {
        remainingTime -= Time.deltaTime;
        SessionManager.OnTimeUpdate?.Invoke(remainingTime);
        
        if (remainingTime <= 0)
        {
            OnGameLose?.Invoke();
        }
    }
    
    public override void OnScoreUpdate(int newScore)
    {
        if (newScore >= targetSettings.targetScore)
        {
            OnGameWin?.Invoke();
        }
    }
    
    public override void OnBubblePopped() { }
    
    public override void OnWaveComplete() { }
}