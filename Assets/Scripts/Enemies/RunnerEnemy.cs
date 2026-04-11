using UnityEngine;

/// <summary>
/// Charges directly at the nearest player. Low HP, low damage, fast.
/// </summary>
public class RunnerEnemy : EnemyBase
{
    public override void Awake()
    {
        base.Awake();
        maxHealth  = 20f;
        damage     = 6f;
        moveSpeed  = 5f;
        xpValue    = 8f;
        _currentHealth = maxHealth;
    }

    protected override void Behave()
    {
        if (_target == null) return;
        Vector2 dir = ((Vector2)_target.position - (Vector2)transform.position).normalized;
        _rb.linearVelocity = new Vector2(dir.x * moveSpeed, _rb.linearVelocity.y);
    }
}
