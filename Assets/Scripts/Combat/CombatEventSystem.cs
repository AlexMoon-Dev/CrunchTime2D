using System;
using UnityEngine;

/// <summary>
/// Global event bus for combat. Perks subscribe here — attack logic stays clean.
/// All hooks receive a mutable DamageContext; perks adjust multipliers/flags on it.
/// </summary>
public static class CombatEventSystem
{
    // Fired by PlayerCombat before damage is sent to enemy.
    // Perks can read/modify context.finalDamage, context.damageMultiplier, etc.
    public static event Action<PlayerCombat, EnemyBase, DamageContext> OnBeforePlayerDamagesEnemy;

    // Fired after damage has been applied (DamageContext is final/read-only by convention).
    public static event Action<PlayerCombat, EnemyBase, DamageContext> OnAfterPlayerDamagesEnemy;

    // Fired when a player kills an enemy.
    public static event Action<PlayerCombat, EnemyBase> OnPlayerKilledEnemy;

    // Fired on every dash start.
    public static event Action<PlayerController> OnPlayerDash;

    // Fired when a player's stats receive damage.
    public static event Action<PlayerStats, DamageContext> OnPlayerHit;

    // --- Raise helpers ---

    public static void RaiseBeforePlayerDamage(PlayerCombat attacker, EnemyBase target, DamageContext ctx)
        => OnBeforePlayerDamagesEnemy?.Invoke(attacker, target, ctx);

    public static void RaiseAfterPlayerDamage(PlayerCombat attacker, EnemyBase target, DamageContext ctx)
        => OnAfterPlayerDamagesEnemy?.Invoke(attacker, target, ctx);

    public static void RaisePlayerKilledEnemy(PlayerCombat attacker, EnemyBase enemy)
        => OnPlayerKilledEnemy?.Invoke(attacker, enemy);

    public static void RaisePlayerDash(PlayerController player)
        => OnPlayerDash?.Invoke(player);

    public static void RaisePlayerHit(PlayerStats player, DamageContext ctx)
        => OnPlayerHit?.Invoke(player, ctx);

    /// <summary>Clear all subscriptions — call on scene unload / game restart.</summary>
    public static void ClearAll()
    {
        OnBeforePlayerDamagesEnemy = null;
        OnAfterPlayerDamagesEnemy  = null;
        OnPlayerKilledEnemy        = null;
        OnPlayerDash               = null;
        OnPlayerHit                = null;
    }
}
