using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Tank/IronHide", fileName = "Perk_IronHide")]
public class IronHidePerk : PerkSO
{
    public override void Apply(PlayerStats stats)
    {
        stats.AddArmor(15f);
        // 10% incoming damage reduction implemented as extra armor equivalent
        // Real reduction hook is in Equip via OnPlayerHit
    }

    public override void Equip(PlayerLeveling owner)
    {
        var stats = GetStats(owner);
        CombatEventSystem.OnPlayerHit += (ps, ctx) =>
        {
            if (ps == stats) ctx.damageMultiplier *= 0.90f;
        };
    }
}
