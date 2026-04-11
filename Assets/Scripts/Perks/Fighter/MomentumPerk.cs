using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Fighter/ClassPerk_Momentum", fileName = "ClassPerk_Momentum")]
public class MomentumPerk : PerkSO
{
    public float stackBonus = 0.05f;
    public int   maxStacks  = 6;  // 30% cap

    private void OnEnable() => isClassPerk = true;

    public override void Equip(PlayerLeveling owner)
    {
        var combat = GetCombat(owner);
        var stacks = new Dictionary<EnemyBase, int>();

        CombatEventSystem.OnBeforePlayerDamagesEnemy += (pc, enemy, ctx) =>
        {
            if (pc != combat) return;
            if (!stacks.ContainsKey(enemy)) stacks[enemy] = 0;
            stacks[enemy] = Mathf.Min(stacks[enemy] + 1, maxStacks);
            ctx.damageMultiplier *= (1f + stacks[enemy] * stackBonus);
        };

        CombatEventSystem.OnPlayerKilledEnemy += (pc, enemy) =>
        {
            if (pc == combat) stacks.Remove(enemy);
        };
    }
}
