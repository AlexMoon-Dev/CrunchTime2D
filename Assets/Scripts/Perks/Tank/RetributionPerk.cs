using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Tank/Retribution", fileName = "Perk_Retribution")]
public class RetributionPerk : PerkSO
{
    public float reflectPercent = 0.15f;
    public float aoeRadius      = 2f;

    public override void Equip(PlayerLeveling owner)
    {
        var stats   = GetStats(owner);
        var combat  = GetCombat(owner);
        LayerMask enemyMask = LayerMask.GetMask("Enemy");

        CombatEventSystem.OnPlayerHit += (ps, ctx) =>
        {
            if (ps != stats) return;
            float reflected = ctx.finalDamage * reflectPercent;
            var hits = Physics2D.OverlapCircleAll(stats.transform.position, aoeRadius, enemyMask);
            foreach (var h in hits)
            {
                var enemy = h.GetComponentInParent<EnemyBase>();
                if (enemy != null)
                {
                    var rCtx = new DamageContext(reflected, DamageType.AoE, stats.gameObject);
                    combat?.BuildAndApplyDamage(enemy, rCtx);
                }
            }
        };
    }
}
