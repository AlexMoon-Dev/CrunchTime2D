using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Ranger/RapidFire", fileName = "Perk_RapidFire")]
public class RapidFirePerk : PerkSO
{
    public override void Apply(PlayerStats stats) => stats.MultiplyAttackSpeed(1.20f);
    // Projectile size reduction handled in Projectile.cs via HasPerk("Rapid Fire")
}
