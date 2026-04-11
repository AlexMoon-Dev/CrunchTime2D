using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Tank/Thunderclap", fileName = "Perk_Thunderclap")]
public class ThunderclapPerk : PerkSO
{
    public float stunChance    = 0.15f;
    public float stunDuration  = 0.5f;

    public override void Equip(PlayerLeveling owner)
    {
        var combat = GetCombat(owner);
        CombatEventSystem.OnAfterPlayerDamagesEnemy += (pc, enemy, ctx) =>
        {
            if (pc != combat || ctx.damageType != DamageType.Melee) return;
            if (Random.value < stunChance)
                enemy.ApplyStun(stunDuration);
        };
    }
}
