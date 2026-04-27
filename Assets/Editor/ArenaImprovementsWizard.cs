using UnityEngine;
using UnityEditor;
using System.IO;

public static class ArenaImprovementsWizard
{
    [MenuItem("Tools/Apply Arena Improvements")]
    static void Run()
    {
        Write("Assets/Scripts/Arena/LizardEnemy.cs",  Lizard());
        Write("Assets/Scripts/Arena/EnemySpawner.cs", Spawner());
        Write("Assets/Scripts/Arena/ArenaHUD.cs",     Hud());
        AssetDatabase.Refresh();
        Debug.Log("[ArenaFix] All 3 scripts written. Waiting for compile...");
    }

    static void Write(string asset, string code)
    {
        File.WriteAllText(Application.dataPath.Replace("Assets","") + asset, code);
        Debug.Log("[ArenaFix] Written: " + asset);
    }

    // ── LizardEnemy ──────────────────────────────────────────────────────────
    static string Lizard() => @"using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LizardEnemy : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    public int   health    = 3;
    public float moveSpeed = 3.5f;
    public bool  isElite   = false;

    [Header(""Combat"")]
    public float attackRange    = 1.8f;
    public float attackCooldown = 0.8f;
    public int   attackDamage   = 10;

    [Header(""Separation"")]
    public float separationRadius = 2.5f;
    public float separationForce  = 2.0f;

    [Header(""Elite Charge"")]
    public float chargeRange    = 12f;
    public float chargeCooldown = 5f;
    public float chargeSpeed    = 12f;
    public float chargeDuration = 1.5f;

    // ── State machine ─────────────────────────────────────────────────────────
    enum State { Chase, Attack, Stagger, Charge, Dying }
    State _state = State.Chase;

    // Shared list for separation (3)
    static readonly List<LizardEnemy> _all = new List<LizardEnemy>();

    // ── Refs & cached values ──────────────────────────────────────────────────
    Transform      _player;
    FPSController  _fpsPlayer;
    Transform      _spriteT;
    SpriteRenderer _sr;
    Color          _baseColor;
    float          _baseY;
    float          _bobPhase;     // (5)
    float          _flankAngle;   // (8)

    float _attackTimer;
    float _staggerTimer;
    float _chargeTimer;
    float _chargeCdTimer;
    float _navTimer;

    NavMeshAgent _agent;
    bool         _useNav;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    void Awake()
    {
        _spriteT = transform.Find(""Sprite"");
        if (_spriteT != null) { _sr = _spriteT.GetComponent<SpriteRenderer>(); _baseY = _spriteT.localPosition.y; }
        _bobPhase   = Random.Range(0f, Mathf.PI * 2f);
        _flankAngle = Random.Range(-40f, 40f);           // (8)
        CreateShadow();                                   // (10)
        _agent  = GetComponent<NavMeshAgent>();
        _useNav = _agent != null && _agent.isOnNavMesh;
        if (_useNav) { _agent.speed = moveSpeed; _agent.angularSpeed = 720f; _agent.acceleration = 20f; }
    }

