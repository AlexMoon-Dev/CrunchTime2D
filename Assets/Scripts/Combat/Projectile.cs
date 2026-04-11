using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed    = 18f;
    public float lifetime = 5f;
    public LayerMask hitLayers;

    private float         _damage;
    private PlayerCombat  _owner;
    private Vector2       _direction;
    private int           _bounceLeft = 0;
    private Rigidbody2D   _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        GetComponent<Collider2D>().isTrigger = true;
        Destroy(gameObject, lifetime);
    }

    public void Init(Vector2 direction, float damage, PlayerCombat owner)
    {
        _direction = direction.normalized;
        _damage    = damage;
        _owner     = owner;
        _rb.linearVelocity = _direction * speed;

        // Perk: Ricochet — bounce to 1 extra enemy
        var leveling = owner.GetComponent<PlayerLeveling>();
        if (leveling != null && leveling.HasPerk("Ricochet"))
            _bounceLeft = 1;

        // Perk: Rapid Fire — shrink projectile slightly
        if (leveling != null && leveling.HasPerk("Rapid Fire"))
            transform.localScale *= 0.75f;

        // TODO: rotate sprite to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy == null) return;

        var ctx = new DamageContext(_damage, DamageType.Projectile, _owner.gameObject);
        _owner.BuildAndApplyDamage(enemy, ctx);

        if (_bounceLeft > 0)
        {
            _bounceLeft--;
            BounceToNearestEnemy(enemy);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void BounceToNearestEnemy(EnemyBase justHit)
    {
        EnemyBase nearest = null;
        float bestDist = float.MaxValue;
        var allEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (var e in allEnemies)
        {
            if (e == justHit || e.IsDead) continue;
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < bestDist) { bestDist = d; nearest = e; }
        }

        if (nearest != null)
        {
            Vector2 dir = ((Vector2)nearest.transform.position - (Vector2)transform.position).normalized;
            _rb.linearVelocity = dir * speed;
            _direction = dir;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
