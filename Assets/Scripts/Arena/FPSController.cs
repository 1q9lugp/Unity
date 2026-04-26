using UnityEngine;

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

    CharacterController _cc;
    Transform           _cam;
    float               _pitch;
    float               _velY;

    void Awake()
    {
        _cc  = GetComponent<CharacterController>();
        var c = GetComponentInChildren<Camera>();
        if (c != null) _cam = c.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void Update()
    {
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
}