    void OnEnable()  { _all.Add(this); }
    void OnDisable() { _all.Remove(this); }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag(""Player"");
        if (p != null) { _player = p.transform; _fpsPlayer = p.GetComponent<FPSController>(); }
        // Color tint by type (visual differentiation)
        if (_sr != null) _sr.color = isElite ? new Color(0.6f, 0.6f, 1f) : new Color(1f, 0.6f, 0.6f);
        _baseColor     = _sr != null ? _sr.color : Color.white;
        _chargeCdTimer = Random.Range(2f, chargeCooldown); // stagger first charges
    }

    void Update()
    {
        if (_player == null || _state == State.Dying) return;

        // (5) Billboard — sprite child always faces camera
        if (_spriteT != null && Camera.main != null)
            _spriteT.rotation = Camera.main.transform.rotation;

        float dist = Vector3.Distance(transform.position, _player.position);

        switch (_state)
        {
            case State.Chase:   UpdateChase(dist);  break;
            case State.Attack:  UpdateAttack(dist); break;
            case State.Stagger: UpdateStagger();    break;
            case State.Charge:  UpdateCharge();     break;
        }

        // (5) Bob — sine wave on sprite Y, randomised phase per enemy
        if (_spriteT != null && _state != State.Dying)
        {
            var lp = _spriteT.localPosition;
            _spriteT.localPosition = new Vector3(lp.x, _baseY + Mathf.Sin(Time.time * 3f + _bobPhase) * 0.12f, lp.z);
        }
    }

    // ── States ────────────────────────────────────────────────────────────────
    void UpdateChase(float dist)
    {
        // (9) Elite charge trigger
        if (isElite)
        {
            _chargeCdTimer -= Time.deltaTime;
            if (_chargeCdTimer <= 0f && dist < chargeRange)
            {
                _state = State.Charge; _chargeTimer = chargeDuration;
                _chargeCdTimer = chargeCooldown;
                StartCoroutine(ChargeWindup());
                return;
            }
        }

        // (4) Transition to attack range
        if (dist < attackRange) { _state = State.Attack; _attackTimer = 0f; return; }

        // (8) Flanking — each enemy approaches from a slightly different angle
        Vector3 toPlayer = _player.position - transform.position; toPlayer.y = 0f;
        Vector3 flankTgt = _player.position + Quaternion.Euler(0, _flankAngle, 0) * (-toPlayer.normalized) * 0.5f;
        Vector3 dir = flankTgt - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f) dir.Normalize();

        // (3) Separation — repel from nearby allies
        Vector3 sep = Vector3.zero;
        for (int i = 0; i < _all.Count; i++)
        {
            var o = _all[i];
            if (o == null || o == this) continue;
            Vector3 away = transform.position - o.transform.position; away.y = 0f;
            float d = away.magnitude;
            if (d < separationRadius && d > 0.001f)
                sep += away.normalized * ((separationRadius - d) / separationRadius);
        }
        dir = (dir + sep * separationForce).normalized;

        // Move
        if (_useNav && _agent.isOnNavMesh)
        {
            _navTimer -= Time.deltaTime;
            if (_navTimer <= 0f) { _agent.SetDestination(_player.position); _navTimer = 0.25f; }
        }
        else
        {
            transform.position += dir * (moveSpeed * Time.deltaTime);
        }

        // (1) Fix facing — look toward player on Y-locked plane
        if (toPlayer.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(toPlayer);
    }

    void UpdateAttack(float dist)
    {
        if (dist > attackRange + 0.5f) { _state = State.Chase; return; }
        // (1) Face player while attacking
        Vector3 d = _player.position - transform.position; d.y = 0f;
        if (d.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(d);
        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0f)
        {
            _attackTimer = attackCooldown;
            if (_fpsPlayer != null) _fpsPlayer.health -= attackDamage;
            StartCoroutine(LungePunch());    // (5) visual lunge
        }
    }

    void UpdateStagger() { _staggerTimer -= Time.deltaTime; if (_staggerTimer <= 0f) _state = State.Chase; }

    void UpdateCharge()
    {
        if (_player == null) { _state = State.Chase; return; }
        Vector3 d = _player.position - transform.position; d.y = 0f;
        transform.position += d.normalized * (chargeSpeed * Time.deltaTime);
        if (d.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(d);
        _chargeTimer -= Time.deltaTime;
        if (_chargeTimer <= 0f) _state = State.Chase;
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    // (2) Hit flash
    IEnumerator HitFlash()
    {
        if (_sr == null) yield break;
        _sr.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        _sr.color = _baseColor;
    }

    // (4) Stagger with knockback
    IEnumerator StaggerRoutine()
    {
        _state = State.Stagger; _staggerTimer = 0.3f;
        if (_player != null)
        {
            Vector3 kb = transform.position - _player.position; kb.y = 0f;
            transform.position += kb.normalized * 0.5f;
        }
        yield return null;
    }

    // (5) Lunge punch animation on sprite child
    IEnumerator LungePunch()
    {
        if (_spriteT == null) yield break;
        Vector3 start = _spriteT.localPosition;
        Vector3 end   = start + new Vector3(0f, 0f, -0.4f);
        float t = 0f;
        while (t < 0.1f) { t += Time.deltaTime; _spriteT.localPosition = Vector3.Lerp(start, end, t / 0.1f); yield return null; }
        t = 0f;
        while (t < 0.1f) { t += Time.deltaTime; _spriteT.localPosition = Vector3.Lerp(end, start, t / 0.1f); yield return null; }
        _spriteT.localPosition = start;
    }

    // (9) Elite charge warning flash
    IEnumerator ChargeWindup()
    {
        for (int i = 0; i < 5; i++)
        {
            if (_sr != null) _sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            if (_sr != null) _sr.color = _baseColor;
            yield return new WaitForSeconds(0.05f);
        }
    }

    // (6) Death — shrink to zero, spawn particles, then destroy
    IEnumerator DieShrink()
    {
        _state = State.Dying;
        SpawnDeathParticles();
        float s = 1f;
        while (s > 0f) { s -= Time.deltaTime * 5f; transform.localScale = Vector3.one * Mathf.Max(0f, s); yield return null; }
        Destroy(gameObject);
    }

    void SpawnDeathParticles()
    {
        var pgo  = new GameObject(""DeathFX"");
        pgo.transform.position = transform.position + Vector3.up * 0.9f;
        var ps   = pgo.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(2f, 6f);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
        main.startColor    = isElite ? new Color(0.3f, 0.3f, 1f) : new Color(0.8f, 0.1f, 0.1f);
        main.maxParticles  = 12;
        main.loop          = false;
        var em = ps.emission;
        em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 12) });
        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius    = 0.3f;
        ps.Play();
        Destroy(pgo, 1.5f);
    }

    // (10) Shadow blob — flat cylinder just above ground
    void CreateShadow()
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        s.name = ""Shadow"";
        s.transform.SetParent(transform);
        s.transform.localPosition = new Vector3(0f, 0.02f, 0f);
        s.transform.localScale    = new Vector3(0.6f, 0.01f, 0.6f);
        Destroy(s.GetComponent<CapsuleCollider>());
        var mr = s.GetComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows    = false;
        var mat = new Material(Shader.Find(""Sprites/Default""));
        mat.color = new Color(0f, 0f, 0f, 0.55f);
        mr.material = mat;
    }

    // (10) Damage number — spawned on a self-contained helper so it survives enemy death
    void SpawnDamageNumber(int dmg)
    {
        var go = new GameObject(""DmgNum"");
        go.transform.position = transform.position + Vector3.up * 1.8f + Random.insideUnitSphere * 0.3f;
        var tm = go.AddComponent<TextMesh>();
        tm.text          = dmg.ToString();
        tm.characterSize = 0.1f;
        tm.fontSize      = 60;
        tm.color         = new Color(1f, 0.9f, 0.1f);
        tm.anchor        = TextAnchor.MiddleCenter;
        tm.alignment     = TextAlignment.Center;
        tm.fontStyle     = FontStyle.Bold;
        go.AddComponent<DmgNumMover>().Run(); // self-managing component
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void TakeDamage(int dmg)
    {
        if (_state == State.Dying) return;
        health -= dmg;
        StartCoroutine(HitFlash());     // (2)
        SpawnDamageNumber(dmg);         // (10)
        if (health <= 0)
        {
            var hud = FindFirstObjectByType<ArenaHUD>();   if (hud != null) hud.AddKill();
            var sp  = FindFirstObjectByType<EnemySpawner>(); if (sp  != null) sp.OnEnemyDied();
            StartCoroutine(DieShrink()); // (6)
            return;
        }
        StartCoroutine(StaggerRoutine()); // (4)
    }
}

