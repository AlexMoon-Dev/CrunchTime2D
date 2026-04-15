using System;
using UnityEngine;

/// <summary>
/// All runtime stats for a player. Perks call the Add* helpers so that
/// the change-event fires and the HUD can react.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats — set these on the prefab")]
    public float maxHealth     = 100f;
    public float armor         = 0f;       // flat damage reduction
    public float attackDamage  = 30f;
    public float attackSpeed   = 2f;       // attacks per second multiplier
    public float moveSpeed     = 6f;
    public float critChance    = 0.05f;    // 0–1
    public float critMultiplier = 1.5f;
    public float dashCooldown  = 1.5f;     // seconds
    public float healthRegen   = 2f;       // hp restored per second (passive)
    public int   playerIndex   = 0;        // 0 = P1, 1 = P2

    [Header("Runtime (read-only in inspector)")]
    [SerializeField] private float _currentHealth;

    public float CurrentHealth => _currentHealth;
    public bool  IsDead        => _currentHealth <= 0f;
    public ClassType EquippedClass { get; set; } = ClassType.None;

    public event Action<float, float> OnHealthChanged;   // (current, max)
    public event Action              OnDeath;

    // Knocked-back immunity (set by Immovable perk)
    public bool immuneToKnockback = false;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    private void Update()
    {
        if (IsDead || _currentHealth >= maxHealth) return;
        Heal(healthRegen * Time.deltaTime);
    }

    // ── Damage ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Apply a DamageContext to this player. Armor reduces flat.
    /// </summary>
    public void TakeDamage(DamageContext ctx)
    {
        if (IsDead) return;

        CombatEventSystem.RaisePlayerHit(this, ctx);
        if (ctx.cancelled) return;

        ctx.Resolve();
        float mitigated = Mathf.Max(0f, ctx.finalDamage - armor);
        _currentHealth = Mathf.Max(0f, _currentHealth - mitigated);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        if (!immuneToKnockback && ctx.knockback != Vector2.zero)
        {
            var rb = GetComponent<Rigidbody2D>();
            rb?.AddForce(ctx.knockback, ForceMode2D.Impulse);
        }

        if (_currentHealth <= 0f)
        {
            GetComponent<PlayerController>()?.TriggerDieAnim();
            OnDeath?.Invoke();
        }
        else
        {
            GetComponent<PlayerVisualFeedback>()?.OnHit();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    /// <summary>
    /// Revives a dead player directly to the given health amount.
    /// Unlike Heal(), this bypasses the IsDead guard.
    /// </summary>
    public void ReviveWithHealth(float amount)
    {
        _currentHealth = Mathf.Clamp(amount, 0.01f, maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    // ── Stat helpers (perks call these) ──────────────────────────────────────

    public void AddMaxHealth(float delta)
    {
        maxHealth     += delta;
        _currentHealth = Mathf.Min(_currentHealth + delta, maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    public void AddHealthRegen(float delta)    => healthRegen   += delta;
    public void AddArmor(float delta)         => armor         += delta;
    public void AddAttackDamage(float delta)  => attackDamage  += delta;
    public void MultiplyAttackDamage(float f) => attackDamage  *= f;
    public void AddAttackSpeed(float delta)   => attackSpeed   += delta;
    public void MultiplyAttackSpeed(float f)  => attackSpeed   *= f;
    public void AddMoveSpeed(float delta)     => moveSpeed     += delta;
    public void MultiplyMoveSpeed(float f)    => moveSpeed     *= f;
    public void AddCritChance(float delta)    => critChance     = Mathf.Clamp01(critChance + delta);
    public void AddDashCooldown(float delta)  => dashCooldown  += delta;

    /// <summary>Roll a crit based on critChance, returns true if crit.</summary>
    public bool RollCrit() => UnityEngine.Random.value < critChance;
}
