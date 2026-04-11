using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Fighter/Relentless", fileName = "Perk_Relentless")]
public class RelentlessPerk : PerkSO
{
    public override void Apply(PlayerStats stats) => stats.AddDashCooldown(stats.dashCooldown * -0.30f);
}
