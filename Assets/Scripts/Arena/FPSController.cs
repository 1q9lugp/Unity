using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float gravity   = -20f;

    [Header("Mouse Look")]
    public float mouseSens = 2f;

    [Header("Stats")]
    public int health = 100;
    public int armor  = 50;
    public int ammo   = 999;

    [Header("On Death")]
    public string gameOverScene = "TitleScreen"; // scene name to load on death

    CharacterController _cc;
    Transform           _cam;
    float               _pitch;
    float               _velY;
    bool                _isDead;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        var c = GetComponentInChildren<Camera>();
        if (c != null) _cam = c.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void Update()
    {
        if (_isDead) return;

        // Cursor toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        // Mouse look
        float yaw   =  Input.GetAxis("Mouse X") * mouseSens;
        float pitch = -Input.GetAxis("Mouse Y") * mouseSens;
        transform.Rotate(0f, yaw, 0f);
        _pitch = Mathf.Clamp(_pitch + pitch, -80f, 80f);
        if (_cam != null) _cam.localEulerAngles = new Vector3(_pitch, 0f, 0f);

        // Move
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = transform.right * h + transform.forward * v;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        if (_cc.isGrounded) _velY = -2f;
        _velY += gravity * Time.deltaTime;

        _cc.Move((dir * moveSpeed + Vector3.up * _velY) * Time.deltaTime);
    }

    // ── damage ───────────────────────────────────────────────────────────
    public void TakeDamage(int dmg)
    {
        if (_isDead) return;

        // Armor absorbs damage first, then health takes the rest
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