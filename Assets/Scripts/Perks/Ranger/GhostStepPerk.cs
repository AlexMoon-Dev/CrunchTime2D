using System.Collections;
using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Ranger/GhostStep", fileName = "Perk_GhostStep")]
public class GhostStepPerk : PerkSO
{
    public float trailDamage   = 8f;
    public float trailDuration = 1f;
    public float trailRadius   = 0.8f;
    public GameObject trailPrefab;  // TODO: replace with real VFX

    public override void Equip(PlayerLeveling owner)
    {
        var ctrl   = GetCtrl(owner);
        var combat = GetCombat(owner);
        LayerMask mask = LayerMask.GetMask("Enemy");

        CombatEventSystem.OnPlayerDash += (pc) =>
        {
            if (pc != ctrl) return;
            owner.StartCoroutine(TrailRoutine(ctrl, combat, mask));
        };
    }

    private IEnumerator TrailRoutine(PlayerController ctrl, PlayerCombat combat, LayerMask mask)
    {
        float elapsed = 0f;
        while (elapsed < trailDuration)
        {
            // TODO: spawn trailPrefab at ctrl.transform.position
            var hits = Physics2D.OverlapCircleAll(ctrl.transform.position, trailRadius, mask);
            foreach (var h in hits)
            {
                var e = h.GetComponentInParent<EnemyBase>();
                if (e != null)
                {
                    var ctx = new DamageContext(trailDamage, DamageType.AoE, ctrl.gameObject);
                    combat.BuildAndApplyDamage(e, ctx);
                }
            }
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
    }
}
