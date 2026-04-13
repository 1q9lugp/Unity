using UnityEngine;

/// Attach to the Background GameObject.
/// Watches for sprite changes each frame and auto-scales
/// the transform to fill the orthographic camera viewport.
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    SpriteRenderer _sr;
    Sprite _prev;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (_sr.sprite == _prev) return;   // nothing changed
        _prev = _sr.sprite;

        if (_sr.sprite == null) return;

        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        float worldH = cam.orthographicSize * 2f;
        float worldW = worldH * cam.aspect;

        Vector2 sz = _sr.sprite.bounds.size;
        if (sz.x > 0f && sz.y > 0f)
            transform.localScale = new Vector3(worldW / sz.x, worldH / sz.y, 1f);
    }
}
