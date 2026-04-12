using System.Collections;
using UnityEngine;

public class AshtarBeam : MonoBehaviour
{
    public float damage   = 9999f;
    public float duration = 0.45f;

    const float BeamHeight = 28f;
    const float BeamWidth  = 1.6f;

    SpriteRenderer _sr;

    void Awake()
    {
        // Build sprite inline
        const int S = 16;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var px = new Color[S * S];
        for (int i = 0; i < px.Length; i++)
        {
            float dx = (((float)(i % S) + 0.5f) / S - 0.5f) * 2f;
            float a  = Mathf.Clamp01((1f - Mathf.Abs(dx)) + Mathf.Clamp01(1f - Mathf.Abs(dx) * 2.5f));
            px[i] = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
        }
        tex.SetPixels(px);
        tex.Apply();
        var spr = Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), S);

        _sr = gameObject.AddComponent<SpriteRenderer>();
        _sr.sprite       = spr;
        _sr.color        = new Color(0.45f, 0.85f, 1f, 0.9f);
        _sr.sortingOrder = 6;
        transform.localScale = new Vector3(BeamWidth, BeamHeight, 1f);

        var bc       = gameObject.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size      = Vector2.one;

        Destroy(gameObject, duration);
        StartCoroutine(BeamFade(duration));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var e = other.GetComponent<EnemyShip>();
        if (e != null) { e.hp = 1; e.TakeHit(); return; }
        if (other.CompareTag("Enemy")) Destroy(other.gameObject);
    }

    IEnumerator BeamFade(float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float p     = t / dur;
            float flick = (p < 0.4f) ? (Mathf.Sin(t * 60f) * 0.15f + 0.85f) : 1f;
            float alpha = Mathf.Lerp(0.9f, 0f, Mathf.Pow(p, 1.5f)) * flick;
            if (_sr) _sr.color = new Color(0.45f, 0.85f, 1f, alpha);
            yield return null;
        }
    }
}
