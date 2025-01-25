using UnityEngine;

public enum GameModeType
{
    [Tooltip("Time-based gameplay where player must score points before time runs out")]
    TimeBased,
    [Tooltip("Wave-based gameplay where player must clear bubbles within time limit")]
    WaveBased,
    [Tooltip("Target score gameplay where player must reach a score goal within time limit")]
    TargetScore
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

[System.Serializable]
public class SpawnerSettings
{
    [Header("Spawner Selection")]
    [Space(10)]
    [Tooltip("How spawners should be selected for each wave")]
    public SpawnerSelectionMode spawnerSelectionMode = SpawnerSelectionMode.All;
    
    [Tooltip("Number of spawners to use in Random mode")]
    [Min(1)] public int spawnersPerWave = 1;
    
    [Tooltip("Allow reusing spawners in consecutive waves")]
    public bool allowConsecutiveReuse = true;

    [Header("Bubble Spawn Settings")]
    [Space(10)]
    [Tooltip("Maximum number of bubbles that can spawn in a single batch")]
    [Min(0)] public int maxBubbleAmount = 5;
    
    [Tooltip("Minimum number of bubbles that will spawn in a single batch")]
    [Min(0)] public int minBubbleAmount = 0;
    
    [Tooltip("Chance (0-1) that a bubble will spawn with the same color as nearby bubbles")]
    [Range(0f, 1f)] public float sameColorSpawnChance = 0.6f;
}

[System.Serializable]
public class CommonGameSettings
{
    [Header("Time Settings")]
    [Space(10)]
    [Tooltip("Base time in seconds for the game session")]
    public float baseTime = 60f;

    [Header("Score Settings")]
    [Space(10)]
    [Tooltip("Multiplier applied to all score gains")]
    public float scoreMultiplier = 1f;
    
    [Header("Combo System")]
    [Space(10)]
    [Tooltip("Enable or disable combo system")]
    public bool allowCombos = true;
    
    [Tooltip("Time window in seconds to maintain combo")]
    public float comboTime = 3f;
    
    [Tooltip("Maximum combo multiplier that can be achieved")]
    [Range(1f, 10f)] public float maxCombo = 5f;

    [Header("Wave Settings")]
    [Space(10)]
    [Tooltip("Time interval between waves in seconds")]
    public float waveInterval = 30f;
    
    [Header("Spawning Configuration")]
    [Space(10)]
    public SpawnerSettings spawnerSettings = new SpawnerSettings();
}

[System.Serializable]
public class GameModeSettings
{
    [Header("Common Settings")]
    public CommonGameSettings commonSettings = new CommonGameSettings();
}

[System.Serializable]
public class TimeModeSettings : GameModeSettings
{
    [Header("Time Mode Configuration")]
    [Space(10)]
    
    [Tooltip("Time added when reaching score threshold")]
    [Min(0f)] public float timeBoostAmount = 5f;
    
    [Tooltip("Score required to earn a time boost")]
    [Min(100)] public int scoreRequiredForTimeBoost = 1000;
}

[System.Serializable]
public class WaveModeSettings : GameModeSettings
{
    [Header("Wave Configuration")]
    [Space(10)]
    
    [Tooltip("Time limit for clearing each wave")]
    [Min(10f)] public float waveTimeout = 30f;
    
    [Tooltip("Initial number of bubbles in first wave")]
    [Min(1)] public int initialBubblesPerWave = 10;
    
    [Header("Difficulty Scaling")]
    [Space(5)]
    
    [Tooltip("Multiplier for increasing bubble count each wave")]
    [Range(1f, 2f)] public float difficultyScaling = 1.2f;
    
    [Tooltip("Maximum bubbles allowed in any wave")]
    [Min(5)] public int maxBubblesPerWave = 20;
    
    [Tooltip("Minimum bubbles required in any wave")]
    [Min(1)] public int minBubblesPerWave = 5;
}

[System.Serializable]
public class TargetScoreModeSettings : GameModeSettings
{
    [Header("Target Score Configuration")]
    [Space(10)]
    
    [Tooltip("Score required to win")]
    [Min(1000)] public int targetScore = 10000;
    
    [Tooltip("Time limit to reach target score")]
    [Min(30f)] public float timeLimit = 180f;
}