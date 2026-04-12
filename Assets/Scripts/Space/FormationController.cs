using System.Collections.Generic;
using UnityEngine;

public class FormationController : MonoBehaviour
{
    public int columns = 5, rows = 3;
    public float colSpacing = 4.5f, rowSpacing = 3.2f;
    public float moveSpeed = 1.5f, driftSpeed = 0.05f, driftFloor = -2f;
    public int miniBossCount;
    public float shootInterval = 3f;
    public GameObject enemy1Prefab, enemy2Prefab, enemy3Prefab, bossPrefab, projectilePrefab;
    public Sprite row0Sprite, row1Sprite, row2Sprite, row3Sprite, row4Sprite, bossSprite;

    readonly List<EnemyShip> _enemies = new List<EnemyShip>();
    float _dir = 1f, _diveTimer = 4f;
    bool _active;
    int _tier = 1;
    bool _hasBoss;
    PlanetEncounterManager _mgr;

    public void Init(int c, int r, float spd, int mb, float shoot,
                     GameObject e1, GameObject e2, GameObject e3,
                     GameObject boss, GameObject pp,
                     Transform pl, PlanetEncounterManager m)
    {
        columns = c; rows = r; moveSpeed = spd; miniBossCount = mb;
        shootInterval = shoot;
        enemy1Prefab = e1; enemy2Prefab = e2; enemy3Prefab = e3;
        bossPrefab = boss; projectilePrefab = pp;
        _mgr = m;
    }

    public void SetTierAndBoss(int tier, bool hasBoss)
    {
        _tier = tier;
        _hasBoss = hasBoss;
    }

    public void OnEnemyDestroyed(EnemyShip e)
    {
        _enemies.Remove(e);
        if (_enemies.Count == 0)
        {
            _active = false;
            if (_mgr != null) _mgr.OnRoundCleared();
        }
    }

    public Vector2 GetFormationWorldPos(Vector2 lo) => (Vector2)transform.position + lo;

    void Start()
    {
        // Spawn enemies
        foreach (var x in _enemies) if (x != null) Destroy(x.gameObject);
        _enemies.Clear();

        var bc = new List<int>();
        if (miniBossCount >= 1) bc.Add(columns / 2);
        if (miniBossCount >= 2) bc.Add(columns / 2 - 1);

        float sx = -(columns - 1) * colSpacing * 0.5f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                bool isMini = bc.Contains(c) && r == 0;
                // Inline prefab selection
                GameObject pfb;
                if (isMini)
                    pfb = bossPrefab != null ? bossPrefab : (_tier >= 3 && enemy3Prefab != null ? enemy3Prefab : (_tier >= 2 && enemy2Prefab != null ? enemy2Prefab : enemy1Prefab));
                else
                    pfb = _tier >= 3 && enemy3Prefab != null ? enemy3Prefab : (_tier >= 2 && enemy2Prefab != null ? enemy2Prefab : enemy1Prefab);
                if (pfb == null) continue;

                var go = Instantiate(pfb, transform);
                var off = new Vector2(sx + c * colSpacing, 8f - r * rowSpacing);
                go.transform.localPosition = new Vector3(off.x, off.y, 0f);

                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // Inline sprite selection
                    Sprite s = isMini ? bossSprite : (r == 0 ? row0Sprite : r == 1 ? row1Sprite : r == 2 ? row2Sprite : r == 3 ? row3Sprite : row4Sprite);
                    if (s != null) sr.sprite = s;
                }

                var es = go.GetComponent<EnemyShip>();
                if (es == null) continue;
                es.formation = this;
                es.formationOffset = off;
                es.projectilePrefab = projectilePrefab;
                es.hp = isMini ? 8 : _tier;
                es.shootInterval = isMini ? shootInterval * 0.5f : shootInterval;
                _enemies.Add(es);
            }
        }

        // Standalone boss
        if (_hasBoss && miniBossCount == 0 && bossPrefab != null)
        {
            float bx = 0f, by = 8f + rowSpacing;
            var go = Instantiate(bossPrefab, transform);
            go.transform.localPosition = new Vector3(bx, by, 0f);
            var es = go.GetComponent<EnemyShip>();
            if (es != null)
            {
                es.formation = this;
                es.formationOffset = new Vector2(bx, by);
                es.hp = 8;
                es.shootInterval = shootInterval * 0.5f;
                es.projectilePrefab = projectilePrefab;
                _enemies.Add(es);
            }
        }

        _active = true;
    }

    void Update()
    {
        if (!_active) return;

        // Dynamic bounce wall — matches player camera clamping
        float bounceX = Camera.main != null
            ? Camera.main.orthographicSize * Camera.main.aspect - 1.5f
            : 9f;

        float dy = -driftSpeed * Time.deltaTime;
        if (transform.position.y + dy < driftFloor) dy = 0f;
        transform.position += new Vector3(_dir * moveSpeed * Time.deltaTime, dy, 0f);

        float px = transform.position.x;
        if (px >= bounceX)
        {
            transform.position = new Vector3(bounceX, transform.position.y, 0f);
            _dir = -1f;
        }
        else if (px <= -bounceX)
        {
            transform.position = new Vector3(-bounceX, transform.position.y, 0f);
            _dir = 1f;
        }

        _diveTimer -= Time.deltaTime;
        if (_diveTimer <= 0f)
        {
            _diveTimer = 4f;
            // Inline dive trigger
            if (_enemies.Count > 0)
            {
                float minY = float.MaxValue;
                foreach (var e in _enemies)
                    if (e != null && !e.IsDiving()) minY = Mathf.Min(minY, e.formationOffset.y);
                if (minY < float.MaxValue)
                {
                    var ca = _enemies.FindAll(e => e != null && !e.IsDiving() && e.formationOffset.y <= minY + 0.01f);
                    if (ca.Count > 0) ca[Random.Range(0, ca.Count)].StartCoroutine("DiveCoroutine");
                }
            }
        }
    }
}
