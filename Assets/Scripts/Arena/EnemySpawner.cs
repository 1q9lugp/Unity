using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Config")]
    public GameObject enemyPrefab;
    public int   maxEnemies  = 100;
    public float spawnRadius = 40f;
    public float eliteChance = 0.2f;

    readonly HashSet<GameObject> _live = new HashSet<GameObject>();

    void Start()
    {
        // Wait 1 second for NavMesh build to fully commit before spawning
        StartCoroutine(InitialSpawn());
    }

    IEnumerator InitialSpawn()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < maxEnemies; i++)
        {
            Spawn();
            yield return new WaitForSeconds(0.02f);
        }
    }

    public void OnEnemyDied(GameObject enemy)
    {
        if (enemy != null) _live.Remove(enemy);
        Invoke(nameof(Spawn), Random.Range(0.5f, 1.5f));
    }

    void Spawn()
    {
        _live.RemoveWhere(e => e == null);
        if (_live.Count >= maxEnemies || enemyPrefab == null) return;

        Vector3 pos = GetSpawnPosition();
        var go      = Instantiate(enemyPrefab, pos, Quaternion.identity);

        // If agent didn't land on NavMesh, warp it to the nearest valid point
        var agent = go.GetComponent<NavMeshAgent>();
        if (agent != null && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(pos, out NavMeshHit warp, 20f, NavMesh.AllAreas))
                agent.Warp(warp.position);
        }

        var script = go.GetComponent<LizardEnemy>();
        if (script != null) script.Setup(Random.value < eliteChance);

        _live.Add(go);
    }

    Vector3 GetSpawnPosition()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        Vector3   origin = player != null ? player.position : Vector3.zero;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float x     = origin.x + Mathf.Cos(angle) * spawnRadius;
        float z     = origin.z + Mathf.Sin(angle) * spawnRadius;

        // Raycast from sky to find ground — ignores player flight altitude
        float groundY = 0f;
        if (Physics.Raycast(new Vector3(x, 500f, z), Vector3.down, out RaycastHit rh, 1000f))
            groundY = rh.point.y;

        Vector3 candidate = new Vector3(x, groundY, z);

        if (NavMesh.SamplePosition(candidate, out NavMeshHit nh, 15f, NavMesh.AllAreas))
            return nh.position;

        return candidate;
    }
}