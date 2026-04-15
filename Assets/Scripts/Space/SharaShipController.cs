using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class SharaShipController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Weapons")]
    public GameObject beamPrefab;
    public float fireRate = 0.25f;

    [Header("Ashtar Beam")]
    public GameObject ashtarBeamPrefab;
    public float beamRechargeTime = 8f;
    public float beamDuration     = 0.45f;

    [Header("Damage")]
    public float invincibilityDuration = 2f;
    public LivesManager livesManager;

    [HideInInspector] public bool invincible;
    
    float _fireCd;
    float _beamCd;
    Text  _beamCdTxt;
    int   _sacrificeCount;
    Text  _sacrificeTxt;
    SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _sr.sortingOrder = 2;
        
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType      = RigidbodyType2D.Kinematic;
            rb.gravityScale  = 0f;
            rb.interpolation = RigidbodyInterpolation2D.None;
            rb.constraints   = RigidbodyConstraints2D.FreezeRotation;
        }

        if (livesManager == null)
            livesManager = FindObjectOfType<LivesManager>();
            
        if (beamPrefab == null)
            beamPrefab = Resources.Load<GameObject>("Prefabs/SharaBeam");
            
        if (ashtarBeamPrefab == null)
            ashtarBeamPrefab = Resources.Load<GameObject>("Prefabs/AshtarBeam");

        BuildUI();
    }

    void BuildUI()
    {
        var cv = FindObjectOfType<Canvas>();
        if (cv == null) return;

        // Beam Cooldown Label
        var uiGo = new GameObject("BeamCooldownText");
        uiGo.transform.SetParent(cv.transform, false);
        var rt = uiGo.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.right;
        rt.anchorMax = Vector2.right;
        rt.pivot = Vector2.right;
        rt.anchoredPosition = new Vector2(-15f, 15f);
        rt.sizeDelta = new Vector2(220f, 36f);
        _beamCdTxt = uiGo.AddComponent<Text>();
        _beamCdTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _beamCdTxt.fontSize = 22;
        _beamCdTxt.fontStyle = FontStyle.Bold;
        _beamCdTxt.alignment = TextAnchor.LowerRight;
        _beamCdTxt.color = new Color(0.45f, 0.85f, 1f, 1f);
        _beamCdTxt.text = "";

        // Sacrifice Counter
        var sGo = new GameObject("SacrificeCounterText");
        sGo.transform.SetParent(cv.transform, false);
        var srt = sGo.AddComponent<RectTransform>();
        srt.anchorMin = new Vector2(0f, 1f);
        srt.anchorMax = new Vector2(0f, 1f);
        srt.pivot = new Vector2(0f, 1f);
        srt.anchoredPosition = new Vector2(15f, -50f);
        srt.sizeDelta = new Vector2(500f, 28f);
        _sacrificeTxt = sGo.AddComponent<Text>();
        _sacrificeTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _sacrificeTxt.fontSize = 18;
        _sacrificeTxt.fontStyle = FontStyle.Bold;
        _sacrificeTxt.alignment = TextAnchor.UpperLeft;
        _sacrificeTxt.color = new Color(1f, 0.25f, 0.25f, 1f);
        _sacrificeTxt.text = "HUMANS SACRIFICED: 0";
    }

    void Update()
    {
        // Movement logic
        float h = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * h * speed * Time.deltaTime);
        
        float hw = Camera.main != null ? Camera.main.orthographicSize * Camera.main.aspect : 12f;
        float cx = Mathf.Clamp(transform.position.x, -hw, hw);
        transform.position = new Vector3(cx, -11f, 0f);

        HandleFiring();
        HandleAshtarBeam();
    }

    void HandleFiring()
    {
        bool firing = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
        if (firing)
        {
            _fireCd -= Time.deltaTime;
            if (_fireCd <= 0f)
            {
                _fireCd = fireRate;
                if (beamPrefab != null)
                {
                    Instantiate(beamPrefab, transform.position + new Vector3(0f, 0.8f, 0f), Quaternion.identity);
                    _sacrificeCount += 1;
                    UpdateSacrificeUI();
                }
            }
        }
        else { _fireCd = 0f; }
    }

    void HandleAshtarBeam()
    {
        if (_beamCd > 0f) 
        { 
            _beamCd -= Time.deltaTime; 
            if (_beamCd < 0f) _beamCd = 0f; 
        }

        if (Input.GetMouseButtonDown(1) && _beamCd <= 0f)
        {
            _beamCd = beamRechargeTime;
            const float bH = 28f;
            var bPos = new Vector3(transform.position.x, transform.position.y + bH * 0.5f, 0f);
            if (ashtarBeamPrefab != null)
            {
                Instantiate(ashtarBeamPrefab, bPos, Quaternion.identity);
                _sacrificeCount += 20;
                UpdateSacrificeUI();
            }
        }

        if (_beamCdTxt != null)
            _beamCdTxt.text = _beamCd > 0f
                ? "\u26a1 BEAM: " + _beamCd.ToString("0.0") + "s"
                : "\u26a1 BEAM READY";
    }

    void UpdateSacrificeUI()
    {
        if (_sacrificeTxt != null) 
            _sacrificeTxt.text = "HUMANS SACRIFICED: " + _sacrificeCount;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (invincible) return;
        var sp = col.GetComponent<SpaceProjectile>();
        if (sp != null && sp.isPlayerShot) return;
        
        bool hit = sp != null || col.CompareTag("Enemy");
        if (hit) StartCoroutine(HitSeq());
    }

    public void TakeHit() { if (!invincible) StartCoroutine(HitSeq()); }
    public void TakeDamage(float v) { if (!invincible) StartCoroutine(HitSeq()); }

    /// <summary>
    /// Critical: Call this from the Retry button to fix the "Invisible/Invincible" bug.
    /// </summary>
    public void ResetShipState()
    {
        StopAllCoroutines();
        invincible = false;
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        _sr.enabled = true;
        _fireCd = 0f;
        _beamCd = 0f;
        transform.position = new Vector3(0f, -11f, 0f);
    }

    IEnumerator HitSeq()
    {
        invincible = true;
        if (livesManager != null) livesManager.LoseLife();
        
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            _sr.enabled = !_sr.enabled;
            yield return new WaitForSeconds(0.15f);
            elapsed += 0.15f;
        }
        
        _sr.enabled = true;
        invincible  = false;
    }
}