using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Neutral/Plating", fileName = "Perk_Plating")]
public class PlatingPerk : PerkSO
{
    public float armorBonus = 5f;
    public override void Apply(PlayerStats stats) => stats.AddArmor(armorBonus);
}
