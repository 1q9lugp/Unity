using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Scales NavMeshAgent speed based on distance to player.
/// Far (>= slowDownRadius) → maxSpeed. Close (<= stopRadius) → minSpeed.
/// Attached at runtime by EnemySpawner.
/// </summary>
public class EnemySpeedController : MonoBehaviour
{
    [HideInInspector] public float maxSpeed       = 45f;
    [HideInInspector] public float minSpeed       = 3.5f;
    [HideInInspector] public float slowDownRadius = 22f;
    [HideInInspector] public float stopRadius     = 2f;

    NavMeshAgent _agent;
    Transform    _player;

    // FIX: Start() and Update() were completely missing — the class was a dead stub.
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;

        // Boot at full speed so the enemy charges hard from spawn.
        if (_agent != null) _agent.speed = maxSpeed;
    }

    void Update()
    {
        // Only runs when on a valid NavMesh; no-op otherwise (LizardEnemy
        // fallback movement handles speed directly via moveSpeed field).
        if (_agent == null || _player == null || !_agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, _player.position);
        float t    = Mathf.InverseLerp(stopRadius, slowDownRadius, dist);
        _agent.speed = Mathf.Lerp(minSpeed, maxSpeed, t);
    }
}