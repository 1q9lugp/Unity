using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Scale")]
    public float enemyScale = 1f;

    [Header("Spawn Config")]
    public GameObject enemyPrefab;
    public int   maxEnemies  = 30;
    public float eliteChance = 0.2f;

    [Header("Spawn Distance")]
    [Tooltip("Enemies never spawn closer than this to the player (units).")]
    public float minSpawnRadius = 200f;   // was 50
    [Tooltip("Enemies never spawn further than this from the player (units).")]
    public float maxSpawnRadius = 350f;   // was 80

    [Header("POV Arc")]
    [Tooltip("Total horizontal arc (degrees) centred on camera forward in which enemies can appear. " +
             "160 = ±80° — covers the full player FOV plus a little peripheral. " +
             "Set to 360 to allow all directions.")]
    public float spawnArcDegrees = 160f;

    [Header("Attempt Budget")]
    [Tooltip("How many random positions to try before falling back to a guaranteed safe one.")]
    public int spawnAttempts = 40;

    [Header("Enemy Spacing")]
    [Tooltip("No new enemy spawns within this radius of an existing enemy.")]
    public float minEnemySpacing = 4f;

    [Header("Speed")]
    public float maxSpeed       = 45f;
    public float minSpeed       = 3.5f;
    public float slowDownRadius = 22f;
    public float stopRadius     = 2f;

    // ── Internal ──────────────────────────────────────────────────────────
    readonly HashSet<GameObject> _live = new HashSet<GameObject>();
    Transform _playerTransform;
    int       _pendingSpawns = 0;

    // Ground-finding constants.
    // Cast from this far ABOVE player Y so we go through the open arena
    // top without reaching the distant safety-net plane (Zem at Y≈-370).
    const float GroundCastUp   = 10f;
    const float GroundCastDown = 25f;

    // ─────────────────────────────────────────────────────────────────────
    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            _playerTransform = p.transform;
        else
            Debug.LogError("[EnemySpawner] No GameObject tagged 'Player' found.");

        StartCoroutine(InitialSpawn());
    }

    IEnumerator InitialSpawn()
    {
        yield return new WaitForSeconds(1.5f);
        for (int i = 0; i < maxEnemies; i++)
        {
            DoSpawn();
            yield return new WaitForSeconds(0.12f);
        }
    }

    public void OnEnemyDied(GameObject enemy)
    {
        if (enemy != null) _live.Remove(enemy);
        _live.RemoveWhere(e => e == null);
        int deficit = maxEnemies - _live.Count - _pendingSpawns;
        if (deficit > 0)
        {
            _pendingSpawns++;
            Invoke(nameof(SpawnFromQueue), Random.Range(2f, 5f));
        }
    }

    void SpawnFromQueue()
    {
        _pendingSpawns = Mathf.Max(0, _pendingSpawns - 1);
        DoSpawn();
    }

    void OnDestroy() => CancelInvoke(nameof(SpawnFromQueue));

    // ── Core spawn ────────────────────────────────────────────────────────
    void DoSpawn()
    {
        _live.RemoveWhere(e => e == null);
        if (_live.Count >= maxEnemies) return;
        if (enemyPrefab == null || _playerTransform == null) return;

        Vector3 pos = GetSpawnPos();
        if (pos == Vector3.zero) return;

        GameObject go = Instantiate(enemyPrefab, pos, Quaternion.identity);

        var agent = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        LizardEnemy lizard = go.GetComponent<LizardEnemy>();
        if (lizard != null)
            lizard.Setup(Random.value < eliteChance, enemyScale);

        EnemySpeedController sc = go.GetComponent<EnemySpeedController>();
        if (sc == null) sc = go.AddComponent<EnemySpeedController>();
        sc.maxSpeed       = maxSpeed;
        sc.minSpeed       = minSpeed;
        sc.slowDownRadius = slowDownRadius;
        sc.stopRadius     = stopRadius;

        _live.Add(go);
    }

    // ── Position finding ──────────────────────────────────────────────────
    Vector3 GetSpawnPos()
    {
        Camera   cam     = Camera.main;
        Vector3  forward = cam != null ? cam.transform.forward : _playerTransform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        forward.Normalize();

        float baseAngle = Mathf.Atan2(forward.x, forward.z);
        float halfArc   = spawnArcDegrees * 0.5f * Mathf.Deg2Rad;

        for (int i = 0; i < spawnAttempts; i++)
        {
            float angle = baseAngle + Random.Range(-halfArc, halfArc);
            float dist  = Random.Range(minSpawnRadius, maxSpawnRadius);

            float cx = _playerTransform.position.x + Mathf.Sin(angle) * dist;
            float cz = _playerTransform.position.z + Mathf.Cos(angle) * dist;

            float groundY;
            if (!TryGetGroundY(cx, cz, out groundY)) continue;

            Vector3 spawnPos = new Vector3(cx, groundY + 1.5f, cz);

            if (EnemyNearby(spawnPos, minEnemySpacing)) continue;

            float xzDist = new Vector2(cx - _playerTransform.position.x,
                                        cz - _playerTransform.position.z).magnitude;
            if (xzDist < minSpawnRadius) continue;

            return spawnPos;
        }

        // Fallback
        float fa  = baseAngle + Random.Range(-halfArc, halfArc);
        float fx  = _playerTransform.position.x + Mathf.Sin(fa) * (minSpawnRadius + 5f);
        float fz  = _playerTransform.position.z + Mathf.Cos(fa) * (minSpawnRadius + 5f);
        float fgy = _playerTransform.position.y;
        TryGetGroundY(fx, fz, out fgy);

        return new Vector3(fx, fgy + 1.5f, fz);
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    bool TryGetGroundY(float x, float z, out float groundY)
    {
        groundY = _playerTransform.position.y;
        Vector3 rayOrigin = new Vector3(x, _playerTransform.position.y + GroundCastUp, z);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, GroundCastDown))
        {
            if (hit.collider.gameObject.layer == 3) return false;
            if (hit.collider.isTrigger)             return false;

            groundY = hit.point.y;
            return true;
        }

        return false;
    }

    bool EnemyNearby(Vector3 centre, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(centre, radius);
        foreach (Collider c in hits)
            if (c.GetComponent<LizardEnemy>() != null)
                return true;
        return false;
    }
}