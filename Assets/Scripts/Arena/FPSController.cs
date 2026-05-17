using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed   = 22f;
    public float jumpForce   = 22f;
    public float gravity     = -55f;

    [Header("Double Jump")]
    public int   extraJumps  = 1;

    [Header("Flight")]
    public float flightSpeed    = 40f;
    public float flightDuration = 10f;
    public float flightCooldown = 6f;

    [Header("Shooting")]
    public float fireRate    = 0.12f;
    public int   damage      = 1;
    public float bulletSpeed = 80f;
    public float bulletSize  = 1f;

    [Header("Laser")]
    public float laserRange         = 500f;
    public int   laserDamagePerTick = 1;
    public float laserTickRate      = 0.08f;
    public float laserBurstDuration = 8f;

    [Header("Mouse Look")]
    public float mouseSens = 2f;

    [Header("Stats")]
    public int health = 100;
    public int armor  = 50;

    [Header("On Death")]
    public string gameOverScene = "TitleScreen";

    // Read by ArenaHUD to display laser timer
    [HideInInspector] public float laserTimeLeft = 0f;

    CharacterController _cc;
    Transform           _cam;
    float               _pitch;
    float               _velY;
    bool                _isDead;

    int   _jumpsLeft;
    bool  _isFlying;
    float _flightTimer;
    float _flightCdTimer;
    float _nextFire;

    LineRenderer _laser;
    float        _nextLaserTick;
    int          _laserKillThreshold;

    ArenaHUD _hud;

    const float TerminalVelocity = -40f;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        var c = GetComponentInChildren<Camera>();
        if (c != null) _cam = c.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        _flightTimer = flightDuration;

        BuildLaser();
    }

    void Start()
    {
        _hud = FindObjectOfType<ArenaHUD>();
        SnapToGround();
    }

    void BuildLaser()
    {
        var laserGO = new GameObject("LaserBeam");
        laserGO.transform.SetParent(_cam != null ? _cam : transform);
        laserGO.transform.localPosition = Vector3.zero;

        _laser = laserGO.AddComponent<LineRenderer>();
        _laser.positionCount = 2;
        _laser.useWorldSpace = true;
        _laser.startWidth    = 0.12f;
        _laser.endWidth      = 0.03f;
        _laser.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _laser.receiveShadows    = false;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat == null) mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0f, 0.9f, 1f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0f, 1f, 1f) * 8f);

        _laser.material   = mat;
        _laser.startColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
        _laser.endColor   = new Color(0.0f, 0.6f, 1.0f, 0.2f);
        _laser.enabled    = false;
    }

    void SnapToGround()
    {
        if (_cc == null) return;

        float searchRange = 600f;
        Vector3 castOrigin = transform.position + Vector3.up * 10f;
        RaycastHit[] hits = Physics.RaycastAll(castOrigin, Vector3.down, searchRange);

        if (hits.Length > 0)
        {
            float lowestY = float.MaxValue;
            bool found = false;

            foreach (var hit in hits)
            {
                if (hit.point.y < lowestY)
                {
                    lowestY = hit.point.y;
                    found = true;
                }
            }

            if (found)
            {
                float targetCenterY = lowestY + _cc.height * 0.5f + 0.1f;
                transform.position = new Vector3(transform.position.x,
                                                 targetCenterY,
                                                 transform.position.z);
            }
        }
    }

    void Update()
    {
        if (_isDead) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        HandleLook();
        HandleMovement();
        HandleFlight();
        HandleShooting();
        CheckLaserUnlock();
        HandleLaser();
    }

    void CheckLaserUnlock()
    {
        if (_hud == null) return;
        int reached = (_hud.kills / 100) * 100;
        if (reached > _laserKillThreshold)
        {
            int newThresholds  = (reached - _laserKillThreshold) / 100;
            laserTimeLeft     += newThresholds * laserBurstDuration;
            _laserKillThreshold = reached;
        }
    }

    void HandleLook()
    {
        float yaw   =  Input.GetAxis("Mouse X") * mouseSens;
        float pitch = -Input.GetAxis("Mouse Y") * mouseSens;
        transform.Rotate(0f, yaw, 0f);
        _pitch = Mathf.Clamp(_pitch + pitch, -80f, 80f);
        if (_cam != null) _cam.localEulerAngles = new Vector3(_pitch, 0f, 0f);
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = transform.right * h + transform.forward * v;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        if (_cc.isGrounded)
        {
            if (_velY < 0f) _velY = -2f;
            _jumpsLeft = extraJumps;
            if (_isFlying) StopFlight();
        }

        if (!_isFlying)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_cc.isGrounded)       _velY = jumpForce;
                else if (_jumpsLeft > 0) { _velY = jumpForce; _jumpsLeft--; }
            }
            _velY += gravity * Time.deltaTime;
            _velY = Mathf.Max(_velY, TerminalVelocity);
        }

        _cc.Move((dir * moveSpeed + Vector3.up * _velY) * Time.deltaTime);
    }

    void HandleFlight()
    {
        if (!_isFlying && _flightCdTimer > 0f)
        {
            _flightCdTimer -= Time.deltaTime;
            if (_flightCdTimer <= 0f) _flightTimer = flightDuration;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_isFlying) StopFlight();
            else if (_flightTimer > 0f && _flightCdTimer <= 0f) _isFlying = true;
        }

        if (_isFlying)
        {
            _flightTimer -= Time.deltaTime;
            float lift = 0f;
            if (Input.GetKey(KeyCode.Space))       lift =  1f;
            if (Input.GetKey(KeyCode.LeftControl)) lift = -1f;
            _velY = Mathf.Lerp(_velY, lift * flightSpeed, Time.deltaTime * 20f);
            if (_flightTimer <= 0f) StopFlight();
        }
    }

    void HandleShooting()
    {
        if (!Input.GetMouseButton(0)) return;
        if (Time.time < _nextFire) return;

        _nextFire = Time.time + fireRate;

        Transform origin = _cam != null ? _cam : transform;
        Vector3   pos    = origin.position + origin.forward * 0.5f;

        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Bullet";
        go.transform.position   = pos;
        go.transform.rotation   = Quaternion.LookRotation(origin.forward);
        go.transform.localScale = new Vector3(0.08f * bulletSize, 0.35f * bulletSize, 0.08f * bulletSize);
        go.transform.Rotate(90f, 0f, 0f);

        var col = go.GetComponent<CapsuleCollider>();
        col.isTrigger = true;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat == null) mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 0.85f, 0f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(1f, 0.7f, 0f) * 4f);
        go.GetComponent<MeshRenderer>().material = mat;
        go.GetComponent<MeshRenderer>().shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        var bullet   = go.AddComponent<ProceduralBullet>();
        bullet.speed  = bulletSpeed;
        bullet.damage = damage;
        bullet.Fire(origin.forward);
    }

    void HandleLaser()
    {
        if (!Input.GetMouseButton(1) || laserTimeLeft <= 0f)
        {
            if (_laser != null) _laser.enabled = false;
            return;
        }

        laserTimeLeft -= Time.deltaTime;
        laserTimeLeft  = Mathf.Max(laserTimeLeft, 0f);

        Transform origin  = _cam != null ? _cam : transform;
        Vector3 beamStart = origin.position + origin.forward * 0.5f;
        Vector3 beamEnd;

        if (Physics.Raycast(origin.position, origin.forward,
                            out RaycastHit hit, laserRange))
        {
            beamEnd = hit.point;

            if (Time.time >= _nextLaserTick)
            {
                _nextLaserTick = Time.time + laserTickRate;
                var enemy = hit.collider.GetComponentInParent<LizardEnemy>();
                if (enemy != null) enemy.TakeDamage(laserDamagePerTick);
            }
        }
        else
        {
            beamEnd = origin.position + origin.forward * laserRange;
        }

        _laser.enabled = true;
        _laser.SetPosition(0, beamStart);
        _laser.SetPosition(1, beamEnd);
    }

    void StopFlight()
    {
        _isFlying      = false;
        _flightCdTimer = flightCooldown;
        _velY          = 0f;
    }

    public void TakeDamage(int dmg)
    {
        if (_isDead) return;
        int absorbed = Mathf.Min(armor, dmg);
        armor  -= absorbed;
        health -= (dmg - absorbed);
        health  = Mathf.Max(health, 0);
        if (health <= 0) Die();
    }

    void Die()
    {
        if (_isDead) return;
        _isDead = true;
        if (_laser != null) _laser.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        SceneManager.LoadScene(gameOverScene);
    }
}