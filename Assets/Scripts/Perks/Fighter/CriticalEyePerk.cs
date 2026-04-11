using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Fighter/CriticalEye", fileName = "Perk_CriticalEye")]
public class CriticalEyePerk : PerkSO
{
    public override void Apply(PlayerStats stats)
    {
        stats.AddCritChance(0.12f);
        stats.critMultiplier = 1.75f;
    }
}
