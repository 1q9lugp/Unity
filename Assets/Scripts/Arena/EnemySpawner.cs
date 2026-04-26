using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Config")]
    public int   maxEnemies  = 100;
    public float spawnRadius = 40f;

    [Header("Templates - assign inactive scene objects in Inspector")]
    public GameObject gruntTemplate;
    public GameObject eliteTemplate;

    int _active;

    void Start()
    {
        for (int i = 0; i < maxEnemies; i++) Spawn(i);
    }

    public void OnEnemyDied()
    {
        _active = Mathf.Max(0, _active - 1);
        Spawn(_active);
    }

    void Spawn(int _)
    {
        if (_active >= maxEnemies) return;

        bool       isElite = Random.value < 0.2f;
        GameObject tmpl    = isElite ? eliteTemplate : gruntTemplate;
        if (tmpl == null) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        Vector3 origin = player != null ? player.transform.position : Vector3.zero;
        float   a      = Random.Range(0f, Mathf.PI * 2f);
Vector3 pos = origin + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * spawnRadius;
        var go    = Instantiate(tmpl, pos, Quaternion.identity);
        go.SetActive(true);

        var enemy = go.GetComponent<LizardEnemy>();
        if (enemy != null)
        {
            enemy.health    = isElite ? 10 : 3;
            enemy.moveSpeed = isElite ? 2.5f : 3.8f;
        }

        _active++;
    }
}
