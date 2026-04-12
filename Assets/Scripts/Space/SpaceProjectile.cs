using UnityEngine;

// SharaBeam: isPlayerShot=true, speed=20 | EnemyProjectile: isPlayerShot=false, speed=6
public class SpaceProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 4f;
    public bool isPlayerShot = true;
    bool _hit;

    void Start()
    {
        if (!isPlayerShot) speed = 6f;
        Invoke(nameof(TimeoutDie), lifetime);

        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 20;

        int tw = isPlayerShot ? 6 : 10;
        int th = isPlayerShot ? 40 : 10;
        var tex = new Texture2D(tw, th, TextureFormat.RGBA32, false);
        Color fill = isPlayerShot ? new Color(0.4f, 0.95f, 1f, 1f) : new Color(1f, 0.2f, 0.1f, 1f);
        Color[] pixels = new Color[tw * th];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = fill;
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        float py = isPlayerShot ? 0f : 0.5f;
        sr.sprite = Sprite.Create(tex, new Rect(0f, 0f, tw, th), new Vector2(0.5f, py), 16f);
        sr.color = Color.white;
    }

    void TimeoutDie() { _hit = true; }

    void Update()
    {
        float dir = isPlayerShot ? 1f : -1f;
        transform.Translate(0f, dir * speed * Time.deltaTime, 0f, Space.World);
        if (!_hit && Mathf.Abs(transform.position.y) > 20f) _hit = true;
        if (_hit) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (_hit) return;
        if (isPlayerShot)
        {
            var e = col.GetComponent<EnemyShip>();
            if (e != null)
            {
                e.TakeHit();
                _hit = true;
            }
        }
        else
        {
            var s = col.GetComponent<SharaShipController>();
            if (s != null)
            {
                s.TakeHit();
                _hit = true;
            }
        }
    }
}
