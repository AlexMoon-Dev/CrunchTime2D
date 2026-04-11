using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Neutral/BruteForce", fileName = "Perk_BruteForce")]
public class BruteForcePerk : PerkSO
{
    public override void Apply(PlayerStats stats) => stats.MultiplyAttackDamage(1.10f);
}
