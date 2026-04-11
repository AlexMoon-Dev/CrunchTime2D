using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHealth  = 30f;
    public float damage     = 5f;
    public float moveSpeed  = 3f;
    public float xpValue    = 10f;
    public float armor      = 0f;
    public float knockbackForce = 4f;

    [Header("Runtime")]
    [SerializeField] protected float _currentHealth;

    public float CurrentHealth => _currentHealth;
    public bool  IsDead        => _currentHealth <= 0f;

    protected Rigidbody2D _rb;
    protected Transform   _target;
    protected bool        _stunned;

    public virtual void Awake()
    {
        _rb             = GetComponent<Rigidbody2D>();
        _currentHealth  = maxHealth;
    }

    public virtual void Start()
    {
        _target = FindClosestPlayer();
    }

    protected virtual void Update()
    {
        if (IsDead || _stunned) return;
        _target = FindClosestPlayer();
        Behave();
    }

    /// <summary>Override in subclass to implement AI movement/attack logic.</summary>
    protected virtual void Behave() { }

    // ── Damage ───────────────────────────────────────────────────────────────

    /// <summary>Returns true if this hit killed the enemy.</summary>
    public virtual bool TakeDamage(DamageContext ctx)
    {
        if (IsDead) return false;

        float mitigated = Mathf.Max(0f, ctx.finalDamage - armor);
        _currentHealth -= mitigated;

        // Knockback
        if (ctx.knockback != Vector2.zero)
            _rb.AddForce(ctx.knockback, ForceMode2D.Impulse);

        if (_currentHealth <= 0f)
        {
            Die();
            return true;
        }
        return false;
    }

    protected virtual void Die()
    {
        _currentHealth = 0f;
        // Award XP to all living players
        var players = FindObjectsByType<PlayerLeveling>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            var stats = p.GetComponent<PlayerStats>();
            if (stats != null && !stats.IsDead)
                p.AddXP(xpValue);
        }
        // TODO: play death VFX
        Destroy(gameObject);
    }

    public void ApplyDifficultyMultiplier(float multiplier)
    {
        maxHealth  *= multiplier;
        _currentHealth = maxHealth;
        damage     *= multiplier;
        xpValue    *= multiplier;
    }

    public void ApplyStun(float duration) => StartCoroutine(StunRoutine(duration));

    private IEnumerator StunRoutine(float duration)
    {
        _stunned = true;
        yield return new WaitForSeconds(duration);
        _stunned = false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    protected Transform FindClosestPlayer()
    {
        var allStats = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);
        Transform best = null;
        float bestDist  = float.MaxValue;
        foreach (var ps in allStats)
        {
            if (ps.IsDead) continue;
            float d = Vector2.Distance(transform.position, ps.transform.position);
            if (d < bestDist) { bestDist = d; best = ps.transform; }
        }
        return best;
    }

    protected void DamagePlayer(PlayerStats player)
    {
        var ctx = new DamageContext(damage, DamageType.Melee, gameObject,
            ((Vector2)(player.transform.position - transform.position)).normalized * knockbackForce);
        player.TakeDamage(ctx);
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        var ps = col.gameObject.GetComponent<PlayerStats>();
        if (ps != null && !IsDead) DamagePlayer(ps);
    }
}
