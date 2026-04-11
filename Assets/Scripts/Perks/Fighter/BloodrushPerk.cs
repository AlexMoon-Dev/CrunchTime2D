using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Fighter/Bloodrush", fileName = "Perk_Bloodrush")]
public class BloodrushPerk : PerkSO
{
    public float healPercent = 0.05f;

    public override void Equip(PlayerLeveling owner)
    {
        var combat = GetCombat(owner);
        var stats  = GetStats(owner);
        CombatEventSystem.OnPlayerKilledEnemy += (pc, _) =>
        {
            if (pc == combat) stats.Heal(stats.maxHealth * healPercent);
        };
    }
}
