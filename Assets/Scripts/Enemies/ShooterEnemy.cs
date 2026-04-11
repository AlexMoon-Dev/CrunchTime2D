using System.Collections;
using UnityEngine;

/// <summary>
/// Keeps distance from nearest player and fires slow projectiles.
/// </summary>
public class ShooterEnemy : EnemyBase
{
    [Header("Shooter")]
    public float preferredDistance = 6f;
    public float fireRate          = 1.5f;
    public float projectileSpeed   = 6f;
    public GameObject projectilePrefab;   // TODO: assign placeholder prefab

    private float _fireCooldown;

    public override void Awake()
    {
        base.Awake();
        maxHealth      = 40f;
        damage         = 8f;
        moveSpeed      = 2f;
        xpValue        = 15f;
        _currentHealth = maxHealth;
    }

    protected override void Behave()
    {
        if (_target == null) return;

        float dist = Vector2.Distance(transform.position, _target.position);

        // Maintain preferred distance
        Vector2 dir = ((Vector2)_target.position - (Vector2)transform.position).normalized;
        if (dist < preferredDistance * 0.8f)
            _rb.linearVelocity = new Vector2(-dir.x * moveSpeed, _rb.linearVelocity.y);
        else if (dist > preferredDistance * 1.2f)
            _rb.linearVelocity = new Vector2(dir.x * moveSpeed, _rb.linearVelocity.y);
        else
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

        _fireCooldown -= Time.deltaTime;
        if (_fireCooldown <= 0f)
        {
            _fireCooldown = 1f / fireRate;
            FireAt(_target.position);
        }
    }

    private void FireAt(Vector3 targetPos)
    {
        if (projectilePrefab == null) return;
        var go  = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var rb  = go.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = ((Vector2)targetPos - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * projectileSpeed;
        }
        var ep = go.GetComponent<EnemyProjectile>();
        if (ep != null) ep.damage = damage;
        Destroy(go, 5f);
    }
}
