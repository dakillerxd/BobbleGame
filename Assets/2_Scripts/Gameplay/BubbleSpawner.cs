using UnityEngine;
using VInspector;
using System.Collections.Generic;
using System.Linq;

public enum SpawnShape
{
    Cube,
    Sphere
}

public class BubbleSpawner : MonoBehaviour
{
    [Foldout("Spawn Settings")] 
    [SerializeField] private SpawnShape spawnShape = SpawnShape.Cube;
    [SerializeField] private float minDistanceBetweenBubbles = 0.5f;
    [SerializeField] private float maxDistanceBetweenBubbles = 1.0f;
    [EndFoldout] 
    
    [Foldout("Shape Parameters")] 
    [Header("Cube Settings")] 
    [SerializeField] private Vector3 cubeSize = new Vector3(15f, 5f, 15f);

    [Header("Sphere Settings")] 
    [SerializeField] private float sphereRadius = 3f;
    [EndFoldout] 

    [Foldout("References")] 
    [SerializeField] private SOBubbleManager bubbleManager;
    [EndFoldout] 
    
    [SerializeField] [ReadOnly] private List<BubbleObject> spawnedBubbles = new List<BubbleObject>();

    public void SpawnBubbles()
    {
        SpawnBubbles(true);
    }

    public void AddBubbles()
    {
        SpawnBubbles(false);
    }

    [Button]
    public void ClearBubbles()
    {
        foreach (BubbleObject bubble in spawnedBubbles)
        {
            if (bubble)
            {
                if (!Application.isPlaying)
                    DestroyImmediate(bubble.gameObject);
                else
                    Destroy(bubble.gameObject);
            }
        }

        spawnedBubbles.Clear();
    }

    private void SpawnBubbles(bool clearExisting)
    {
        if (!bubbleManager)
        {
            Debug.LogError("Bubble Manager not assigned!");
            return;
        }

        if (SessionManager.CurrentGameMode == null)
        {
            Debug.LogError("No active game mode found!");
            return;
        }

        if (clearExisting)
        {
            ClearBubbles();
        }
        else
        {
            spawnedBubbles.RemoveAll(bubble => bubble == null);
            BubbleObject[] childBubbles = GetComponentsInChildren<BubbleObject>();
            foreach (BubbleObject bubble in childBubbles)
            {
                if (!spawnedBubbles.Contains(bubble))
                {
                    if (Application.isPlaying)
                        Destroy(bubble.gameObject);
                    else
                        DestroyImmediate(bubble.gameObject);
                }
            }
        }

        Vector3[] positions = GenerateConnectedPositions(GetExistingBubblePositions());
        List<(Vector3 position, Material color)> newBubbleData = new List<(Vector3, Material)>();

        foreach (Vector3 position in positions)
        {
            Material validColor = GetValidColor(position, newBubbleData);
            
            BubbleObject bubble = Instantiate(
                bubbleManager.BubbleObjectPrefab,
                position,
                Quaternion.identity,
                transform
            );

            bubble.SetBubbleColor(validColor);
            spawnedBubbles.Add(bubble);
            newBubbleData.Add((position, validColor));
        }
    }
    
    
    private Vector3[] GenerateConnectedPositions(Vector3[] existingPositions)
    {
        int targetBubbleCount = Random.Range(
            SessionManager.CurrentGameMode.MinBubbleAmount, 
            SessionManager.CurrentGameMode.MaxBubbleAmount + 1
        );
        
        List<Vector3> positions = new List<Vector3>();
    
        if (existingPositions != null && existingPositions.Length > 0)
        {
            positions.AddRange(existingPositions);
        }
        else
        {
            positions.Add(GetRandomPositionInShape());
        }

        int maxAttempts = 1000;
        while (positions.Count < targetBubbleCount + (existingPositions?.Length ?? 0) && maxAttempts > 0)
        {
            Vector3 anchorPosition = positions[Random.Range(0, positions.Count)];
            Vector3? newPosition = FindValidConnectedPosition(anchorPosition, positions);
        
            if (newPosition.HasValue)
            {
                positions.Add(newPosition.Value);
            }
        
            maxAttempts--;
        }

        if (existingPositions != null)
        {
            return positions.Skip(existingPositions.Length).ToArray();
        }
    
        return positions.ToArray();
    }

    private Vector3? FindValidConnectedPosition(Vector3 anchorPosition, List<Vector3> existingPositions)
    {
        for (int i = 0; i < 50; i++)
        {
            // Generate a random direction
            Vector3 randomDirection = Random.onUnitSphere;
            float distance = Random.Range(minDistanceBetweenBubbles, maxDistanceBetweenBubbles);
            Vector3 candidatePosition = anchorPosition + randomDirection * distance;

            // Check if the position is within the chosen shape
            if (!IsPositionInShape(candidatePosition))
            {
                continue;
            }

            // Check if the position is valid relative to other bubbles
            bool isValid = true;
            foreach (Vector3 existingPos in existingPositions)
            {
                if (Vector3.Distance(candidatePosition, existingPos) < minDistanceBetweenBubbles)
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                return candidatePosition;
            }
        }

        return null;
    }

