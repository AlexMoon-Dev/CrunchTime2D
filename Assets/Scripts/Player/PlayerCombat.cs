using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles attack logic for all three classes.
/// Class-specific behaviour is switched by EquippedClass; perks hook in via CombatEventSystem.
/// </summary>
[RequireComponent(typeof(PlayerStats), typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Melee Hitbox Settings")]
    public float meleeRange      = 1.5f;
    public float heavyRange      = 2.0f;
    public LayerMask enemyLayers;

    [Header("Projectile")]
    public GameObject projectilePrefab;   // TODO: replace with real art
    public Transform  projectileSpawn;

    // ── Internal state ────────────────────────────────────────────────────────
    private PlayerStats      _stats;
    private PlayerController _ctrl;
    private float            _attackCooldown;

    // Fighter combo state
    private int   _comboHit     = 0;
    private float _comboResetTimer;
    private const float ComboResetTime = 0.8f;

    // Ranger charge state
    private bool  _heavyCharging;
    private float _chargeTime;
    private const float MaxChargeTime = 2f;

    // Class Perk flags (set by class perk scripts)
    [HideInInspector] public bool perkBulwark   = false; // Tank: heavy → shield bash
    [HideInInspector] public bool perkArsenal   = false; // Ranger: heavy → 3-shot spread

    private void Awake()
    {
        _stats = GetComponent<PlayerStats>();
        _ctrl  = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Wave) return;
        if (_stats.IsDead) return;

        _attackCooldown -= Time.deltaTime;

        // Fighter combo reset
        if (_comboHit > 0)
        {
            _comboResetTimer -= Time.deltaTime;
            if (_comboResetTimer <= 0f) _comboHit = 0;
        }

        // Ranger heavy charge tick
        if (_heavyCharging)
        {
            _chargeTime = Mathf.Min(_chargeTime + Time.deltaTime, MaxChargeTime);
        }
    }

    // ── Input callbacks ───────────────────────────────────────────────────────

    public void OnBasicAttack(InputValue value)
    {
        if (!value.isPressed) return;
        if (_attackCooldown > 0f) return;
        if (_stats.IsDead) return;

        DoBasicAttack();
    }

    public void OnHeavyAttack(InputValue value)
    {
        if (_stats.IsDead) return;

        if (_stats.EquippedClass == ClassType.Ranger && !perkArsenal)
        {
            // Hold to charge, release to fire
            if (value.isPressed)
            {
                _heavyCharging = true;
                _chargeTime    = 0f;
            }
            else
            {
                if (_heavyCharging)
                {
                    _heavyCharging = false;
                    DoRangerHeavy(_chargeTime);
                }
            }
        }
        else
        {
            if (value.isPressed && _attackCooldown <= 0f)
                DoHeavyAttack();
        }
    }

    // ── Attack implementations ────────────────────────────────────────────────

    private void DoBasicAttack()
    {
        float cooldown = 1f / _stats.attackSpeed;
        _attackCooldown = cooldown;

        switch (_stats.EquippedClass)
        {
            case ClassType.Tank:    DoTankBasic();    break;
            case ClassType.Fighter: DoFighterBasic(); break;
            case ClassType.Ranger:  DoRangerBasic();  break;
            default:                DoGenericMelee(meleeRange, _stats.attackDamage, Vector2.zero); break;
        }
    }

    private void DoHeavyAttack()
    {
        float cooldown = (1f / _stats.attackSpeed) * 1.8f;
        _attackCooldown = cooldown;

        switch (_stats.EquippedClass)
        {
            case ClassType.Tank:    DoTankHeavy();    break;
            case ClassType.Fighter: DoFighterHeavy(); break;
            case ClassType.Ranger:
                if (perkArsenal) DoRangerArsenal();
                else             DoRangerHeavy(MaxChargeTime); // fallback full charge
                break;
            default: DoGenericMelee(heavyRange, _stats.attackDamage * 1.5f, new Vector2(_ctrl.FacingDir * 5f, 2f)); break;
        }
    }

    // ── Tank ──────────────────────────────────────────────────────────────────

    private void DoTankBasic()
    {
        // Wide arc sweep
        var hits = Physics2D.OverlapCircleAll(transform.position, meleeRange, enemyLayers);
        foreach (var h in hits)
            ApplyMeleeDamage(h, _stats.attackDamage, Vector2.zero);
    }

    private void DoTankHeavy()
    {
        if (perkBulwark)
        {
            // Shield bash: damage scales with armor
            float dmg = _stats.attackDamage + _stats.armor * 0.5f;
            Vector2 kb = new Vector2(_ctrl.FacingDir * 10f, 3f);
            DoGenericMelee(heavyRange, dmg, kb);
        }
        else
        {
            // Ground slam: AoE in front
            float dmg = _stats.attackDamage * 1.8f;
            Vector2 offset = new Vector2(_ctrl.FacingDir * 0.5f, -0.3f);
            var hits = Physics2D.OverlapCircleAll((Vector2)transform.position + offset, heavyRange * 1.2f, enemyLayers);
            Vector2 kb = new Vector2(0f, 4f);
            foreach (var h in hits)
                ApplyMeleeDamage(h, dmg, kb);
        }
    }

    // ── Fighter ───────────────────────────────────────────────────────────────

    private void DoFighterBasic()
    {
        _comboHit++;
        _comboResetTimer = ComboResetTime;

        // Perk Flurry extends combo to 3 hits
        int maxCombo = HasPerk("Flurry") ? 3 : 2;
        float dmgMult = _comboHit >= maxCombo ? 1.4f : 1f; // last hit bonus
        if (_comboHit >= maxCombo) _comboHit = 0;

        DoGenericMelee(meleeRange, _stats.attackDamage * dmgMult, Vector2.zero);
    }

    private void DoFighterHeavy()
    {
        // Forward lunge — move player slightly and damage
        Vector2 lungeDir = new Vector2(_ctrl.FacingDir, 0f);
        GetComponent<Rigidbody2D>().AddForce(lungeDir * 8f, ForceMode2D.Impulse);
        var hits = Physics2D.OverlapBoxAll(
            (Vector2)transform.position + lungeDir * (heavyRange * 0.5f),
            new Vector2(heavyRange, 1.2f), 0f, enemyLayers);
        foreach (var h in hits)
            ApplyMeleeDamage(h, _stats.attackDamage * 2f, lungeDir * 6f);
    }

    // ── Ranger ────────────────────────────────────────────────────────────────

    private void DoRangerBasic()
    {
        FireProjectile(_ctrl.AimDirection, _stats.attackDamage, 1f);
    }

    private void DoRangerHeavy(float chargeRatio)
    {
        float dmg = _stats.attackDamage * (1f + chargeRatio * 1.5f);
        float sizeScale = 1f + chargeRatio * 0.5f;
        FireProjectile(_ctrl.AimDirection, dmg, sizeScale);
    }

    private void DoRangerArsenal()
    {
        // 3-shot spread
        float[] angles = { -15f, 0f, 15f };
        foreach (float a in angles)
        {
            Vector2 dir = Quaternion.Euler(0, 0, a) * _ctrl.AimDirection;
            FireProjectile(dir, _stats.attackDamage * 0.8f, 1f);
        }
    }

    // ── Shared helpers ────────────────────────────────────────────────────────

    private void DoGenericMelee(float range, float damage, Vector2 kb)
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(_ctrl.FacingDir * range * 0.5f, 0f);
        var hits = Physics2D.OverlapBoxAll(origin, new Vector2(range, 1.5f), 0f, enemyLayers);
        foreach (var h in hits)
            ApplyMeleeDamage(h, damage, kb);
    }

    private void ApplyMeleeDamage(Collider2D col, float damage, Vector2 kb)
    {
        var enemy = col.GetComponentInParent<EnemyBase>();
        if (enemy == null) return;

        var ctx = new DamageContext(damage, DamageType.Melee, gameObject, kb);
        BuildAndApplyDamage(enemy, ctx);
    }

    public void FireProjectile(Vector2 direction, float damage, float sizeScale)
    {
        if (projectilePrefab == null) return;
        var spawnPos = projectileSpawn != null ? projectileSpawn.position : transform.position;
        var go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        go.transform.localScale = Vector3.one * sizeScale;
        var proj = go.GetComponent<Projectile>();
        if (proj != null)
            proj.Init(direction, damage, this);
    }

    /// <summary>Run damage through the event pipeline then apply to enemy.</summary>
    public void BuildAndApplyDamage(EnemyBase enemy, DamageContext ctx)
    {
        // Crit roll
        if (_stats.RollCrit())
        {
            ctx.isCrit           = true;
            ctx.damageMultiplier *= _stats.critMultiplier;
        }

        CombatEventSystem.RaiseBeforePlayerDamage(this, enemy, ctx);
        if (ctx.cancelled) return;

        ctx.Resolve();
        bool killed = enemy.TakeDamage(ctx);
        CombatEventSystem.RaiseAfterPlayerDamage(this, enemy, ctx);

        if (killed)
            CombatEventSystem.RaisePlayerKilledEnemy(this, enemy);
    }

    // Quick perk check — PerkRuntimeData is a simple list we search by name
    private bool HasPerk(string perkName)
    {
        var leveling = GetComponent<PlayerLeveling>();
        return leveling != null && leveling.HasPerk(perkName);
    }

    // Expose stats ref for projectile / perk use
    public PlayerStats Stats => _stats;
}
