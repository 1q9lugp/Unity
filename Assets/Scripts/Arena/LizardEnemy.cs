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

    [Header("Separation")]
    public LayerMask enemyLayerMask;    

    enum State { Chase, Melee }
    State _state = State.Chase;

    Transform    _player;
    FPSController _fpsController;
    NavMeshAgent _agent;
    bool         _useNav;
    bool         _isFlashing;
    bool         _isDead;

    float   _nextMelee;
    float   _nextSepCheck;
    Vector3 _cachedSeparation;

    float _bobOffset;
    float _baseScale;

    ArenaHUD     _hud;
    EnemySpawner _spawner;

    // ── New Setup Method ──
    public void Setup(bool isElite)
    {
        // This handles the procedural stats based on the Spawner's roll
        health          = isElite ? 10 : 3;
        moveSpeed       = isElite ? 2.5f : 3.8f;
        meleeDamage     = isElite ? 20 : 10;
        scaleMultiplier = isElite ? 1.4f : 1.0f;
        _baseScale      = scaleMultiplier;

        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        
        if (sprite != null)
        {
            sprite.color = isElite 
                ? new Color(0.7f, 0.8f, 1f)    // Blue-ish
                : new Color(1f, 0.8f, 0.8f);   // Red-ish
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
            _player       = p.transform;
            _fpsController = p.GetComponent<FPSController>();
        }

        _agent  = GetComponent<NavMeshAgent>();
        _useNav = _agent != null && _agent.isOnNavMesh;

        if (_useNav)
        {
            _agent.speed        = moveSpeed;
            _agent.angularSpeed = 720f;
            _agent.acceleration = 20f;
        }

        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();

        _bobOffset  = Random.Range(0f, 10f);
        _baseScale  = scaleMultiplier;
    }

    void Update()
    {
        if (_player == null || _isDead) return;

        float dist = Vector3.Distance(transform.position, _player.position);
        _state = dist <= meleeRange ? State.Melee : State.Chase;

        switch (_state)
        {
            case State.Chase: HandleMovement();    break;
            case State.Melee: TryMeleeAttack();    break;
        }

        HandleProceduralAnimation();
        HandleBillboarding();
    }

    void HandleMovement()
    {
        Vector3 direction = (_player.position - transform.position).normalized;
        direction.y = 0;

        if (Time.time > _nextSepCheck)
        {
            _nextSepCheck    = Time.time + 0.1f;
            _cachedSeparation = Vector3.zero;

            Collider[] neighbors = Physics.OverlapSphere(transform.position, 1.5f, enemyLayerMask);
            foreach (var n in neighbors)
            {
                if (n.gameObject != gameObject)
                    _cachedSeparation += (transform.position - n.transform.position);
            }
        }

        if (_useNav && _agent.isOnNavMesh)
        {
            _agent.SetDestination(_player.position);
        }
        else
        {
            Vector3 finalMove = (direction + _cachedSeparation.normalized * 0.5f).normalized;
            transform.position += finalMove * (moveSpeed * Time.deltaTime);
        }

        if (direction != Vector3.zero)
            transform.forward = direction;
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

        float pulse = Mathf.Sin(Time.time * 2f + _bobOffset) * 0.05f;
        float s     = _baseScale + pulse;
        sprite.transform.localScale = new Vector3(s, s, 1f);

        float bob = Mathf.Sin(Time.time * 5f + _bobOffset) * 0.1f;
        sprite.transform.localPosition = new Vector3(0, 0.9f + bob, 0);

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
        
        // Corrected: Passing the gameObject to the spawner
        if (_spawner != null)
            _spawner.OnEnemyDied(this.gameObject);

        StartCoroutine(DeathAnim());
    }

    IEnumerator DeathAnim()
    {
        float elapsed = 0f;
        float duration = 0.15f;
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