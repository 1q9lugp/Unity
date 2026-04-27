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
    public float fireRate   = 0.12f;   // seconds between shots
    public int   damage     = 1;
    public float bulletSpeed = 80f;

    [Header("Mouse Look")]
    public float mouseSens = 2f;

    [Header("Stats")]
    public int health = 100;
    public int armor  = 50;
    public int ammo   = 999;

    [Header("On Death")]
    public string gameOverScene = "TitleScreen";

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

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        var c = GetComponentInChildren<Camera>();
        if (c != null) _cam = c.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        _flightTimer = flightDuration;
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
            if (_velY < 0f) _velY = -0.5f;
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
        if (ammo <= 0) return;

        _nextFire = Time.time + fireRate;
        ammo--;

        // Spawn origin: camera position, fired along camera forward
        Transform origin = _cam != null ? _cam : transform;
        Vector3   pos    = origin.position + origin.forward * 0.5f;

        // Build a temporary procedural bullet: glowing elongated capsule
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Bullet";
        go.transform.position   = pos;
        go.transform.rotation   = Quaternion.LookRotation(origin.forward);
        go.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);

        // Rotate capsule so its length axis aligns with forward
        go.transform.Rotate(90f, 0f, 0f);

        // Swap to trigger so it detects enemies without blocking movement
        var col = go.GetComponent<CapsuleCollider>();
        col.isTrigger = true;

        // Bright emissive yellow material
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat == null) mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 0.85f, 0f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(1f, 0.7f, 0f) * 4f);
        go.GetComponent<MeshRenderer>().material = mat;

        // Remove shadow casting — bullets shouldn't cast shadows
        go.GetComponent<MeshRenderer>().shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        var bullet   = go.AddComponent<ProceduralBullet>();
        bullet.speed  = bulletSpeed;
        bullet.damage = damage;
        bullet.Fire(origin.forward);
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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        SceneManager.LoadScene(gameOverScene);
    }
}