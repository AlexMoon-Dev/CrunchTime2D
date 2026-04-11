using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Fighter/Flurry", fileName = "Perk_Flurry")]
public class FlurryPerk : PerkSO
{
    public override void Apply(PlayerStats stats) => stats.MultiplyAttackSpeed(1.15f);
    // The combo extension (3rd hit) is handled in PlayerCombat.HasPerk("Flurry")
}