    private Vector3 GetRandomPositionInShape()
    {
        return GetRandomPositionForShape(Random.value);
    }

    private bool IsPositionInShape(Vector3 position)
    {
        Vector3 localPosition = position - transform.position;
        
        switch (spawnShape)
        {
            case SpawnShape.Cube:
                return Mathf.Abs(localPosition.x) <= cubeSize.x / 2f &&
                       Mathf.Abs(localPosition.y) <= cubeSize.y / 2f &&
                       Mathf.Abs(localPosition.z) <= cubeSize.z / 2f;

            case SpawnShape.Sphere:
                return localPosition.magnitude <= sphereRadius;

            default:
                return false;
        }
    }

    private Vector3 GetRandomPositionForShape(float progressT)
    {
        switch (spawnShape)
        {
            case SpawnShape.Cube:
                return transform.position + new Vector3(
                    Random.Range(-cubeSize.x / 2f, cubeSize.x / 2f),
                    Random.Range(-cubeSize.y / 2f, cubeSize.y / 2f),
                    Random.Range(-cubeSize.z / 2f, cubeSize.z / 2f)
                );

            case SpawnShape.Sphere:
                float theta = Random.Range(0f, 2f * Mathf.PI);
                float phi = Random.Range(0f, Mathf.PI);
                float radius = sphereRadius * Random.value;
                return transform.position + new Vector3(
                    radius * Mathf.Sin(phi) * Mathf.Cos(theta),
                    radius * Mathf.Sin(phi) * Mathf.Sin(theta),
                    radius * Mathf.Cos(phi)
                );

            default:
                return transform.position;
        }
    }

    private Material GetValidColor(Vector3 position, List<(Vector3 position, Material color)> newBubbles)
    {
        List<Material> availableColors = new List<Material>(bubbleManager.BubbleColors);

        if (Random.value < SessionManager.CurrentGameMode.SameColorSpawnChance)
        {
            return availableColors[Random.Range(0, availableColors.Count)];
        }

        List<Material> invalidColors = new List<Material>();

        // Check newly spawned bubbles in this batch
        foreach ((Vector3 bubblePos, Material bubbleColor) in newBubbles)
        {
            if (Vector3.Distance(position, bubblePos) <= minDistanceBetweenBubbles * 1.1f)
            {
                int sameColorCount = 0;
                foreach ((Vector3 otherPos, Material otherColor) in newBubbles)
                {
                    if (otherColor == bubbleColor &&
                        Vector3.Distance(position, otherPos) <= minDistanceBetweenBubbles * 1.1f)
                    {
                        sameColorCount++;
                        if (sameColorCount >= 1)
                        {
                            invalidColors.Add(bubbleColor);
                            break;
                        }
                    }
                }
            }
        }

        // Check existing bubbles
        foreach (BubbleObject existingBubble in spawnedBubbles)
        {
            if (!existingBubble) continue;

            if (Vector3.Distance(position, existingBubble.transform.position) <= minDistanceBetweenBubbles * 1.1f)
            {
                int sameColorCount = 0;
                foreach (BubbleObject otherBubble in spawnedBubbles)
                {
                    if (!otherBubble) continue;

                    if (otherBubble.BubbleColor() == existingBubble.BubbleColor() &&
                        Vector3.Distance(position, otherBubble.transform.position) <= minDistanceBetweenBubbles * 1.1f)
                    {
                        sameColorCount++;
                        if (sameColorCount >= 1)
                        {
                            invalidColors.Add(existingBubble.BubbleColor());
                            break;
                        }
                    }
                }
            }
        }

        // Remove invalid colors
        foreach (Material invalidColor in invalidColors)
        {
            availableColors.Remove(invalidColor);
        }

        // Fallback if all colors are invalid
        if (availableColors.Count == 0)
        {
            availableColors = new List<Material>(bubbleManager.BubbleColors);
        }

        return availableColors[Random.Range(0, availableColors.Count)];
    }

    private Vector3[] GetExistingBubblePositions()
    {
        spawnedBubbles.RemoveAll(bubble => bubble == null);
        return spawnedBubbles.Select(bubble => bubble.transform.position).ToArray();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        minDistanceBetweenBubbles = Mathf.Max(0.1f, minDistanceBetweenBubbles);
        maxDistanceBetweenBubbles = Mathf.Max(minDistanceBetweenBubbles, maxDistanceBetweenBubbles);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        switch (spawnShape)
        {
            case SpawnShape.Cube:
                Gizmos.DrawWireCube(transform.position, cubeSize);
                break;

            case SpawnShape.Sphere:
                Gizmos.DrawWireSphere(transform.position, sphereRadius);
                break;
        }
    }
#endif
}