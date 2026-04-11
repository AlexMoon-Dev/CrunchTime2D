using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Tank/ClassPerk_Fortress", fileName = "ClassPerk_Fortress")]
public class FortressPerk : PerkSO
{
    public float allyDamageReduction = 0.20f;
    public float auraRadius          = 5f;

    private void OnEnable() => isClassPerk = true;

    public override void Equip(PlayerLeveling owner)
    {
        var myStats = GetStats(owner);
        // Nearby allies take 20% less damage
        CombatEventSystem.OnPlayerHit += (ps, ctx) =>
        {
            if (ps == myStats) return;
            if (Vector2.Distance(ps.transform.position, myStats.transform.position) <= auraRadius)
                ctx.damageMultiplier *= (1f - allyDamageReduction);
        };
    }
}
