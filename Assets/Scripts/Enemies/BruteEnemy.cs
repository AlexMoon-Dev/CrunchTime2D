using UnityEngine;

/// <summary>
/// Slow, tanky melee enemy. Applies heavy knockback on hit.
/// </summary>
public class BruteEnemy : EnemyBase
{
    [Header("Brute")]
    public float attackCooldown = 1.5f;

    private float _atkTimer;

    public override void Awake()
    {
        base.Awake();
        maxHealth      = 100f;
        damage         = 18f;
        moveSpeed      = 1.8f;
        xpValue        = 25f;
        knockbackForce = 8f;
        armor          = 5f;
        _currentHealth = maxHealth;
    }

    protected override void Behave()
    {
        if (_target == null) return;

        float dist = Vector2.Distance(transform.position, _target.position);
        Vector2 dir = ((Vector2)_target.position - (Vector2)transform.position).normalized;

        if (dist > 1.5f)
            _rb.linearVelocity = new Vector2(dir.x * moveSpeed, _rb.linearVelocity.y);
        else
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

        FaceTarget();
        AnimFloat("Speed", Mathf.Abs(_rb.linearVelocity.x));

        _atkTimer -= Time.deltaTime;
        if (_atkTimer <= 0f && dist <= 1.5f)
        {
            _atkTimer = attackCooldown;
            AnimTrigger("AttackTrigger");
            var ps = _target.GetComponent<PlayerStats>();
            if (ps != null) DamagePlayer(ps);
        }
    }
}
