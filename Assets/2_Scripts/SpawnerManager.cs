using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager
{
    private List<BubbleSpawner> allSpawners;
    private List<BubbleSpawner> activeSpawners;
    private List<BubbleSpawner> previousWaveSpawners;
    private int currentSpawnerIndex;
    private System.Random random;

    public SpawnerManager()
    {
        activeSpawners = new List<BubbleSpawner>();
        previousWaveSpawners = new List<BubbleSpawner>();
        random = new System.Random();
    }

    public void Initialize(List<BubbleSpawner> spawners)
    {
        allSpawners = new List<BubbleSpawner>(spawners);
        currentSpawnerIndex = 0;
    }

    public List<BubbleSpawner> GetSpawnersForWave(int waveNumber, SpawnerSettings settings)
    {
        previousWaveSpawners = new List<BubbleSpawner>(activeSpawners);
        activeSpawners.Clear();

        switch (settings.spawnerSelectionMode)
        {
            case SpawnerSelectionMode.All:
                activeSpawners.AddRange(allSpawners);
                break;

            case SpawnerSelectionMode.Random:
                int spawnCount = Mathf.Min(settings.spawnersPerWave, allSpawners.Count);
                var availableSpawners = new List<BubbleSpawner>(allSpawners);
                
                if (!settings.allowConsecutiveReuse)
                {
                    availableSpawners.RemoveAll(s => previousWaveSpawners.Contains(s));
                }

                while (activeSpawners.Count < spawnCount && availableSpawners.Count > 0)
                {
                    int index = random.Next(availableSpawners.Count);
                    activeSpawners.Add(availableSpawners[index]);
                    availableSpawners.RemoveAt(index);
                }
                break;

            case SpawnerSelectionMode.Sequential:
                activeSpawners.Add(allSpawners[currentSpawnerIndex]);
                currentSpawnerIndex = (currentSpawnerIndex + 1) % allSpawners.Count;
                break;

            case SpawnerSelectionMode.WaveBased:
                int spawnerIndex = waveNumber % allSpawners.Count;
                activeSpawners.Add(allSpawners[spawnerIndex]);
                break;
        }

        return activeSpawners;
    }
}