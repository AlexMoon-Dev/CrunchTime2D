using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Ranger/ExplosiveRound", fileName = "Perk_ExplosiveRound")]
public class ExplosiveRoundPerk : PerkSO
{
    public float explosionRadius = 2f;

    public override void Equip(PlayerLeveling owner)
    {
        var combat    = GetCombat(owner);
        var stats     = GetStats(owner);
        LayerMask mask = LayerMask.GetMask("Enemy");

        CombatEventSystem.OnAfterPlayerDamagesEnemy += (pc, enemy, ctx) =>
        {
            if (pc != combat || !ctx.extras.ContainsKey("isHeavy")) return;
            // AoE at impact point
            var hits = Physics2D.OverlapCircleAll(enemy.transform.position, explosionRadius, mask);
            foreach (var h in hits)
            {
                var e = h.GetComponentInParent<EnemyBase>();
                if (e != null && e != enemy)
                {
                    var aCtx = new DamageContext(stats.attackDamage * 0.7f, DamageType.AoE, stats.gameObject);
                    combat.BuildAndApplyDamage(e, aCtx);
                }
            }
        };
    }
}
