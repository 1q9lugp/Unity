using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class LizardEnemy : MonoBehaviour
{
    [Header("Stats")]
    public int   health    = 3;
    public float moveSpeed = 3.5f;

    [Header("Combat")]
    public int   meleeDamage   = 10;
    public float meleeRange    = 1.2f;
    public float meleeCooldown = 0.8f;

    [Header("Visual")]
    public SpriteRenderer sprite;
    public float scaleMultiplier = 1f;

    enum State { Chase, Melee }
    State _state = State.Chase;

    Transform     _player;
    FPSController _fpsController;
    NavMeshAgent  _agent;
    bool          _useNav;
    bool          _isFlashing;
    bool          _isDead;

    float   _nextMelee;
    float   _nextDestUpdate;   // staggered path refresh
    float   _destInterval;     // per-enemy random interval

    float _bobOffset;
    float _baseScale;

    ArenaHUD     _hud;
    EnemySpawner _spawner;

    public void Setup(bool isElite)
    {
        health          = isElite ? 10 : 3;
        meleeDamage     = isElite ? 20 : 10;
        scaleMultiplier = isElite ? 1.4f : 1.0f;
        _baseScale      = scaleMultiplier;

        // Random speed variance ±15% so the horde doesn't arrive as one wall
        float variance  = Random.Range(0.85f, 1.15f);
        moveSpeed       = (isElite ? 2.5f : 3.8f) * variance;

        transform.localScale = Vector3.one * scaleMultiplier;

        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = isElite
                ? new Color(0.7f, 0.8f, 1f)
                : new Color(1f, 0.8f, 0.8f);
        }

        if (_agent != null) _agent.speed = moveSpeed;
    }

    void Start()
    {
        _hud     = FindObjectOfType<ArenaHUD>();
        _spawner = FindObjectOfType<EnemySpawner>();

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            _player        = p.transform;
            _fpsController = p.GetComponent<FPSController>();
        }

        _agent  = GetComponent<NavMeshAgent>();
        _useNav = _agent != null && _agent.isOnNavMesh;

        if (_useNav)
        {
            _agent.speed               = moveSpeed;
            _agent.angularSpeed        = 360f;
            _agent.acceleration        = 12f;
            _agent.autoBraking         = false;
            // Let NavMeshAgent handle rotation — no manual override
            _agent.updateRotation      = true;
            _agent.updateUpAxis        = false;
        }

        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();

        _bobOffset    = Random.Range(0f, 10f);
        _baseScale    = scaleMultiplier;
        // Stagger path refresh: each enemy gets a slightly different interval
        // so 100 agents don't all call SetDestination on the same frame
        _destInterval    = Random.Range(0.2f, 0.35f);
        _nextDestUpdate  = Time.time + Random.Range(0f, _destInterval);
    }

    void Update()
    {
        if (_player == null || _isDead) return;

        float dist = Vector3.Distance(transform.position, _player.position);
        _state = dist <= meleeRange ? State.Melee : State.Chase;

        switch (_state)
        {
            case State.Chase: HandleMovement();  break;
            case State.Melee: TryMeleeAttack();  break;
        }

        HandleProceduralAnimation();
        HandleBillboarding();
    }

    void HandleMovement()
    {
        if (_useNav && _agent.isOnNavMesh)
        {
            // Staggered SetDestination — not every frame
            if (Time.time >= _nextDestUpdate)
            {
                _agent.SetDestination(_player.position);
                _nextDestUpdate = Time.time + _destInterval;
            }

            // Separation: nudge away from nearby enemies using NavMesh velocity
            Vector3 sep = Vector3.zero;
            Collider[] neighbors = Physics.OverlapSphere(transform.position, 1.2f);
            foreach (var n in neighbors)
            {
                if (n.gameObject == gameObject) continue;
                if (n.GetComponent<LizardEnemy>() == null) continue;
                sep += (transform.position - n.transform.position);
            }

            if (sep != Vector3.zero)
            {
                // Blend separation into desired velocity
                Vector3 desired = _agent.desiredVelocity + sep.normalized * 1.5f;
                _agent.velocity = Vector3.ClampMagnitude(desired, moveSpeed);
            }
        }
        else
        {
            // Fallback: direct movement without NavMesh
            Vector3 dir = (_player.position - transform.position);
            dir.y = 0f;
            transform.position += dir.normalized * (moveSpeed * Time.deltaTime);
            if (dir != Vector3.zero)
                transform.forward = Vector3.Lerp(transform.forward, dir.normalized, Time.deltaTime * 8f);
        }
    }

    void TryMeleeAttack()
    {
        if (Time.time < _nextMelee) return;
        _nextMelee = Time.time + meleeCooldown;
        _fpsController?.TakeDamage(meleeDamage);
    }

    public void TakeDamage(int dmg)
    {
        if (_isDead) return;
        health -= dmg;
        if (!_isFlashing && sprite != null) StartCoroutine(HitFlash());
        if (health <= 0) Die();
    }

    void HandleBillboarding()
    {
        if (Camera.main == null || sprite == null) return;
        sprite.transform.rotation = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f);
    }

    void HandleProceduralAnimation()
    {
        if (sprite == null) return;

        float halfH = sprite.sprite != null
            ? sprite.sprite.bounds.extents.y * _baseScale
            : 1.0f;

        float pulse = Mathf.Sin(Time.time * 2f + _bobOffset) * 0.05f;
        float s     = _baseScale + pulse;
        sprite.transform.localScale = new Vector3(s, s, 1f);

        float bob = Mathf.Sin(Time.time * 5f + _bobOffset) * 0.1f;
        sprite.transform.localPosition = new Vector3(0, halfH + bob, 0);

        float tilt = Mathf.Sin(Time.time * 10f + _bobOffset) * 4f;
        sprite.transform.localRotation = Quaternion.Euler(0, 0, tilt);
    }

    IEnumerator HitFlash()
    {
        _isFlashing = true;
        Color orig = sprite.color;
        sprite.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        sprite.color = orig;
        _isFlashing  = false;
    }

    void Die()
    {
        _isDead = true;
        if (_agent != null) _agent.enabled = false;
        _hud?.AddKill();
        if (_spawner != null) _spawner.OnEnemyDied(this.gameObject);
        StartCoroutine(DeathAnim());
    }

    IEnumerator DeathAnim()
    {
        float elapsed   = 0f;
        float duration  = 0.15f;
        Vector3 startScale = transform.localScale;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
            yield return null;
        }
        Destroy(gameObject);
    }
}