using UnityEngine;

[CreateAssetMenu(menuName = "CrunchTime/Perks/Neutral/Regeneration", fileName = "Perk_Regeneration")]
public class RegenerationPerk : PerkSO
{
    public float regenBonus = 3f;   // additional hp/s granted by this perk

    public override void Apply(PlayerStats stats) => stats.AddHealthRegen(regenBonus);
}
