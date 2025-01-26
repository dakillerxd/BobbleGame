using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum EndConditionType
{
    [Tooltip("Game ends when time limit is reached")]
    TimeBased,
    [Tooltip("Game ends when target score is reached")]
    ScoreBased,
    [Tooltip("Game ends after specific wave count")]
    WaveBased
}

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
    [Header("Mode Information")]
    [Tooltip("Display name for this game mode")]
    [SerializeField] private string modeName = "New GameMode";
    
    [Tooltip("Detailed description of how this game mode works")]
    [SerializeField] [TextArea] private string description;

    [Header("End Condition Settings")]
    [Tooltip("How the game determines when to end")]
    [SerializeField] private EndConditionType endConditionType;
    
    [Tooltip("If true, time counts up to target. If false, counts down from target")]
    [SerializeField] private bool isCountUp;
    
    [Tooltip("Time limit in seconds. Used differently based on isCountUp setting")]
    [SerializeField] private float targetTime = 60f;
    
    [Tooltip("Score required to win in score-based mode")]
    [SerializeField] private int targetScore = 1000;
    
    [Tooltip("Number of waves to complete in wave-based mode")]
    [SerializeField] private int targetWave = 10;

    [Header("Score Settings")]
    [Tooltip("Multiplier applied to all score gains")]
    [SerializeField] private float scoreMultiplier = 1f;

    [Header("Wave Settings")]
    [Tooltip("Time between waves in seconds")]
    [SerializeField] private float waveInterval = 30f;
    
    [Tooltip("Starting number of bubbles in the first wave")]
    [SerializeField] private int initialBubblesPerWave = 10;
    
    [Tooltip("How much to increase bubble count each wave (multiplier)")]
    [SerializeField] private float difficultyScaling = 1.2f;
    
    [Tooltip("Maximum number of bubbles allowed in any wave")]
    [SerializeField] private int maxBubblesPerWave = 20;
    
    [Tooltip("Minimum number of bubbles required in any wave")]
    [SerializeField] private int minBubblesPerWave = 5;

    [Header("Time Settings")]
    [Tooltip("Enable time boost rewards for scoring")]
    [SerializeField] private bool enableTimeBoosts;
    
    [Tooltip("Amount of time added when scoring threshold is reached")]
    [SerializeField] private float timeBoostAmount = 5f;
    
    [Tooltip("Score required to earn a time boost")]
    [SerializeField] private int scoreRequiredForTimeBoost = 1000;

    [Header("Spawner Settings")]
    [Tooltip("How spawners are selected for each wave")]
    [SerializeField] private SpawnerSelectionMode spawnerSelectionMode = SpawnerSelectionMode.All;
    
    [Tooltip("Number of spawners to use when in Random mode")]
    [SerializeField] [Min(1)] private int spawnersPerWave = 1;
    
    [Tooltip("If false, prevents same spawner from being used in consecutive waves")]
    [SerializeField] private bool allowConsecutiveReuse = true;
    
    [Tooltip("Maximum number of bubbles that can spawn in one batch")]
    [SerializeField] [Min(0)] private int maxBubbleAmount = 5;
    
    [Tooltip("Minimum number of bubbles that will spawn in one batch")]
    [SerializeField] [Min(0)] private int minBubbleAmount = 0;
    
    [Tooltip("Chance (0-1) that a bubble will spawn with same color as nearby bubbles")]
    [SerializeField] [Range(0f, 1f)] private float sameColorSpawnChance = 0.6f;

    // Property getters
    public string ModeName => modeName;
    public EndConditionType EndConditionType => endConditionType;
    public bool IsCountUp => isCountUp;
    public float TargetTime => targetTime;
    public int TargetScore => targetScore;
    public int TargetWave => targetWave;
    public float ScoreMultiplier => scoreMultiplier;
    public float WaveInterval => waveInterval;
    public int MinBubbleAmount => minBubbleAmount;
    public int MaxBubbleAmount => maxBubbleAmount;
    public float SameColorSpawnChance => sameColorSpawnChance;
    public bool EnableTimeBoosts => enableTimeBoosts;
    public float TimeBoostAmount => timeBoostAmount;
    public int ScoreRequiredForTimeBoost => scoreRequiredForTimeBoost;

    public List<BubbleSpawner> GetActiveSpawners(List<BubbleSpawner> allSpawners, int waveNumber, List<BubbleSpawner> previousSpawners)
    {
        var activeSpawners = new List<BubbleSpawner>();

        switch (spawnerSelectionMode)
        {
            case SpawnerSelectionMode.All:
                activeSpawners.AddRange(allSpawners);
                break;

            case SpawnerSelectionMode.Random:
                int spawnCount = Mathf.Min(spawnersPerWave, allSpawners.Count);
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
                int sequentialIndex = waveNumber % allSpawners.Count;
                activeSpawners.Add(allSpawners[sequentialIndex]);
                break;

            case SpawnerSelectionMode.WaveBased:
                int waveBasedIndex = waveNumber % allSpawners.Count;
                activeSpawners.Add(allSpawners[waveBasedIndex]);
                break;
        }

        return activeSpawners;
    }

    public bool HasReachedEndCondition(float currentTime, int currentScore, int currentWave)
    {
        switch (endConditionType)
        {
            case EndConditionType.TimeBased:
                return isCountUp ? currentTime >= targetTime : currentTime <= 0;
            
            case EndConditionType.ScoreBased:
                return currentScore >= targetScore;
            
            case EndConditionType.WaveBased:
                return currentWave >= targetWave;
            
            default:
                return false;
        }
    }

    public bool IsWinCondition(float currentTime, int currentScore, int currentWave)
    {
        switch (endConditionType)
        {
            case EndConditionType.TimeBased:
                return isCountUp ? currentTime >= targetTime : currentTime > 0;
            
            case EndConditionType.ScoreBased:
                return currentScore >= targetScore;
            
            case EndConditionType.WaveBased:
                return currentWave >= targetWave;
            
            default:
                return false;
        }
    }
}