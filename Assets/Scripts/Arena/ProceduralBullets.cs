using UnityEngine;

public class ProceduralBullet : MonoBehaviour
{
    public float speed    = 80f;
    public int   damage   = 1;
    public float lifetime = 3f;

    Vector3 _dir;
    bool    _dead;

    public void Fire(Vector3 direction)
    {
        _dir = direction.normalized;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (_dead) return;

        float   dist = speed * Time.deltaTime;
        Vector3 move = _dir * dist;

        // Raycast along the travel path this frame before moving
        if (Physics.Raycast(transform.position, _dir, out RaycastHit hit, dist + 0.1f))
        {
            var enemy = hit.collider.GetComponentInParent<LizardEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                _dead = true;
                Destroy(gameObject);
                return;
            }
        }

        transform.position += move;
    }
}