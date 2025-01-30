using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using VInspector;

public enum SpawnerSelectionMode
{
    [Tooltip("Use all available spawners")]
    All,
    [Tooltip("Randomly select a subset of spawners")]
    Random,
    [Tooltip("Use increasing number of spawners based on wave number")]
    WaveBased
}

public enum TimerMode
{
    [Tooltip("Timer counts up from 0")]
    CountUp,
    [Tooltip("Timer counts down from target time")]
    CountDown,
    [Tooltip("Timer is disabled")]
    Off
}

[CreateAssetMenu(fileName = "GameMode", menuName = "SO GameMode/GameMode Configuration")]
public class SOGameModeConfig : ScriptableObject
{
    [Header("Mode Information")] // --------------------------------------------------------------------------------------------
    [Tooltip("Display name for this game mode")]
    [SerializeField] private string modeName = "New GameMode";
    
    [Tooltip("Detailed description of how this game mode works")]
    [SerializeField] [TextArea] private string description;

    [Header("Timer Settings")] // --------------------------------------------------------------------------------------------
    [Tooltip("Timer mode for this game mode")]
    [SerializeField] private TimerMode timerMode = TimerMode.Off;

    [ShowIf("IsTimerCountUp")]
    [Tooltip("Target time to reach in seconds (count up mode)")]
    [SerializeField] private float targetTime = 60f;
    [EndIf]
    
