using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Fighter/Tenacity", fileName = "Perk_Tenacity")]
public class TenacityPerk : PerkSO
{
    public override void Apply(PlayerStats stats)
    {
        stats.AddMaxHealth(20f);
        stats.MultiplyMoveSpeed(1.10f);
    }
}
