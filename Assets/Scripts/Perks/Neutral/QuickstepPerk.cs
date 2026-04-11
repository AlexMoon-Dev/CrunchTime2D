using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Neutral/Quickstep", fileName = "Perk_Quickstep")]
public class QuickstepPerk : PerkSO
{
    public override void Apply(PlayerStats stats) => stats.MultiplyMoveSpeed(1.10f);
}