    [ShowIf("IsTimerEnabled")]
    [Tooltip("Time added/subtracted when popping a bubble")]
    [SerializeField] private float bubbleTimeWorth = 1f;
    [Tooltip("Percentage of time remaining that triggers warning state (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float warningThreshold = 0.25f;
    [Tooltip("Color of timer during normal state")]
    [SerializeField] private Color normalTimerColor = Color.white;
    [Tooltip("Color of timer during warning state")]
    [SerializeField] private Color warningTimerColor = Color.red;
    [EndIf]
    
    [Header("Score Settings")] // --------------------------------------------------------------------------------------------
    [Tooltip("The worth of a bubble")]
    [SerializeField] private int bubbleScoreWorth = 1;
    [Tooltip("Multiplier applied to all score gains")]
    [SerializeField] private float scoreMultiplier = 1f;

    [Header("Wave Settings")] // --------------------------------------------------------------------------------------------
    [Tooltip("Time between waves in seconds")]
    [SerializeField] [Min(1)] private float waveInterval = 5f;
    
    
    [Header("Bubble Spawning Settings")] // --------------------------------------------------------------------------------------------
    [Tooltip("How spawners are selected for each wave")]
    [SerializeField] private SpawnerSelectionMode spawnerSelectionMode = SpawnerSelectionMode.All;

    [ShowIf("IsRandomSpawnerMode")]
    [Tooltip("Number of spawners to use when in Random mode")]
    [SerializeField] [Min(0)] private int spawnersPerWave = 1;
    [Tooltip("If false, prevents same spawner from being used in consecutive waves")]
    [SerializeField] private bool allowConsecutiveReuse = true;
    [EndIf]

    [ShowIf("IsWaveBasedMode")]
    [Tooltip("Starting number of spawners in the first wave")]
    [SerializeField] [Min(0)] private int initialSpawnersPerWave = 1;
    [Tooltip("How much to increase spawner count each wave (multiplier)")]
    [SerializeField] [Min(0)] private float spawnerScaling = 1.2f;
    [Tooltip("Maximum number of spawners allowed in any wave")]
    [SerializeField] [Min(0)]private int maxSpawnersPerWave = 5;
    [Tooltip("Minimum number of spawners required in any wave")]
    [SerializeField] [Min(0)] private int minSpawnersPerWave = 1;
    [EndIf]
    
    
    [Space(10)]
    [Tooltip("Chance (0-1) that a bubble will spawn with same color as nearby bubbles")]
    [SerializeField] [Range(0f, 1f)] private float sameColorSpawnChance = 0.6f;
    [Tooltip("Enable wave-based scaling for bubble counts")]
    [SerializeField] private bool enableBubbleScaling;


    [HideIf("enableBubbleScaling")]
    [Tooltip("Fixed range of bubbles to spawn per wave when scaling is disabled")]
    [SerializeField] private Vector2Int bubbleRange = new Vector2Int(3, 10);
    [EndIf]

    [ShowIf("enableBubbleScaling")]
    [Tooltip("Starting number of bubbles in the first wave")]
    [SerializeField] [Min(0)] private int initialBubbleCount = 3;
    [Tooltip("Maximum number of bubbles allowed in any wave")]
    [SerializeField] private int maxBubbleCount = 50;
    [Tooltip("How much to increase bubble count each wave (multiplier)")]
    [SerializeField] [Min(1)] private float bubbleScaling = 1.5f;
    [EndIf]
    

    

    [Header("Win Conditions")] // --------------------------------------------------------------------------------------------
    [Tooltip("Enable time-based win condition")]
    [SerializeField] private bool useTimeForWinCondition;
    
    [Tooltip("Enable score-based win condition")]
    [SerializeField] private bool enableScoreCondition;
    
    [ShowIf("enableScoreCondition")]
    [Tooltip("Score required to win")]
    [SerializeField] private int targetScore = 1000;
    [EndIf]
    
    [Tooltip("Enable wave-based win condition")]
    [SerializeField] private bool enableWaveCondition;
    
    [ShowIf("enableWaveCondition")]
    [Tooltip("Number of waves to complete")]
    [SerializeField] private int targetWave = 10;
    [EndIf]

    [Header("Lose Conditions")] // --------------------------------------------------------------------------------------------
    [SerializeField] private bool enableWaveClearFailCondition;
    [ShowIf("enableWaveClearFailCondition")]
    [Tooltip("Player must clear all bubbles before wave timer ends")]
    [SerializeField] private float timeToCompleteWave = 30f;
    [EndIf]
    
    [SerializeField] private bool enableMaxDeathsCondition;
    [ShowIf("enableMaxDeathsCondition")]
    [Tooltip("Maximum number of deaths allowed")]
    [SerializeField] private int maxDeaths = 3;
    [EndIf]
    
    [SerializeField] private bool enableMinWaveScoreCondition;
    [ShowIf("enableMinWaveScoreCondition")]
    [Tooltip("Minimum score required per wave")]
    [SerializeField] private int minScorePerWave = 100;
    [EndIf]
    
    [SerializeField] private bool enableMaxBubblesCondition;
    [ShowIf("enableMaxBubblesCondition")]
    [Tooltip("Maximum number of bubbles that can be active at once")]
    [SerializeField] private int maxActiveBubbles = 20;
    [EndIf]
    
    [ShowIf("IsTimerCountDown")]
    [Tooltip("Enable time-based lose condition (time runs out)")]
    [SerializeField] private bool useTimeForLoseCondition;
    [EndIf]



    
    
    // Property getters // --------------------------------------------------------------------------------------------
    public bool IsRandomSpawnerMode => spawnerSelectionMode == SpawnerSelectionMode.Random;
    public bool IsTimerEnabled => timerMode != TimerMode.Off;
    public bool IsTimerCountUp => timerMode == TimerMode.CountUp;
    public bool IsTimerCountDown => timerMode == TimerMode.CountDown;
    public string ModeName => modeName;
    public TimerMode TimerMode => timerMode;
    public float BubbleTimeWorth => bubbleTimeWorth;
    public float TargetTime => targetTime;
    public float WarningThreshold => warningThreshold;
    public int TargetScore => targetScore;
    public int TargetWave => targetWave;
    public float ScoreMultiplier => scoreMultiplier;
    public int BubbleScoreWorth => bubbleScoreWorth;
    public float WaveInterval => waveInterval;
    public float SameColorSpawnChance => sameColorSpawnChance;
    public bool EnableWaveClearFailCondition => enableWaveClearFailCondition;
    public float TimeToCompleteWave => timeToCompleteWave;
    public bool EnableMaxDeathsCondition => enableMaxDeathsCondition;
    public int MaxDeaths => maxDeaths;
    public bool EnableMinWaveScoreCondition => enableMinWaveScoreCondition;
    public int MinScorePerWave => minScorePerWave;
    public bool EnableMaxBubblesCondition => enableMaxBubblesCondition;
    public int MaxActiveBubbles => maxActiveBubbles;
    public bool IsWaveBasedMode => spawnerSelectionMode == SpawnerSelectionMode.WaveBased;
    public bool EnableBubbleScaling => enableBubbleScaling;
    public int MinBubbleAmount 
    {
        get
        {
            if (!enableBubbleScaling) return bubbleRange.x;
        
            // Wave 1 use initial count
            if (GameManager.CurrentWave == 1) return initialBubbleCount;
        
            // Calculate scaled amount based on wave number (subtract 1 since we start from wave 1)
            float scaledAmount = initialBubbleCount * Mathf.Pow(bubbleScaling, GameManager.CurrentWave - 1);
            return Mathf.Min(Mathf.RoundToInt(scaledAmount), maxBubbleCount);
        }
    }

    public int MaxBubbleAmount 
    {
        get
        {
            if (!enableBubbleScaling) return bubbleRange.y;
            return MinBubbleAmount; // When scaling is enabled, use exact scaled amount
        }
    }


    #region Spawner Management // -------------------------------------------------------------------------

    public List<BubbleSpawner> GetActiveSpawners(List<BubbleSpawner> allSpawners, int waveNumber, List<BubbleSpawner> previousSpawners)
    {
        var activeSpawners = new List<BubbleSpawner>();
        int spawnCount;

        switch (spawnerSelectionMode)
        {
            case SpawnerSelectionMode.All:
                activeSpawners.AddRange(allSpawners);
                break;

            case SpawnerSelectionMode.Random:
                spawnCount = Mathf.Min(spawnersPerWave, allSpawners.Count);
                var availableSpawners = new List<BubbleSpawner>(allSpawners);
            
                if (!allowConsecutiveReuse)
                {
                    availableSpawners.RemoveAll(s => previousSpawners.Contains(s));
                }

                while (activeSpawners.Count < spawnCount && availableSpawners.Count > 0)
                {
                    int index = Random.Range(0, availableSpawners.Count);
                    activeSpawners.Add(availableSpawners[index]);
                    availableSpawners.RemoveAt(index);
                }
                break;

            case SpawnerSelectionMode.WaveBased:
                float scaledAmount = initialSpawnersPerWave * Mathf.Pow(spawnerScaling, waveNumber);
                spawnCount = Mathf.RoundToInt(scaledAmount);
                spawnCount = Mathf.Clamp(spawnCount, minSpawnersPerWave, 
                    Mathf.Min(maxSpawnersPerWave, allSpawners.Count));
            
                for (int i = 0; i < spawnCount; i++)
                {
                    int sequentialIndex = (waveNumber + i) % allSpawners.Count;
                    activeSpawners.Add(allSpawners[sequentialIndex]);
                }
                break;
        }

        return activeSpawners;
    }

    #endregion Spawner Management // -------------------------------------------------------------------------
    
    
    #region Game State Management // -------------------------------------------------------------------------

    public bool HasReachedEndCondition(float currentTime, int currentScore, int currentWave)
    {
        // Check if any win condition is met
        bool timeCondition = useTimeForWinCondition && (TimerMode != TimerMode.CountUp || currentTime >= targetTime);
        bool scoreCondition = enableScoreCondition && currentScore >= targetScore;
        bool waveCondition = enableWaveCondition && currentWave >= targetWave;

        // Check for time-based lose condition in countdown mode
        bool timeFailCondition = TimerMode == TimerMode.CountDown && useTimeForLoseCondition && currentTime <= 0;

        // Game ends if we win or lose by time
        return timeFailCondition || scoreCondition || waveCondition || timeCondition;
    }

    public bool IsWinCondition(float currentTime, int currentScore, int currentWave)
    {
        // If we're in countdown mode and time runs out, it's a loss not a win
        if (TimerMode == TimerMode.CountDown && currentTime <= 0 && useTimeForLoseCondition)
        {
            return false;
        }

        // Check each win condition
        bool timeWin = useTimeForWinCondition && (TimerMode != TimerMode.CountUp || currentTime >= targetTime);
        bool scoreWin = enableScoreCondition && currentScore >= targetScore;
        bool waveWin = enableWaveCondition && currentWave >= targetWave;

        return timeWin || scoreWin || waveWin;
    }

    public bool HasReachedLoseCondition(float currentTime, int currentScore, int currentDeaths, 
        int activeBubbles, float waveTimer, int currentWaveScore)
    {
        // Time-based loss (only in countdown mode)
        if (TimerMode == TimerMode.CountDown && useTimeForLoseCondition && currentTime <= 0)
        {
            return true;
        }

        // Other lose conditions
        if (enableWaveClearFailCondition && waveTimer <= 0) return true;
        if (enableMaxDeathsCondition && currentDeaths >= maxDeaths) return true;
        if (enableMinWaveScoreCondition && currentWaveScore < minScorePerWave) return true;
        if (enableMaxBubblesCondition && activeBubbles > maxActiveBubbles) return true;

        return false;
    }
    
    public Color GetTimerColor(float currentTime)
    {
        if (!IsTimerEnabled) return Color.white;

        float timePercentage;
        if (IsTimerCountUp)
        {
            // For count up, we want to warn when we're running out of time to reach target
            timePercentage = (targetTime - currentTime) / targetTime;
        }
        else // CountDown
        {
            timePercentage = currentTime / targetTime;
        }

        return timePercentage <= warningThreshold ? warningTimerColor : normalTimerColor;
    }

    #endregion Game State Management // -------------------------------------------------------------------------

    
    #region Editor Validation // -------------------------------------------------------------------------

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Wave clear settings validation
        if (enableWaveClearFailCondition)
        {
            timeToCompleteWave = Mathf.Max(1f, timeToCompleteWave);
        }

        // Death settings validation
        if (enableMaxDeathsCondition)
        {
            maxDeaths = Mathf.Max(1, maxDeaths);
        }

        // Score settings validation
        if (enableMinWaveScoreCondition)
        {
            minScorePerWave = Mathf.Max(0, minScorePerWave);
        }

        // Bubble settings validation
        if (enableMaxBubblesCondition)
        {
            maxActiveBubbles = Mathf.Max(1, maxActiveBubbles);
        }
        if (!enableBubbleScaling)
        {
            bubbleRange.x = Mathf.Max(0, bubbleRange.x);
            bubbleRange.y = Mathf.Max(bubbleRange.x, bubbleRange.y);
        }
        else
        {
            maxBubbleCount = Mathf.Max(initialBubbleCount, maxBubbleCount);
            bubbleScaling = Mathf.Max(1f, bubbleScaling);
        }
        
    }
#endif

    #endregion Editor Validation // -------------------------------------------------------------------------



}