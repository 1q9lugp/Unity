using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Config")]
    public GameObject enemyPrefab; 
    public int maxEnemies = 100;
    public float spawnRadius = 40f;
    public float eliteChance = 0.2f;

    private readonly HashSet<GameObject> _liveEnemies = new HashSet<GameObject>();

    void Start()
    {
        StartCoroutine(InitialSpawnRoutine());
    }

    IEnumerator InitialSpawnRoutine()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            Spawn();
            yield return new WaitForSeconds(0.02f); 
        }
    }

    public void OnEnemyDied(GameObject enemy)
    {
        if (enemy != null)
            _liveEnemies.Remove(enemy);
            
        Invoke(nameof(Spawn), Random.Range(0.5f, 1.5f));
    }

    void Spawn()
    {
        _liveEnemies.RemoveWhere(e => e == null);
        if (_liveEnemies.Count >= maxEnemies) return;

        Vector3 pos = GetSpawnPosition();
        GameObject go = Instantiate(enemyPrefab, pos, Quaternion.identity);
        
        LizardEnemy script = go.GetComponent<LizardEnemy>();
        if (script != null)
        {
            bool isElite = Random.value < eliteChance;
            script.Setup(isElite); 
        }

        _liveEnemies.Add(go);
    }

    Vector3 GetSpawnPosition()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        Vector3 origin = player != null ? player.position : Vector3.zero;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * spawnRadius;
        Vector3 candidate = origin + offset;

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return candidate; 
    }
}