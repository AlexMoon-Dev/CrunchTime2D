using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Ranger/PoisonTip", fileName = "Perk_PoisonTip")]
public class PoisonTipPerk : PerkSO
{
    public float dotDamage   = 5f;
    public float dotInterval = 0.5f;
    public float dotDuration = 3f;
    public int   stacksNeeded = 3;

    public override void Equip(PlayerLeveling owner)
    {
        var combat = GetCombat(owner);
        var poisonStacks = new Dictionary<EnemyBase, int>();

        CombatEventSystem.OnAfterPlayerDamagesEnemy += (pc, enemy, ctx) =>
        {
            if (pc != combat || ctx.damageType != DamageType.Projectile) return;
            if (!poisonStacks.ContainsKey(enemy)) poisonStacks[enemy] = 0;
            poisonStacks[enemy]++;
            if (poisonStacks[enemy] >= stacksNeeded)
            {
                poisonStacks[enemy] = 0;
                owner.StartCoroutine(ApplyPoison(enemy, combat));
            }
        };
    }

    private IEnumerator ApplyPoison(EnemyBase enemy, PlayerCombat combat)
    {
        float elapsed = 0f;
        while (elapsed < dotDuration && enemy != null && !enemy.IsDead)
        {
            yield return new WaitForSeconds(dotInterval);
            elapsed += dotInterval;
            var ctx = new DamageContext(dotDamage, DamageType.DoT, combat.gameObject);
            combat.BuildAndApplyDamage(enemy, ctx);
        }
    }
}
