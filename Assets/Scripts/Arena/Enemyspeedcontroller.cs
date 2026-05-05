using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Automatically adjusts this enemy's NavMeshAgent speed every frame
/// based on its distance from the player.
///
///   Far  (>= slowDownRadius) → maxSpeed  (enormous)
///   Close (<= stopRadius)    → minSpeed  (sluggish / stalking pace)
///   Between                  → smooth linear interpolation
///
/// Attached at runtime by EnemySpawner – no manual setup required.
/// </summary>
public class EnemySpeedController : MonoBehaviour
{
    [HideInInspector] public float maxSpeed       = 45f;
    [HideInInspector] public float minSpeed       = 3.5f;
    [HideInInspector] public float slowDownRadius = 22f;
    [HideInInspector] public float stopRadius     = 2f;

    NavMeshAgent _agent;
    Transform    _player;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;

        // Begin at full speed so the enemy charges in fast.
        if (_agent != null) _agent.speed = maxSpeed;
    }

    void Update()
    {
        if (_agent == null || _player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);

        // t = 0 when at stopRadius (close)  → minSpeed
        // t = 1 when at slowDownRadius (far) → maxSpeed
        float t = Mathf.InverseLerp(stopRadius, slowDownRadius, dist);

        _agent.speed = Mathf.Lerp(minSpeed, maxSpeed, t);
    }
}