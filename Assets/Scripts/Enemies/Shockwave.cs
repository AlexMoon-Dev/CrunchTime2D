using UnityEngine;

/// <summary>
/// Placeholder shockwave that travels horizontally and damages players.
/// Attach to a thin prefab rectangle.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Shockwave : MonoBehaviour
{
    public float damage    = 25f;
    public int   direction = 1;
    public float speed     = 8f;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        GetComponent<Collider2D>().isTrigger = true;
        Destroy(gameObject, 4f);
    }

    private void Start()
    {
        _rb.linearVelocity = new Vector2(direction * speed, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var ps = other.GetComponent<PlayerStats>();
        if (ps == null) return;
        var ctx = new DamageContext(damage, DamageType.AoE, gameObject, new Vector2(direction * 5f, 4f));
        ps.TakeDamage(ctx);
    }
}
