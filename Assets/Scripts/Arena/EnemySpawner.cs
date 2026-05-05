using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawns enemies in a circular ring at half the NavMesh-edge distance from
/// the player. Each enemy starts with enormous speed that scales down
/// smoothly as it approaches the player (handled by EnemySpeedController).
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // ── Visual / scale ────────────────────────────────────────────────────
    [Header("Scale")]
    public float enemyScale = 1f;

    // ── Spawn config ──────────────────────────────────────────────────────
    [Header("Spawn Config")]
    public GameObject enemyPrefab;
    public int   maxEnemies  = 30;
    public float eliteChance = 0.2f;

    [Tooltip("Minimum guaranteed spawn distance if the NavMesh edge is very close.")]
    public float minSpawnRadius = 40f;

    [Tooltip("How many angle slots to divide the circle into when looking for a free spot.")]
    public int spawnCircleResolution = 24;

    [Tooltip("Radius of the overlap-sphere check used to reject blocked spawn points.")]
    public float colliderCheckRadius = 1.2f;

    // ── Speed settings (fed to EnemySpeedController) ─────────────────────
    [Header("Speed (Distance-Based)")]
    [Tooltip("Speed when the enemy is at or beyond slowDownRadius from the player.")]
    public float maxSpeed = 45f;

    [Tooltip("Speed when the enemy reaches the player's feet (stopRadius).")]
    public float minSpeed = 3.5f;

    [Tooltip("Distance at which slowdown begins.")]
    public float slowDownRadius = 22f;

    [Tooltip("Distance at which speed reaches minSpeed (melee range).")]
    public float stopRadius = 2f;

    // ── Internal ──────────────────────────────────────────────────────────
    readonly HashSet<GameObject> _live = new HashSet<GameObject>();

    // ─────────────────────────────────────────────────────────────────────
    void Start() => StartCoroutine(InitialSpawn());

    IEnumerator InitialSpawn()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < maxEnemies; i++)
        {
            Spawn();
            yield return new WaitForSeconds(0.02f);
        }
    }

    // Called by LizardEnemy (or death handler) when an enemy dies.
    public void OnEnemyDied(GameObject enemy)
    {
        if (enemy != null) _live.Remove(enemy);
        Invoke(nameof(Spawn), Random.Range(0.5f, 1.5f));
    }

    // ── Core spawn ────────────────────────────────────────────────────────
    void Spawn()
    {
        _live.RemoveWhere(e => e == null);
        if (_live.Count >= maxEnemies || enemyPrefab == null) return;

        Vector3 pos = GetSpawnPosition();

        GameObject go = Instantiate(enemyPrefab, pos, Quaternion.identity);

        // Warp onto NavMesh if not already snapped.
        NavMeshAgent agent = go.GetComponent<NavMeshAgent>();
        if (agent != null && !agent.isOnNavMesh)
            if (NavMesh.SamplePosition(pos, out NavMeshHit warpHit, 20f, NavMesh.AllAreas))
                agent.Warp(warpHit.position);

        // Elite chance.
        LizardEnemy script = go.GetComponent<LizardEnemy>();
        if (script != null) script.Setup(Random.value < eliteChance);

        // Attach dynamic speed controller.
        EnemySpeedController speedCtrl = go.AddComponent<EnemySpeedController>();
        speedCtrl.maxSpeed      = maxSpeed;
        speedCtrl.minSpeed      = minSpeed;
        speedCtrl.slowDownRadius = slowDownRadius;
        speedCtrl.stopRadius    = stopRadius;

        go.transform.localScale *= enemyScale;
        _live.Add(go);
    }

    // ── Spawn-position logic ──────────────────────────────────────────────
    /// <summary>
    /// Iterates evenly-spaced angles around the player in a circle.
    /// For each direction it casts a NavMesh ray to the edge, then tries
    /// to place the enemy at half that edge distance (clamped to
    /// minSpawnRadius). Skips positions blocked by physics colliders.
    /// Falls back to a simple radius sample if nothing clean is found.
    /// </summary>
    Vector3 GetSpawnPosition()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        Vector3   origin = player != null ? player.position : Vector3.zero;

        // Shuffle starting angle so the ring doesn't look identical every wave.
        float angleOffset = Random.Range(0f, Mathf.PI * 2f);

        for (int i = 0; i < spawnCircleResolution; i++)
        {
            float   angle = angleOffset + (i / (float)spawnCircleResolution) * Mathf.PI * 2f;
            Vector3 dir   = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            // ① Find where the NavMesh ends in this direction.
            float edgeDist = FindNavMeshEdgeDistance(origin, dir);

            // ② Spawn at half of that distance, but at least minSpawnRadius.
            float spawnDist = Mathf.Max(edgeDist * 0.5f, minSpawnRadius);

            Vector3 candidate = origin + dir * spawnDist;

            // ③ Project onto actual ground.
            candidate.y = SampleGroundY(candidate, origin.y);

            // ④ Snap to nearest NavMesh surface.
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit nh, 15f, NavMesh.AllAreas))
                continue;

            // ⑤ Reject positions that overlap a physics collider (walls, props).
            if (Physics.CheckSphere(nh.position + Vector3.up * 0.5f, colliderCheckRadius))
                continue;

            return nh.position;
        }

        // Fallback: random point on a plain circle at minSpawnRadius.
        return FallbackSpawnPosition(origin);
    }

    /// <summary>
    /// Uses NavMesh.Raycast (which stops at holes/edges) to measure how far
    /// the navmesh extends in the given horizontal direction.
    /// </summary>
    float FindNavMeshEdgeDistance(Vector3 origin, Vector3 direction)
    {
        const float maxSearch = 600f;
        Vector3 dest = origin + direction * maxSearch;

        // NavMesh.Raycast returns true when it hits a boundary or hole.
        if (NavMesh.Raycast(origin, dest, out NavMeshHit hit, NavMesh.AllAreas))
            return Mathf.Max(hit.distance, minSpawnRadius * 2f);

        return maxSearch; // Open navmesh – use the full search distance.
    }

    float SampleGroundY(Vector3 xzPos, float fallbackY)
    {
        if (Physics.Raycast(new Vector3(xzPos.x, 500f, xzPos.z),
                            Vector3.down, out RaycastHit rh, 1000f))
            return rh.point.y;
        return fallbackY;
    }

    Vector3 FallbackSpawnPosition(Vector3 origin)
    {
        float   angle     = Random.Range(0f, Mathf.PI * 2f);
        Vector3 candidate = origin + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * minSpawnRadius;
        candidate.y = SampleGroundY(candidate, origin.y);

        if (NavMesh.SamplePosition(candidate, out NavMeshHit nh, 20f, NavMesh.AllAreas))
            return nh.position;

        return candidate;
    }
}