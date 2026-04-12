using System.Collections;
using UnityEngine;
public class EnemyShip : MonoBehaviour
{
    public int hp = 1;
    public float shootInterval = 3f;
    public GameObject projectilePrefab;
    [HideInInspector] public FormationController formation;
    [HideInInspector] public Vector2 formationOffset;
    bool _diving, _dead;
    SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _sr.sortingOrder = 1;
    }

    void Start() => StartCoroutine(ShootLoop());

    public void TakeHit()
    {
        if (_dead) return;
        hp--;
        if (hp <= 0) { _dead = true; if (formation != null) formation.OnEnemyDestroyed(this); Destroy(gameObject); }
        else StartCoroutine(HitFlash());
    }

    public bool IsDiving() => _diving;

    IEnumerator HitFlash()
    {
        if (_sr) _sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        if (!_dead && _sr) _sr.color = Color.white;
    }

    IEnumerator ShootLoop()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, shootInterval));
        while (!_dead)
        {
            if (this == null) yield break;
            if (projectilePrefab != null)
            {
                var go = Instantiate(projectilePrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
                var p = go.GetComponent<SpaceProjectile>();
                if (p != null) p.isPlayerShot = false;
            }
            yield return new WaitForSeconds(Random.Range(shootInterval * 0.7f, shootInterval * 1.3f));
        }
    }

public IEnumerator DiveCoroutine()
    {
        if (_diving || _dead) yield break;
        _diving = true;
        var plGo = GameObject.FindWithTag("Player");
        float tx = plGo != null ? plGo.transform.position.x : transform.position.x;
        var target = new Vector3(tx, -10f, 0f);
        while (!_dead)
        {
            if (this == null) yield break;
            if (Vector3.Distance(transform.position, target) <= 0.08f) break;
            transform.position = Vector3.MoveTowards(transform.position, target, 8f * Time.deltaTime);
            yield return null;
        }
        if (this == null || _dead) yield break;
        float el = 0f;
        var from = transform.position;
        while (!_dead)
        {
            if (this == null) yield break;
            el += Time.deltaTime;
            if (el >= 2f) break;
            float t = Mathf.SmoothStep(0f, 1f, el / 2f);
            if (formation != null)
                transform.position = Vector3.Lerp(from, (Vector3)formation.GetFormationWorldPos(formationOffset), t);
            yield return null;
        }
        if (this != null && !_dead) _diving = false;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (_dead) return;
        if (col.CompareTag("Player"))
        {
            var lm = FindObjectOfType<LivesManager>();
            if (lm != null) lm.LoseLife();
            _dead = true;
            if (formation != null) formation.OnEnemyDestroyed(this);
            Destroy(gameObject);
        }
    }
}
