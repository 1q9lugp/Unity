using UnityEngine;
using UnityEngine.AI;

public class LizardEnemy : MonoBehaviour
{
    public int   health    = 3;
    public float moveSpeed = 3.5f;

    Transform    _player;
    NavMeshAgent _agent;
    bool         _useNav;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;

        _agent  = GetComponent<NavMeshAgent>();
        _useNav = _agent != null && _agent.isOnNavMesh;
        if (_useNav)
        {
            _agent.speed        = moveSpeed;
            _agent.angularSpeed = 720f;
            _agent.acceleration = 20f;
        }
    }

    void Update()
    {
        if (_player == null) return;

        var cam = Camera.main;
        if (cam != null) transform.forward = cam.transform.forward;

        if (_useNav && _agent.isOnNavMesh)
        {
            _agent.SetDestination(_player.position);
        }
        else
        {
            Vector3 d = _player.position - transform.position;
            d.y = 0f;
            transform.position += d.normalized * (moveSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health > 0) return;
        var spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null) spawner.OnEnemyDied();
        Destroy(gameObject);
    }
}
