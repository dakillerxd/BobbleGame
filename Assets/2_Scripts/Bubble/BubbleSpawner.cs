using UnityEngine;
using VInspector;
using System.Collections.Generic;

public enum SpawnShape
{
    Cube,
    Sphere,
    Ring,
    Circle,
    Rectangle,
    Line
}

public class BubbleSpawner : MonoBehaviour
{
    [Foldout("Spawn Settings")]
    [SerializeField] private SpawnShape spawnShape = SpawnShape.Sphere;
    [SerializeField] private bool randomizeInShape = true;
    [SerializeField] [Min(0)] private int maxBubbleAmount = 5;
    [SerializeField] [Min(0)] private int minBubbleAmount = 0;
    [SerializeField] private float minDistanceBetweenBubbles = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float sameColorSpawnChance = 0.6f;
    [EndFoldout]
    
    [Foldout("Shape Parameters")]
    [Header("Cube Settings")]
    [SerializeField] private Vector3 cubeSize = new Vector3(5f, 5f, 5f);
    
    [Header("Sphere Settings")]
    [SerializeField] private float sphereRadius = 3f;
    
    [Header("Ring Settings")]
    [SerializeField] private float ringRadius = 3f;
    [SerializeField] private float ringThickness = 0.5f;
    
    [Header("Circle Settings")]
    [SerializeField] private float circleRadius = 3f;
    
    [Header("Rectangle Settings")]
    [SerializeField] private Vector2 rectangleSize = new Vector2(5f, 3f);
    
    [Header("Line Settings")]
    [SerializeField] private float lineLength = 5f;
    [EndFoldout]
    
    [Foldout("References")]
    [SerializeField] private SOBubbleManager bubbleManager;
    [EndFoldout]
    
    [SerializeField] [ReadOnly] private List<BubbleObject> spawnedBubbles = new List<BubbleObject>();

    [Button] 
    public void SpawnBubbles()
    {
        SpawnBubbles(true);
    }

    [Button]
    public void AddBubbles()
    {
        SpawnBubbles(false);
    }
    