// Damage number floater — lives on its own GameObject, survives enemy death
public class DmgNumMover : MonoBehaviour
{
    public void Run() { StartCoroutine(Float()); }

    IEnumerator Float()
    {
        var tm = GetComponent<TextMesh>();
        if (tm == null) { Destroy(gameObject); yield break; }
        float t = 0f;
        Color c = tm.color;
        while (t < 0.6f)
        {
            t += Time.deltaTime;
            transform.position += Vector3.up * (Time.deltaTime * 1.5f);
            if (Camera.main != null) transform.forward = -Camera.main.transform.forward;
            c.a = 1f - t / 0.6f;
            tm.color = c;
            yield return null;
        }
        Destroy(gameObject);
    }
}
";

    // ── EnemySpawner ─────────────────────────────────────────────────────────
    static string Spawner() => @"using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header(""Wave Config"")]
    public int   totalWaves    = 10;
    public float spawnRadius   = 40f;
    public float spawnInterval = 0.08f;  // seconds between individual spawns within a wave
    public float wavePause     = 4f;     // pause between waves

    [Header(""Templates — assign inactive scene objects"")]
    public GameObject gruntTemplate;
    public GameObject eliteTemplate;

    // Read by ArenaHUD
    [HideInInspector] public int  currentWave   = 0;
    [HideInInspector] public int  activeEnemies = 0;
    [HideInInspector] public bool allWavesDone  = false;

    ArenaHUD _hud;

    void Start()
    {
        _hud = FindFirstObjectByType<ArenaHUD>();
        StartCoroutine(WaveLoop());
    }

    public void OnEnemyDied() { activeEnemies = Mathf.Max(0, activeEnemies - 1); }

    IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(1f); // brief intro pause

        for (int w = 1; w <= totalWaves; w++)
        {
            currentWave = w;
            if (_hud != null) _hud.SetWave(w, totalWaves);

            yield return StartCoroutine(SpawnWave(WaveCount(w), WaveEliteRate(w)));
            yield return new WaitUntil(() => activeEnemies <= 0);

            if (w < totalWaves)
            {
                if (_hud != null) _hud.ShowWaveMessage(""WAVE "" + w + "" CLEAR"");
                yield return new WaitForSeconds(wavePause);
            }
        }

        allWavesDone = true;
        if (_hud != null) _hud.ShowWaveMessage(""ALL WAVES COMPLETE"");
    }

    IEnumerator SpawnWave(int count, float eliteRate)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnOne(eliteRate);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnOne(float eliteRate)
    {
        bool isElite   = Random.value < eliteRate;
        var  tmpl      = isElite ? eliteTemplate : gruntTemplate;
        if (tmpl == null) return;

        var     player = GameObject.FindGameObjectWithTag(""Player"");
        Vector3 origin = player != null ? player.transform.position : Vector3.zero;
        float   a      = Random.Range(0f, Mathf.PI * 2f);
        Vector3 pos    = origin + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * spawnRadius;

        var go = Instantiate(tmpl, pos, Quaternion.identity);
        go.SetActive(true);

        var e = go.GetComponent<LizardEnemy>();
        if (e != null)
        {
            e.isElite   = isElite;
            e.health    = isElite ? 10 : 3;
            e.moveSpeed = isElite ? 3.2f : 3.8f; // elites slightly slower, still threatening
        }
        activeEnemies++;
    }

    // Wave scaling — count 8→40, elite ratio 0%→50%
    int   WaveCount    (int w) => Mathf.RoundToInt(Mathf.Lerp(8f,  40f,  (w - 1f) / Mathf.Max(1, totalWaves - 1)));
    float WaveEliteRate(int w) => Mathf.Lerp(0f, 0.5f, (w - 1f) / Mathf.Max(1, totalWaves - 1));
}
";

    // ── ArenaHUD ──────────────────────────────────────────────────────────────
    static string Hud() => @"using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ArenaHUD : MonoBehaviour
{
    FPSController _player;
    Text _ammoNum, _healthNum, _armorNum, _killNum, _waveNum, _waveMsg;
    int _kills;

    // ── Public API ────────────────────────────────────────────────────────────
    public void AddKill() { _kills++; }

    public void SetWave(int w, int total)
    {
        if (_waveNum != null) _waveNum.text = ""WAVE "" + w + "" / "" + total;
    }

    public void ShowWaveMessage(string msg)
    {
        if (_waveMsg != null) StartCoroutine(FlashMsg(msg));
    }

    IEnumerator FlashMsg(string msg)
    {
        if (_waveMsg == null) yield break;
        _waveMsg.text = msg;
        _waveMsg.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        _waveMsg.gameObject.SetActive(false);
    }

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    void Start()
    {
        _player = FindFirstObjectByType<FPSController>();
        if (_player != null) { _player.health = 100; _player.ammo = 150; _player.armor = 50; }
        BuildHUD();
    }

    void Update()
    {
        if (_player == null) return;
        if (_healthNum != null) _healthNum.text = ""HEALTH: "" + _player.health;
        if (_armorNum  != null) _armorNum.text  = ""ARMOR: ""  + _player.armor  + ""%"";
        if (_ammoNum   != null) _ammoNum.text   = ""AMMO: ""   + _player.ammo;
        if (_killNum   != null) _killNum.text   = ""LIZARDS: "" + _kills;
    }

    // ── UI construction ───────────────────────────────────────────────────────
    void BuildHUD()
    {
        var cv = GetComponent<Canvas>(); if (cv == null) return;
        Color yellow = new Color(0.95f, 0.85f, 0.10f);
        Color red    = new Color(0.95f, 0.20f, 0.15f);
        Color green  = new Color(0.15f, 0.85f, 0.25f);
        Color lblue  = new Color(0.30f, 0.80f, 0.95f);
        Color panel  = new Color(0.12f, 0.12f, 0.12f, 0.65f);
        Vector2 mSz  = new Vector2(280, 112);

        var left = CreateModule(""Vitals"", new Vector2(0, 0), mSz, Vector2.zero, panel);
        _armorNum  = CreateStat(left, ""ARMOR: "",  new Vector2(10,  25), 22, lblue,  TextAnchor.MiddleLeft);
        _healthNum = CreateStat(left, ""HEALTH: "", new Vector2(10, -22), 36, red,    TextAnchor.MiddleLeft);

        var right = CreateModule(""Combat"", new Vector2(1, 0), mSz, Vector2.zero, panel);
        _killNum = CreateStat(right, ""LIZARDS: "", new Vector2(-10,  25), 22, green,  TextAnchor.MiddleRight);
        _ammoNum = CreateStat(right, ""AMMO: "",    new Vector2(-10, -22), 36, yellow, TextAnchor.MiddleRight);

        // Wave counter — top centre
        var wg = new GameObject(""WaveCounter""); wg.transform.SetParent(transform, false);
        var wr = wg.AddComponent<RectTransform>();
        wr.anchorMin = new Vector2(0.5f, 1f); wr.anchorMax = new Vector2(0.5f, 1f);
        wr.pivot = new Vector2(0.5f, 1f); wr.anchoredPosition = new Vector2(0, -10f); wr.sizeDelta = new Vector2(320f, 40f);
        _waveNum = wg.AddComponent<Text>();
        _waveNum.font = Resources.GetBuiltinResource<Font>(""LegacyRuntime.ttf"");
        _waveNum.fontSize = 28; _waveNum.fontStyle = FontStyle.Bold;
        _waveNum.alignment = TextAnchor.UpperCenter; _waveNum.color = Color.white; _waveNum.text = ""WAVE 0 / 10"";
        var wol = wg.AddComponent<Outline>(); wol.effectColor = Color.black; wol.effectDistance = new Vector2(2, -2);

        // Wave message — centre screen, hidden by default
        var mg = new GameObject(""WaveMsg""); mg.transform.SetParent(transform, false);
        var mr = mg.AddComponent<RectTransform>();
        mr.anchorMin = new Vector2(0f, 0.42f); mr.anchorMax = new Vector2(1f, 0.58f);
        mr.offsetMin = Vector2.zero; mr.offsetMax = Vector2.zero;
        _waveMsg = mg.AddComponent<Text>();
        _waveMsg.font = Resources.GetBuiltinResource<Font>(""LegacyRuntime.ttf"");
        _waveMsg.fontSize = 56; _waveMsg.fontStyle = FontStyle.Bold;
        _waveMsg.alignment = TextAnchor.MiddleCenter;
        _waveMsg.color = new Color(1f, 0.85f, 0.1f, 1f); _waveMsg.text = """";
        var mol = mg.AddComponent<Outline>(); mol.effectColor = Color.black; mol.effectDistance = new Vector2(3, -3);
        mg.SetActive(false);
    }

    GameObject CreateModule(string name, Vector2 anchor, Vector2 size, Vector2 pos, Color bg)
    {
        var go = new GameObject(name); go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.AddComponent<Image>().color = bg;
        return go;
    }

    Text CreateStat(GameObject parent, string label, Vector2 pos, int fs, Color col, TextAnchor align)
    {
        var go = new GameObject(label + ""Text""); go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(260, 60);
        var txt = go.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>(""LegacyRuntime.ttf"");
        txt.fontSize = fs; txt.fontStyle = FontStyle.Bold; txt.color = col;
        txt.alignment = align; txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.text = label + ""0"";
        var ol = go.AddComponent<Outline>(); ol.effectColor = Color.black; ol.effectDistance = new Vector2(2, -2);
        return txt;
    }
}
";
}
