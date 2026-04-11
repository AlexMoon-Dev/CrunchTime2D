using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Neutral/Swiftness", fileName = "Perk_Swiftness")]
public class SwiftnessPerk : PerkSO
{
    public override void Apply(PlayerStats stats) => stats.MultiplyAttackSpeed(1.10f);
}
