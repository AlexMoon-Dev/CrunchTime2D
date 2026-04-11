using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Tank/Immovable", fileName = "Perk_Immovable")]
public class ImmovablePerk : PerkSO
{
    public override void Apply(PlayerStats stats)
    {
        stats.AddArmor(10f);
        stats.immuneToKnockback = true;
    }
}
