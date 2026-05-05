using UnityEngine;
using System.Collections.Generic;

public class HeadSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject[] facePrefabs;

    [Header("Spawn Area")]
    public float areaWidth = 9000f;
    public float areaDepth = 9000f;
    public float groundY = 0f;

    [Header("Spawn Settings")]
    public int totalFaces = 400;
    public float minSpacing = 140f;
    public float minScale = 60f;
    public float maxScale = 120f;
    public int maxPlacementAttempts = 50;

    [Header("Restricted Zone (player spawn)")]
    public Vector3 restrictedCenter = Vector3.zero;
    public float restrictedRadius = 200f;

    [Header("Tilt")]
    public float maxTiltX = 12f;
    public float maxTiltZ = 8f;

    private List<Vector3> placedPositions = new();

    void Start()
    {
        SpawnFaces();
    }

    void SpawnFaces()
    {
        if (facePrefabs == null || facePrefabs.Length == 0)
        {
            Debug.LogError("HeadSpawner: No face prefabs assigned.");
            return;
        }

        int spawned = 0;

        for (int i = 0; i < totalFaces; i++)
        {
            bool placed = false;

            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                float x = Random.Range(-areaWidth / 2f, areaWidth / 2f);
                float z = Random.Range(-areaDepth / 2f, areaDepth / 2f);
                Vector3 candidate = new Vector3(x, groundY, z);

                // clear player spawn area
                if (Vector3.Distance(candidate, restrictedCenter) < restrictedRadius)
                    continue;

                // enforce spacing
                bool tooClose = false;
                foreach (var pos in placedPositions)
                {
                    if (Vector3.Distance(candidate, pos) < minSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                // spawn
                GameObject prefab = facePrefabs[Random.Range(0, facePrefabs.Length)];
                float scale = Random.Range(minScale, maxScale);

                // faces look up (180 on X), slight random tilt, full random Y rotation
                Quaternion rot = Quaternion.Euler(
                    180f + Random.Range(-maxTiltX, maxTiltX),
                    Random.Range(0f, 360f),
                    Random.Range(-maxTiltZ, maxTiltZ)
                );

                GameObject face = Instantiate(prefab, candidate, rot, transform);
                face.transform.localScale = new Vector3(scale, scale, scale);

                placedPositions.Add(candidate);
                spawned++;
                placed = true;
                break;
            }

            if (!placed)
                Debug.LogWarning($"HeadSpawner: Could not place face {i}.");
        }

        Debug.Log($"HeadSpawner: Placed {spawned}/{totalFaces} faces.");
    }
}