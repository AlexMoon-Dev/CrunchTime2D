using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Ranger/ClassPerk_Predator", fileName = "ClassPerk_Predator")]
public class PredatorPerk : PerkSO
{
    public float threshold = 0.30f;

    private void OnEnable() => isClassPerk = true;

    public override void Equip(PlayerLeveling owner)
    {
        var combat = GetCombat(owner);
        CombatEventSystem.OnBeforePlayerDamagesEnemy += (pc, enemy, ctx) =>
        {
            if (pc != combat) return;
            if (enemy.CurrentHealth / enemy.maxHealth < threshold)
                ctx.damageMultiplier *= 2f;
        };
    }
}
