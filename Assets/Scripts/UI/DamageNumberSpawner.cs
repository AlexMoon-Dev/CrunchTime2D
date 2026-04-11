using UnityEngine;

/// <summary>
/// Listens to CombatEventSystem and spawns floating damage numbers over enemies.
/// Add this component to any persistent scene object (e.g. GameManager).
/// </summary>
public class DamageNumberSpawner : MonoBehaviour
{
    public DamageNumber prefab;

    private void OnEnable()
    {
        CombatEventSystem.OnAfterPlayerDamagesEnemy += OnDamageDealt;
    }

    private void OnDisable()
    {
        CombatEventSystem.OnAfterPlayerDamagesEnemy -= OnDamageDealt;
    }

    private void OnDamageDealt(PlayerCombat attacker, EnemyBase enemy, DamageContext ctx)
    {
        if (prefab == null || enemy == null) return;

        // Show health actually lost (final damage minus enemy armor)
        float actual = Mathf.Max(0f, ctx.finalDamage - enemy.armor);
        if (actual <= 0f) return;

        var num = Instantiate(prefab);
        num.Show(actual, ctx.isCrit, enemy.transform.position);
    }
}
