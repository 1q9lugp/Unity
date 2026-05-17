using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class LizardEnemy : MonoBehaviour
{
    // ── Tunable stats (overridden by EnemySpawner at spawn time) ──────────
    [Header("Stats")]
    public int   health    = 3;

    [Header("Speed")]
    public float maxSpeed       = 30f;   // speed when far
    public float minSpeed       = 7f;    // speed when at melee range
    public float slowDownRadius = 28f;   // distance at which max speed kicks in
    public float stopRadius     = 1.8f;  // melee range

    [Header("Movement Feel")]
    public float acceleration    = 22f;   // high = snappy, low = floaty
    public float turnSpeed       = 500f;  // deg/s; high for aggressive pivot
    public float wanderStrength  = 0.18f; // tight zigzag angle (radians)
    public float wanderFrequency = 1.4f;  // fast oscillation = unpredictable

    [Header("Sprint Bursts")]
    [Tooltip("Chance per second that an enemy outside Charge range triggers a sprint burst.")]
    public float sprintChance    = 0.35f;
    [Tooltip("Speed multiplier during a sprint burst.")]
    public float sprintMult      = 1.7f;
    [Tooltip("Duration of each sprint burst in seconds.")]
    public float sprintDuration  = 0.45f;

    [Header("Combat")]
    public int   meleeDamage   = 10;
    public float meleeCooldown = 0.55f;

    [Header("Visual")]
    public SpriteRenderer sprite;
    public float scaleMultiplier = 1f;

    // ── Internal state ────────────────────────────────────────────────────
    enum Phase { Stalk, Charge, Melee }
    Phase _phase = Phase.Stalk;

    Transform     _player;
    FPSController _fpsController;
    NavMeshAgent  _agent;
    bool          _useNav;
    bool          _isFlashing;
    bool          _isDead;

    Vector3 _smoothVel = Vector3.zero;
    float   _velY      = 0f;
    bool    _isGrounded;

    float _wanderPhase;   // unique per enemy so no two oscillate in sync
    float _bobOffset;
    float _baseScale;

    // Sprint burst state
    bool  _sprinting;
    float _sprintTimer;

    float _nextMelee;

    ArenaHUD     _hud;
    EnemySpawner _spawner;

   // ── Setup ─────────────────────────────────────────────────────────────
// baseScale is now passed in from EnemySpawner so the Inspector field works.
// Elite is exactly 2× the base size; the CapsuleCollider scales with the
// root transform so hitbox and visual stay in sync.
public void Setup(bool isElite, float baseScale = 1f)
{
    health          = isElite ? 12 : 3;
    meleeDamage     = isElite ? 22 : 10;

    // Elite = 2× basic — collider and sprite both follow root transform scale
    scaleMultiplier = isElite ? baseScale * 2f : baseScale;
    _baseScale      = scaleMultiplier;

    // Per-enemy speed variance keeps a crowd from moving like a single unit
    float v = Random.Range(0.80f, 1.20f);
    maxSpeed *= v;
    minSpeed *= v;

    // Elites are bigger and meaner
    if (isElite)
    {
        maxSpeed      *= 1.20f;
        acceleration  *= 1.15f;
        meleeCooldown *= 0.75f;
    }

    // Apply scale to ROOT — CapsuleCollider inherits this automatically
    transform.localScale = Vector3.one * scaleMultiplier;

    if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
    if (sprite != null)
        sprite.color = isElite
            ? new Color(0.50f, 0.72f, 1.00f)
            : new Color(1.00f, 0.72f, 0.72f);
}
    // ── Lifecycle ─────────────────────────────────────────────────────────
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
            _agent.speed        = maxSpeed;
            _agent.angularSpeed = 0f;
            _agent.acceleration = acceleration;
            _agent.autoBraking  = false;
            _agent.updateUpAxis = false;
        }

        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();

        _wanderPhase = Random.Range(0f, Mathf.PI * 2f);
        _bobOffset   = Random.Range(0f, 10f);
        _baseScale   = scaleMultiplier;
    }

    void Update()
    {
        if (_player == null || _isDead) return;

        float distXZ = HorizontalDist(_player.position);

        // Phase transitions
        if      (distXZ <= stopRadius)               _phase = Phase.Melee;
        else if (distXZ <= slowDownRadius * 0.55f)   _phase = Phase.Charge;
        else                                         _phase = Phase.Stalk;

        switch (_phase)
        {
            case Phase.Stalk:
            case Phase.Charge: HandleMovement(distXZ); break;
            case Phase.Melee:  HandleMelee();           break;
        }

        HandleProceduralAnimation();
        HandleBillboarding();
    }

    // ── Movement ──────────────────────────────────────────────────────────
    void HandleMovement(float distXZ)
    {
        // ── Gravity ──────────────────────────────────────────────────────
        _isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.5f, Vector3.down, 1.2f);
        if (_isGrounded) { if (_velY < 0f) _velY = 0f; }
        else
        {
            _velY += Physics.gravity.y * Time.deltaTime;
            _velY  = Mathf.Max(_velY, -40f);
        }
        transform.position += Vector3.up * (_velY * Time.deltaTime);

        // ── Target speed ─────────────────────────────────────────────────
        float t           = Mathf.InverseLerp(stopRadius, slowDownRadius, distXZ);
        float targetSpeed = Mathf.Lerp(minSpeed, maxSpeed, t);

        // Charge: direct, fast close-in burst
        if (_phase == Phase.Charge) targetSpeed *= 1.45f;

        // Sprint burst: random lunges during Stalk to break up constant speed
        if (_phase == Phase.Stalk)
        {
            if (_sprinting)
            {
                _sprintTimer -= Time.deltaTime;
                if (_sprintTimer <= 0f) _sprinting = false;
                else targetSpeed *= sprintMult;
            }
            else if (Random.value < sprintChance * Time.deltaTime)
            {
                _sprinting   = true;
                _sprintTimer = sprintDuration;
            }
        }
        else
        {
            // Not in Stalk — cancel any pending sprint
            _sprinting = false;
        }

        // ── Chase direction ───────────────────────────────────────────────
        Vector3 toPlayer = _player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.001f) return;
        Vector3 chaseDir = toPlayer.normalized;

        // ── Wander (tight high-frequency zigzag) ─────────────────────────
        // Shrinks to zero as enemy enters Charge so close-in approach is direct
        float wanderFade  = Mathf.Clamp01((distXZ - stopRadius)
                            / (slowDownRadius * 0.35f));
        float wanderAngle = Mathf.Sin(Time.time * wanderFrequency + _wanderPhase)
                            * wanderStrength * wanderFade * Mathf.Rad2Deg;
        Vector3 wanderDir = Quaternion.Euler(0f, wanderAngle, 0f) * chaseDir;

        // ── Separation: inverse-distance, low weight ──────────────────────
        Vector3 sep = ComputeSeparation(1.5f);

        Vector3 desiredDir = wanderDir + sep * 0.5f;
        if (desiredDir.sqrMagnitude < 0.001f) desiredDir = chaseDir;
        desiredDir.Normalize();

        // ── Smooth velocity ───────────────────────────────────────────────
        if (_useNav && _agent.isOnNavMesh)
        {
            _agent.SetDestination(_player.position);
            _agent.speed = targetSpeed;
            Vector3 desired = _agent.desiredVelocity.normalized * targetSpeed + sep * 0.5f;
            _agent.velocity = Vector3.ClampMagnitude(desired, targetSpeed);
        }
        else
        {
            Vector3 targetVel = desiredDir * targetSpeed;
            _smoothVel = Vector3.MoveTowards(
                _smoothVel, targetVel, acceleration * Time.deltaTime);
            transform.position += _smoothVel * Time.deltaTime;
        }

        // ── Rotation ─────────────────────────────────────────────────────
        if (chaseDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(chaseDir);
            transform.rotation   = Quaternion.RotateTowards(
                transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
    }

    // ── Melee ─────────────────────────────────────────────────────────────
    void HandleMelee()
    {
        // Bleed off velocity smoothly rather than snap-stopping
        _smoothVel = Vector3.MoveTowards(
            _smoothVel, Vector3.zero, acceleration * 2.5f * Time.deltaTime);
        if (_smoothVel.sqrMagnitude > 0.001f)
            transform.position += _smoothVel * Time.deltaTime;

        if (Time.time < _nextMelee) return;
        _nextMelee = Time.time + meleeCooldown;
        _fpsController?.TakeDamage(meleeDamage);
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    float HorizontalDist(Vector3 t)
    {
        float dx = t.x - transform.position.x;
        float dz = t.z - transform.position.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    Vector3 ComputeSeparation(float radius)
    {
        Vector3    sep   = Vector3.zero;
        Collider[] nearby = Physics.OverlapSphere(transform.position, radius);
        foreach (var n in nearby)
        {
            if (n.gameObject == gameObject) continue;
            if (n.GetComponent<LizardEnemy>() == null) continue;
            Vector3 away = transform.position - n.transform.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f)
                away = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            sep += away.normalized / Mathf.Max(away.magnitude, 0.1f);
        }
        return sep;
    }

    // ── Damage / Death ────────────────────────────────────────────────────
    public void TakeDamage(int dmg)
    {
        if (_isDead) return;
        health -= dmg;
        if (!_isFlashing && sprite != null) StartCoroutine(HitFlash());
        if (health <= 0) Die();
    }

    void Die()
    {
        _isDead = true;
        if (_agent != null) _agent.enabled = false;
        _hud?.AddKill();
        _spawner?.OnEnemyDied(gameObject);
        StartCoroutine(DeathAnim());
    }

    // ── Visuals ───────────────────────────────────────────────────────────
void HandleProceduralAnimation()
{
    if (sprite == null) return;

    // BUG FIX: Do NOT multiply by _baseScale here.
    // The root transform already encodes overall size (and the CapsuleCollider
    // scales with it). The sprite child should animate around local-scale = 1
    // so the visual and hitbox stay aligned for both basic and elite enemies.
    float halfH = sprite.sprite != null
        ? sprite.sprite.bounds.extents.y   // ← was: * _baseScale (caused overscale)
        : 1.0f;

    float speedRatio = _smoothVel.magnitude / Mathf.Max(maxSpeed, 0.01f);
    float bobSpeed   = Mathf.Lerp(2f, 12f, speedRatio);
    float bobAmt     = Mathf.Lerp(0.02f, 0.16f, speedRatio);

    float pulse = Mathf.Sin(Time.time * 2.2f + _bobOffset) * 0.04f;
    float s     = 1f + pulse;             // ← was: _baseScale + pulse (double-scaled)

    sprite.transform.localScale    = new Vector3(s, s, 1f);
    sprite.transform.localPosition = new Vector3(
        0f, halfH + Mathf.Sin(Time.time * bobSpeed + _bobOffset) * bobAmt, 0f);
    sprite.transform.localRotation = Quaternion.Euler(
        0f, 0f,
        Mathf.Sin(Time.time * bobSpeed * 1.1f + _bobOffset) * 6f * speedRatio);
}

    // Placeholder: originally missing method, added to avoid compile error
    void HandleBillboarding()
    {
        // No logic changed – fill with your own billboarding code if needed
    }

    IEnumerator HitFlash()
    {
        _isFlashing = true;
        Color orig = sprite.color;
        sprite.color = Color.white;
        yield return new WaitForSeconds(0.07f);
        sprite.color = orig;
        _isFlashing  = false;
    }

    IEnumerator DeathAnim()
    {
        float elapsed = 0f, duration = 0.12f;
        Vector3 start = transform.localScale;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, Vector3.zero, elapsed / duration);
            yield return null;
        }
        Destroy(gameObject);
    }
}