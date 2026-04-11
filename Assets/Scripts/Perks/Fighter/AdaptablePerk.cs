using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Fighter/ClassPerk_Adaptable", fileName = "ClassPerk_Adaptable")]
public class AdaptablePerk : PerkSO
{
    public int levelsPerBonus = 5;

    private void OnEnable() => isClassPerk = true;

    public override void Equip(PlayerLeveling owner)
    {
        int grantedAt = owner.Level;
        owner.OnLevelUp += (lv) =>
        {
            if ((lv.Level - grantedAt) % levelsPerBonus == 0)
            {
                var s = lv.GetComponent<PlayerStats>();
                s.AddMaxHealth(1f);
                s.AddArmor(1f);
                s.AddAttackDamage(1f);
                s.AddMoveSpeed(0.1f);
                s.AddAttackSpeed(0.1f);
                s.AddCritChance(0.01f);
            }
        };
    }
}
