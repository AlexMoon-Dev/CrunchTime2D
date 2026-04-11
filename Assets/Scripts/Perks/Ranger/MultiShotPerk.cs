using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Ranger/MultiShot", fileName = "Perk_MultiShot")]
public class MultiShotPerk : PerkSO
{
    // Every 4th basic attack fires 2 projectiles.
    // Implemented in Projectile / PlayerCombat via HasPerk check + counter.
    // The perk simply registers itself; actual logic lives in PlayerCombat.
}
