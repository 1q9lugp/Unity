using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Config")]
    public int   maxEnemies  = 100;
    public float spawnRadius = 40f;
    public float eliteChance = 0.2f;

    [Header("Templates")]
    public GameObject gruntTemplate;
    public GameObject eliteTemplate;

    // Track live enemies in a set — O(1) add/remove, no counter drift
    readonly HashSet<LizardEnemy> _live = new HashSet<LizardEnemy>();

    int ActiveCount => _live.Count;

    // ── lifecycle ────────────────────────────────────────────────────────
    void Start()
    {
        StartCoroutine(InitialSpawnRoutine());
    }

    IEnumerator InitialSpawnRoutine()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            Spawn();
            yield return new WaitForSeconds(0.05f);
        }
    }

    // Called by LizardEnemy.Die()
    public void OnEnemyDied()
    {
        // Clean dead entries (destroyed objects become null in the HashSet)
        _live.RemoveWhere(e => e == null);
        Invoke(nameof(Spawn), 1.0f);
    }

    // ── spawning ─────────────────────────────────────────────────────────
    void Spawn()
    {
        // Refresh for any enemies that were destroyed outside of Die()
        _live.RemoveWhere(e => e == null);
        if (ActiveCount >= maxEnemies) return;

        bool isElite = Random.value < eliteChance;
        GameObject tmpl = isElite ? eliteTemplate : gruntTemplate;
        if (tmpl == null) return;

        Vector3 pos = GetSpawnPosition();

        var go    = Instantiate(tmpl, pos, Quaternion.identity);
        go.SetActive(true);

        var enemy = go.GetComponent<LizardEnemy>();
        if (enemy != null)
        {
            // Stats
            enemy.health        = isElite ? 10  : 3;
            enemy.moveSpeed     = isElite ? 2.5f : 3.8f;
            enemy.meleeDamage   = isElite ? 20  : 10;
            enemy.scaleMultiplier = isElite ? 1.4f : 1.0f;

            // Tint
            if (enemy.sprite != null)
                enemy.sprite.color = isElite
                    ? new Color(0.7f, 0.8f, 1f)    // blue-ish elite
                    : new Color(1f,   0.8f, 0.8f);  // red-ish grunt

            _live.Add(enemy);
        }
    }

    // Finds a random point on the NavMesh around the player
    Vector3 GetSpawnPosition()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        Vector3 origin = player != null ? player.transform.position : Vector3.zero;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            float   a      = Random.Range(0f, Mathf.PI * 2f);
            Vector3 candidate = origin + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * spawnRadius;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, 3f, NavMesh.AllAreas))
                return hit.position;
        }

        // Fallback — raw position if NavMesh not found after 10 tries
        float fallbackAngle = Random.Range(0f, Mathf.PI * 2f);
        return origin + new Vector3(Mathf.Cos(fallbackAngle), 0f, Mathf.Sin(fallbackAngle)) * spawnRadius;
    }
}