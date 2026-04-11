using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Ranger/Ricochet", fileName = "Perk_Ricochet")]
public class RicochetPerk : PerkSO
{
    public override void Equip(PlayerLeveling owner)
    {
        // Projectile.cs checks this flag via GetComponentInParent<PlayerLeveling>().HasPerk
        // Flag is already registered by PlayerLeveling.GrantPerk -> _perkNames
    }
}
