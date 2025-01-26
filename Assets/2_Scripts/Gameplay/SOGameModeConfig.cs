using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using VInspector;

public enum SpawnerSelectionMode
{
    [Tooltip("Use all available spawners")]
    All,
    [Tooltip("Randomly select a subset of spawners")]
    Random,
    [Tooltip("Use spawners in sequential order")]
    Sequential,
    [Tooltip("Use spawners based on wave number")]
    WaveBased
}

[CreateAssetMenu(fileName = "GameMode", menuName = "SO GameMode/GameMode Configuration")]
public class SOGameModeConfig : ScriptableObject
{
    [Header("Mode Information")] // -------------------------------------------------------------------------
    [Tooltip("Display name for this game mode")]
    [SerializeField] private string modeName = "New GameMode";
    
    [Tooltip("Detailed description of how this game mode works")]
    [SerializeField] [TextArea] private string description;

    [Header("Timer Settings")] // -------------------------------------------------------------------------
    [Tooltip("If true, timer counts up from 0. If false, counts down from targetTime")]
    [SerializeField] private bool isCountUp;
    
    [Header("Score Settings")] // -------------------------------------------------------------------------
    [Tooltip("The worth of a bubble")]
    [SerializeField] private int bubbleScoreWorth = 1;
    [Tooltip("Multiplier applied to all score gains")]
    [SerializeField] private float scoreMultiplier = 1f;

    [Header("Wave Settings")] // -------------------------------------------------------------------------
    [Tooltip("Time between waves in seconds")]
    [SerializeField] private float waveInterval = 5f;
    

    [SerializeField] private bool enableWaveScaling;
    
    [ShowIf("enableWaveScaling")]
    [Tooltip("Starting number of bubbles in the first wave")]
    [SerializeField] private int initialBubblesPerWave = 10;
    
    [Tooltip("How much to increase bubble count each wave (multiplier)")]
    [SerializeField] private float bubbleScaling = 1.2f;
    
    [Tooltip("Maximum number of bubbles allowed in any wave")]
    [SerializeField] private int maxBubblesPerWave = 20;
    
    [Tooltip("Minimum number of bubbles required in any wave")]
    [SerializeField] private int minBubblesPerWave = 5;
    [EndIf]
    
    
    [SerializeField] private bool enableSpawnerScaling;

    [ShowIf("enableSpawnerScaling")]
    [Tooltip("Starting number of spawners in the first wave")]
    [SerializeField] private int initialSpawnersPerWave = 1;

    [Tooltip("How much to increase spawner count each wave (multiplier)")]
    [SerializeField] private float spawnerScaling = 1.2f;

    [Tooltip("Maximum number of spawners allowed in any wave")]
    [SerializeField] private int maxSpawnersPerWave = 5;

    [Tooltip("Minimum number of spawners required in any wave")]
    [SerializeField] private int minSpawnersPerWave = 1;
    [EndIf]

    [Header("Time Boost Settings")] // -------------------------------------------------------------------------
    [Tooltip("Enable time boost rewards for scoring")]
    [SerializeField] private bool enableTimeBoosts;
    
    [ShowIf("enableTimeBoosts")]
    [Tooltip("Amount of time added when scoring threshold is reached")]
    [SerializeField] private float timeBoostAmount = 5f;
    
    [Tooltip("Score required to earn a time boost")]
    [SerializeField] private int scoreRequiredForTimeBoost = 1000;
    [EndIf]

    [Header("Spawner Settings")] // -------------------------------------------------------------------------
    [Tooltip("How spawners are selected for each wave")]
    [SerializeField] private SpawnerSelectionMode spawnerSelectionMode = SpawnerSelectionMode.All;
    
    [ShowIf("IsRandomSpawnerMode")]
    [Tooltip("Number of spawners to use when in Random mode")]
    [SerializeField] [Min(1)] private int spawnersPerWave = 1;
    
    [Tooltip("If false, prevents same spawner from being used in consecutive waves")]
    [SerializeField] private bool allowConsecutiveReuse = true;
    [EndIf]
    
    [Tooltip("Maximum number of bubbles that can spawn in one batch")]
    [SerializeField] [Min(0)] private int maxBubbleAmount = 5;
    
    [Tooltip("Minimum number of bubbles that will spawn in one batch")]
    [SerializeField] [Min(0)] private int minBubbleAmount = 0;
    
    [Tooltip("Chance (0-1) that a bubble will spawn with same color as nearby bubbles")]
    [SerializeField] [Range(0f, 1f)] private float sameColorSpawnChance = 0.6f;

    [Header("Win Conditions")] // -------------------------------------------------------------------------
    [Tooltip("Enable time-based win condition")]
    [SerializeField] private bool useTimeForWinCondition;

    [ShowIf("ShowTargetTime")]
    [Tooltip("Target time to reach in seconds (count up mode)")]
    [SerializeField] private float targetTime = 60f;
    [EndIf]
    
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

    [Header("Lose Conditions")] // -------------------------------------------------------------------------
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
    
    [HideIf("isCountUp")]
    [Tooltip("Enable time-based lose condition (time runs out)")]
    [SerializeField] private bool useTimeForLoseCondition;
    [EndIf]

