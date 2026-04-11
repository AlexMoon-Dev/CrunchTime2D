using System.Collections;
using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Fighter/IronWill", fileName = "Perk_IronWill")]
public class IronWillPerk : PerkSO
{
    public float threshold       = 0.25f;
    public float reductionAmount = 0.50f;
    public float buffDuration    = 5f;
    public float skillCooldown   = 60f;

    public override void Equip(PlayerLeveling owner)
    {
        var stats = GetStats(owner);
        bool onCooldown = false;

        CombatEventSystem.OnPlayerHit += (ps, ctx) =>
        {
            if (ps != stats || onCooldown) return;
            if (stats.CurrentHealth / stats.maxHealth < threshold)
            {
                onCooldown = true;
                owner.StartCoroutine(IronWillRoutine(stats, owner, () => onCooldown = false));
            }
        };
    }

    private IEnumerator IronWillRoutine(PlayerStats stats, PlayerLeveling owner, System.Action resetCooldown)
    {
        // Apply temporary reduction via a hook that expires
        bool active = true;
        System.Action<PlayerStats, DamageContext> handler = (ps, ctx) =>
        {
            if (ps == stats && active) ctx.damageMultiplier *= (1f - reductionAmount);
        };
        CombatEventSystem.OnPlayerHit += handler;
        yield return new WaitForSeconds(buffDuration);
        active = false;
        CombatEventSystem.OnPlayerHit -= handler;
        yield return new WaitForSeconds(skillCooldown - buffDuration);
        resetCooldown();
    }
}
