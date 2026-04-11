using UnityEngine;

public enum DamageType { Melee, Projectile, AoE, DoT }
public enum ClassType { None, Tank, Fighter, Ranger }

/// <summary>
/// Mutable damage payload passed through the combat event pipeline.
/// Perks modify this object rather than touching attack logic directly.
/// </summary>
public class DamageContext
{
    public float baseDamage;
    public float finalDamage;
    public DamageType damageType;
    public bool isCrit;
    public GameObject source;        // The attacker's GameObject
    public Vector2 knockback;
    public float damageMultiplier = 1f;
    public bool cancelled = false;
    // Extra data bag for perks that need to pass flags forward
    public System.Collections.Generic.Dictionary<string, object> extras
        = new System.Collections.Generic.Dictionary<string, object>();

    public DamageContext(float damage, DamageType type, GameObject src, Vector2 kb = default)
    {
        baseDamage    = damage;
        finalDamage   = damage;
        damageType    = type;
        isCrit        = false;
        source        = src;
        knockback     = kb;
    }

    /// <summary>Call before applying to commit multiplier into finalDamage.</summary>
    public void Resolve()
    {
        finalDamage = baseDamage * damageMultiplier;
    }
}
