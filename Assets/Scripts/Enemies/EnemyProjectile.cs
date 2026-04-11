using UnityEngine;

/// <summary>Simple enemy projectile that damages players on contact.</summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    public float damage = 8f;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.gravityScale = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var ps = other.GetComponent<PlayerStats>();
        if (ps == null) return;

        var ctx = new DamageContext(damage, DamageType.Projectile, gameObject);
        ps.TakeDamage(ctx);
        Destroy(gameObject);
    }
}