    [Button] 
    public void ClearBubbles()
    {
        // Destroy all bubbles from the list
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

        if (clearExisting)
        {
            ClearBubbles();
        }
        else
        {
            // Clean up any destroyed bubbles from the list
            spawnedBubbles.RemoveAll(bubble => bubble == null);
            
            // Find any orphaned bubbles in children and destroy them
            BubbleObject[] childBubbles = GetComponentsInChildren<BubbleObject>();
            foreach (BubbleObject bubble in childBubbles)
            {
                if (!spawnedBubbles.Contains(bubble))
                {
                    Destroy(bubble.gameObject);
                }
            }
        }

        Vector3[] newPositions = GenerateClusterPositions(GetExistingBubblePositions());
        List<(Vector3 position, Material color)> newBubbleData = new List<(Vector3, Material)>();
        
        foreach (Vector3 position in newPositions)
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

    private Material GetValidColor(Vector3 position, List<(Vector3 position, Material color)> newBubbles)
    {
        List<Material> availableColors = new List<Material>(bubbleManager.BubbleColors);
        
        // If random value is less than sameColorSpawnChance, skip color validation
        if (Random.value < sameColorSpawnChance)
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
        Vector3[] positions = new Vector3[spawnedBubbles.Count];
        
        for (int i = 0; i < spawnedBubbles.Count; i++)
        {
            positions[i] = spawnedBubbles[i].transform.position;
        }
        
        return positions;
    }

    private Vector3 GetRandomPositionForShape(float progressT)
    {
        switch (spawnShape)
        {
            case SpawnShape.Cube:
                if (randomizeInShape)
                {
                    return transform.position + new Vector3(
                        Random.Range(-cubeSize.x / 2f, cubeSize.x / 2f),
                        Random.Range(-cubeSize.y / 2f, cubeSize.y / 2f),
                        Random.Range(-cubeSize.z / 2f, cubeSize.z / 2f)
                    );
                }
                else
                {
                    // Organized pattern: fill cube layer by layer
                    float x = Mathf.Lerp(-cubeSize.x / 2f, cubeSize.x / 2f, (progressT * 13.0f) % 1f);
                    float y = Mathf.Lerp(-cubeSize.y / 2f, cubeSize.y / 2f, ((progressT * 13.0f) * 7.0f) % 1f);
                    float z = Mathf.Lerp(-cubeSize.z / 2f, cubeSize.z / 2f, ((progressT * 13.0f) * 11.0f) % 1f);
                    return transform.position + new Vector3(x, y, z);
                }

            case SpawnShape.Sphere:
                if (randomizeInShape)
                {
                    // Random point in sphere using spherical coordinates
                    float theta = Random.Range(0f, 2f * Mathf.PI);
                    float phi = Random.Range(0f, Mathf.PI);
                    float radius = sphereRadius * Random.value;
                    
                    float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
                    float y = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
                    float z = radius * Mathf.Cos(phi);
                    return transform.position + new Vector3(x, y, z);
                }
                else
                {
                    // Organized pattern: spiral from center outward
                    float theta = progressT * Mathf.PI * 20f;
                    float phi = progressT * Mathf.PI;
                    float radius = sphereRadius * progressT;
                    
                    float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
                    float y = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
                    float z = radius * Mathf.Cos(phi);
                    return transform.position + new Vector3(x, y, z);
                }

            case SpawnShape.Ring:
                float angle;
                float ringR;
                if (randomizeInShape)
                {
                    angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    ringR = ringRadius - Random.Range(0f, ringThickness);
                }
                else
                {
                    angle = progressT * Mathf.PI * 2f;
                    ringR = ringRadius - (ringThickness * 0.5f);
                }
                return transform.position + new Vector3(
                    Mathf.Cos(angle) * ringR,
                    Mathf.Sin(angle) * ringR,
                    0
                );

            case SpawnShape.Circle:
                if (randomizeInShape)
                {
                    Vector2 randomDisk = Random.insideUnitCircle * circleRadius;
                    return transform.position + new Vector3(randomDisk.x, randomDisk.y, 0);
                }
                else
                {
                    // Spiral pattern from center
                    float spiral = progressT * Mathf.PI * 10f;
                    float r = progressT * circleRadius;
                    return transform.position + new Vector3(
                        Mathf.Cos(spiral) * r,
                        Mathf.Sin(spiral) * r,
                        0
                    );
                }

            case SpawnShape.Rectangle:
                if (randomizeInShape)
                {
                    float randomX = Random.Range(-rectangleSize.x / 2f, rectangleSize.x / 2f);
                    float randomY = Random.Range(-rectangleSize.y / 2f, rectangleSize.y / 2f);
                    return transform.position + new Vector3(randomX, randomY, 0);
                }
                else
                {
                    // Snake pattern
                    float x = Mathf.Lerp(-rectangleSize.x / 2f, rectangleSize.x / 2f, (progressT * 7.0f) % 1f);
                    float y = Mathf.Lerp(-rectangleSize.y / 2f, rectangleSize.y / 2f, Mathf.Floor(progressT * 7.0f) / 6f);
                    return transform.position + new Vector3(x, y, 0);
                }

            case SpawnShape.Line:
                if (randomizeInShape)
                {
                    float randomX = Random.Range(-lineLength / 2f, lineLength / 2f);
                    return transform.position + new Vector3(randomX, 0, 0);
                }
                else
                {
                    float x = Mathf.Lerp(-lineLength / 2f, lineLength / 2f, progressT);
                    return transform.position + new Vector3(x, 0, 0);
                }

            default:
                return transform.position;
        }
    }

    private Vector3[] GenerateClusterPositions(Vector3[] existingPositions)
    {
        int validMinBubbles = Mathf.Min(minBubbleAmount, maxBubbleAmount);
        int validMaxBubbles = Mathf.Max(minBubbleAmount, maxBubbleAmount);
        int targetBubbleCount = Random.Range(validMinBubbles, validMaxBubbles + 1);
        
        Vector3[] positions = new Vector3[targetBubbleCount];
        int currentSpawned = 0;
        int maxAttempts = 1000;
        int attemptsPerPosition = 50;

        // Calculate the total number of bubbles (existing + new)
        int existingCount = existingPositions?.Length ?? 0;
        int totalBubbles = existingCount + targetBubbleCount;

        while (currentSpawned < targetBubbleCount && maxAttempts > 0)
        {
            int currentAttempts = attemptsPerPosition;
            bool positionFound = false;

            while (currentAttempts > 0 && !positionFound)
            {
                // Calculate progress based on total bubbles
                float progress = (float)(existingCount + currentSpawned) / totalBubbles;
                Vector3 randomPosition = GetRandomPositionForShape(progress);
                
                bool isValidPosition = true;
                
                // Check against new positions
                for (int i = 0; i < currentSpawned; i++)
                {
                    if (Vector3.Distance(randomPosition, positions[i]) < minDistanceBetweenBubbles)
                    {
                        isValidPosition = false;
                        break;
                    }
                }
                
                // Check against existing positions
                if (isValidPosition && existingPositions != null)
                {
                    foreach (Vector3 existingPos in existingPositions)
                    {
                        if (Vector3.Distance(randomPosition, existingPos) < minDistanceBetweenBubbles)
                        {
                            isValidPosition = false;
                            break;
                        }
                    }
                }

                if (isValidPosition)
                {
                    positions[currentSpawned] = randomPosition;
                    currentSpawned++;
                    positionFound = true;
                }

                currentAttempts--;
            }

            maxAttempts--;
        }

        if (currentSpawned < targetBubbleCount)
        {
            System.Array.Resize(ref positions, currentSpawned);
        }

        return positions;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        minBubbleAmount = Mathf.Min(minBubbleAmount, maxBubbleAmount);
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

            case SpawnShape.Ring:
                Gizmos.DrawWireSphere(transform.position, ringRadius);
                Gizmos.DrawWireSphere(transform.position, ringRadius - ringThickness);
                break;

            case SpawnShape.Circle:
                Gizmos.DrawWireSphere(transform.position, circleRadius);
                break;

            case SpawnShape.Rectangle:
                Gizmos.DrawWireCube(transform.position, new Vector3(rectangleSize.x, rectangleSize.y, 0));
                break;

            case SpawnShape.Line:
                Vector3 startPoint = transform.position + Vector3.left * (lineLength / 2f);
                Vector3 endPoint = transform.position + Vector3.right * (lineLength / 2f);
                Gizmos.DrawLine(startPoint, endPoint);
                break;
        }
    }
#endif
}