using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Tank/GroundShaker", fileName = "Perk_GroundShaker")]
public class GroundShakerPerk : PerkSO
{
    public GameObject shockwavePrefab;  // TODO: replace with real art/prefab

    public override void Equip(PlayerLeveling owner)
    {
        var stats  = GetStats(owner);
        var combat = GetCombat(owner);
        // Append a shockwave on heavy attack after damage resolves
        CombatEventSystem.OnAfterPlayerDamagesEnemy += (pc, enemy, ctx) =>
        {
            if (pc != combat || ctx.damageType != DamageType.Melee) return;
            if (shockwavePrefab != null)
                Object.Instantiate(shockwavePrefab, stats.transform.position, Quaternion.identity);
            // Fallback inline: AoE along ground
            else
            {
                var hits = Physics2D.OverlapBoxAll(
                    (Vector2)stats.transform.position + Vector2.right * combat.GetComponent<PlayerController>().FacingDir * 2f,
                    new Vector2(5f, 0.5f), 0f, LayerMask.GetMask("Enemy"));
                foreach (var h in hits)
                {
                    var e = h.GetComponentInParent<EnemyBase>();
                    if (e != null && e != enemy)
                    {
                        var wCtx = new DamageContext(stats.attackDamage * 0.5f, DamageType.AoE, stats.gameObject);
                        combat.BuildAndApplyDamage(e, wCtx);
                    }
                }
            }
        };
    }
}
