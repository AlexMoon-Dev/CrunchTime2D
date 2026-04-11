using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Ranger/Marked", fileName = "Perk_Marked")]
public class MarkedPerk : PerkSO
{
    public float bonusDamage = 0.25f;

    public override void Equip(PlayerLeveling owner)
    {
        var combat = GetCombat(owner);
        var markedEnemies = new HashSet<EnemyBase>();

        CombatEventSystem.OnBeforePlayerDamagesEnemy += (pc, enemy, ctx) =>
        {
            if (pc != combat) return;
            if (markedEnemies.Contains(enemy))
            {
                ctx.damageMultiplier *= (1f + bonusDamage);
                markedEnemies.Remove(enemy);
            }
            else
            {
                markedEnemies.Add(enemy);
            }
        };

        CombatEventSystem.OnPlayerKilledEnemy += (pc, enemy) =>
        {
            if (pc == combat) markedEnemies.Remove(enemy);
        };
    }
}
