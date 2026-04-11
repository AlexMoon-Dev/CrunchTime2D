using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Fighter/StreetBrawler", fileName = "Perk_StreetBrawler")]
public class StreetBrawlerPerk : PerkSO
{
    public int   stacksRequired = 5;
    public float bonusDamage    = 0.50f;

    public override void Equip(PlayerLeveling owner)
    {
        var combat = GetCombat(owner);
        var stacks = new Dictionary<EnemyBase, int>();
        bool primed = false;

        CombatEventSystem.OnBeforePlayerDamagesEnemy += (pc, enemy, ctx) =>
        {
            if (pc != combat) return;
            if (primed) { ctx.damageMultiplier *= (1f + bonusDamage); primed = false; stacks[enemy] = 0; return; }
            if (!stacks.ContainsKey(enemy)) stacks[enemy] = 0;
            stacks[enemy]++;
            if (stacks[enemy] >= stacksRequired) { primed = true; stacks[enemy] = 0; }
        };
    }
}
