using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Neutral/Vitality", fileName = "Perk_Vitality")]
public class VitalityPerk : PerkSO
{
    public float healthBonus = 20f;
    public override void Apply(PlayerStats stats) => stats.AddMaxHealth(healthBonus);
}
