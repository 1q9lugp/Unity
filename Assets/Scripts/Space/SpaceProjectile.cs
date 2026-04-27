using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpaceProjectile : MonoBehaviour
{
    [Header("General Settings")]
    public float speed = 20f;
    public float lifetime = 4f;
    public bool isPlayerShot = true;

    [Header("Enemy Sprite Settings")]
    public Sprite enemyProjectileSprite;
    
    private bool _hit = false;
    private SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    // Call this immediately after Instantiate to ensure logic is correct
    public void SetType(bool playerShot)
    {
        isPlayerShot = playerShot;
        speed = isPlayerShot ? 20f : 6f;
        SetupVisuals();
    }

    void Start()
    {
        SetupVisuals();
        Invoke(nameof(TimeoutDie), lifetime);
    }

    void SetupVisuals()
    {
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        _sr.sortingLayerName = "Default";
        _sr.sortingOrder = 20;

        if (isPlayerShot)
        {
            int tw = 6; int th = 40;
            var tex = new Texture2D(tw, th, TextureFormat.RGBA32, false);
            Color fill = new Color(0.4f, 0.95f, 1f, 1f);
            Color[] pixels = new Color[tw * th];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = fill;
            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            _sr.sprite = Sprite.Create(tex, new Rect(0f, 0f, tw, th), new Vector2(0.5f, 0f), 16f);
            _sr.color = Color.white;
        }
        else
        {
            if (enemyProjectileSprite != null)
            {
                _sr.sprite = enemyProjectileSprite;
                _sr.color = Color.white;
                transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            }
            else
            {
                
                int s = 6; // Smaller fallback (was 10)
                var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
                Color[] pixels = new Color[s * s];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.red;
                tex.SetPixels(pixels);
                tex.Apply();
                _sr.sprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 16f);
                transform.localScale = Vector3.one;
            
            }
        }
    }

    void Update()
    {
        if (_hit) { Destroy(gameObject); return; }
        float direction = isPlayerShot ? 1f : -1f;
        transform.Translate(0f, direction * speed * Time.deltaTime, 0f, Space.World);
        if (Mathf.Abs(transform.position.y) > 25f) _hit = true;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (_hit) return;

        if (isPlayerShot)
        {
            EnemyShip enemy = col.GetComponent<EnemyShip>();
            if (enemy != null)
            {
                enemy.TakeHit();
                _hit = true;
            }
        }
        else
        {
            SharaShipController player = col.GetComponent<SharaShipController>();
            if (player != null && !player.invincible)
            {
                player.TakeHit();
                _hit = true;
            }
        }
    }

    void TimeoutDie() { _hit = true; }
}