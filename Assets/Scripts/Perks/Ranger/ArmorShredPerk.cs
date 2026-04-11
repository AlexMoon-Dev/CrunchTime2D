using System.Collections;
using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Ranger/ArmorShred", fileName = "Perk_ArmorShred")]
public class ArmorShredPerk : PerkSO
{
    public float shredPercent = 0.20f;
    public float duration     = 5f;

    public override void Equip(PlayerLeveling owner)
    {
        var combat = GetCombat(owner);
        CombatEventSystem.OnAfterPlayerDamagesEnemy += (pc, enemy, ctx) =>
        {
            if (pc != combat || ctx.damageType != DamageType.Projectile) return;
            // Only heavy attacks (handled by DamageContext extras flag)
            if (!ctx.extras.ContainsKey("isHeavy")) return;
            owner.StartCoroutine(ShredRoutine(enemy));
        };
    }

    private IEnumerator ShredRoutine(EnemyBase enemy)
    {
        float removed = enemy.armor * shredPercent;
        enemy.armor -= removed;
        yield return new WaitForSeconds(duration);
        if (enemy != null) enemy.armor += removed;
    }
}