    // Property getters // -------------------------------------------------------------------------
    public string ModeName => modeName;
    public bool IsCountUp => isCountUp;
    private bool ShowTargetTime() => useTimeForWinCondition && isCountUp;
    public bool UseTimeForWinCondition => useTimeForWinCondition;
    public bool UseTimeForLoseCondition => !isCountUp && useTimeForLoseCondition;
    public float TargetTime => isCountUp ? targetTime : 0f;
    public int TargetScore => targetScore;
    public int TargetWave => targetWave;
    public float ScoreMultiplier => scoreMultiplier;
    public int BubbleScoreWorth => bubbleScoreWorth;
    public float WaveInterval => waveInterval;
    public int MinBubbleAmount => minBubbleAmount;
    public int MaxBubbleAmount => maxBubbleAmount;
    public float SameColorSpawnChance => sameColorSpawnChance;
    public bool EnableTimeBoosts => enableTimeBoosts;
    public float TimeBoostAmount => timeBoostAmount;
    public int ScoreRequiredForTimeBoost => scoreRequiredForTimeBoost;
    public bool EnableWaveClearFailCondition => enableWaveClearFailCondition;
    public float TimeToCompleteWave => timeToCompleteWave;
    public bool EnableMaxDeathsCondition => enableMaxDeathsCondition;
    public int MaxDeaths => maxDeaths;
    public bool EnableMinWaveScoreCondition => enableMinWaveScoreCondition;
    public int MinScorePerWave => minScorePerWave;
    public bool EnableMaxBubblesCondition => enableMaxBubblesCondition;
    public int MaxActiveBubbles => maxActiveBubbles;

    private bool IsRandomSpawnerMode() => spawnerSelectionMode == SpawnerSelectionMode.Random;

    // Spawner Management // -------------------------------------------------------------------------
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
                if (enableSpawnerScaling)
                {
                    float scaledAmount = initialSpawnersPerWave * Mathf.Pow(spawnerScaling, waveNumber);
                    spawnCount = Mathf.RoundToInt(scaledAmount);
                    spawnCount = Mathf.Clamp(spawnCount, minSpawnersPerWave, 
                        Mathf.Min(maxSpawnersPerWave, allSpawners.Count));
                }
                else
                {
                    spawnCount = Mathf.Min(spawnersPerWave, allSpawners.Count);
                }

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

            case SpawnerSelectionMode.Sequential:
            case SpawnerSelectionMode.WaveBased:
                if (enableSpawnerScaling)
                {
                    float scaledAmount = initialSpawnersPerWave * Mathf.Pow(spawnerScaling, waveNumber);
                    spawnCount = Mathf.RoundToInt(scaledAmount);
                    spawnCount = Mathf.Clamp(spawnCount, minSpawnersPerWave, 
                        Mathf.Min(maxSpawnersPerWave, allSpawners.Count));
                    
                    for (int i = 0; i < spawnCount; i++)
                    {
                        int sequentialIndex = (waveNumber + i) % allSpawners.Count;
                        activeSpawners.Add(allSpawners[sequentialIndex]);
                    }
                }
                else
                {
                    int sequentialIndex = waveNumber % allSpawners.Count;
                    activeSpawners.Add(allSpawners[sequentialIndex]);
                }
                break;
        }

        return activeSpawners;
    }

    // Wave Management // -------------------------------------------------------------------------
    public int GetBubblesForWave(int waveNumber)
    {
        if (!enableWaveScaling)
        {
            return Random.Range(minBubblesPerWave, maxBubblesPerWave + 1);
        }

        float scaledAmount = initialBubblesPerWave * Mathf.Pow(bubbleScaling, waveNumber);
        int bubbleCount = Mathf.RoundToInt(scaledAmount);
        return Mathf.Clamp(bubbleCount, minBubblesPerWave, maxBubblesPerWave);
    }

    // Game State Management // -------------------------------------------------------------------------
    public bool HasReachedEndCondition(float currentTime, int currentScore, int currentWave)
    {
        // Check if any win condition is met
        bool timeCondition = useTimeForWinCondition && (!isCountUp || currentTime >= targetTime);
        bool scoreCondition = enableScoreCondition && currentScore >= targetScore;
        bool waveCondition = enableWaveCondition && currentWave >= targetWave;

        // Check for time-based lose condition in countdown mode
        bool timeFailCondition = !isCountUp && useTimeForLoseCondition && currentTime <= 0;

        // Game ends if we win or lose by time
        return timeFailCondition || scoreCondition || waveCondition || timeCondition;
    }

    public bool IsWinCondition(float currentTime, int currentScore, int currentWave)
    {
        // If we're in countdown mode and time runs out, it's a loss not a win
        if (!isCountUp && currentTime <= 0 && useTimeForLoseCondition)
        {
            return false;
        }

        // Check each win condition
        bool timeWin = useTimeForWinCondition && (!isCountUp || currentTime >= targetTime);
        bool scoreWin = enableScoreCondition && currentScore >= targetScore;
        bool waveWin = enableWaveCondition && currentWave >= targetWave;

        return timeWin || scoreWin || waveWin;
    }

    public bool HasReachedLoseCondition(float currentTime, int currentScore, int currentDeaths, 
        int activeBubbles, float waveTimer, int currentWaveScore)
    {
        // Time-based loss (only in countdown mode)
        if (!isCountUp && useTimeForLoseCondition && currentTime <= 0)
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

    // Editor Validation // -------------------------------------------------------------------------
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

        // Wave scaling validation
        if (enableWaveScaling)
        {
            initialBubblesPerWave = Mathf.Clamp(initialBubblesPerWave, minBubblesPerWave, maxBubblesPerWave);
            bubbleScaling = Mathf.Max(1f, bubbleScaling);
        }

        // Spawner scaling validation
        if (enableSpawnerScaling)
        {
            initialSpawnersPerWave = Mathf.Clamp(initialSpawnersPerWave, minSpawnersPerWave, maxSpawnersPerWave);
            spawnerScaling = Mathf.Max(1f, spawnerScaling);
        }
    }
#endif
}