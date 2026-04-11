using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Neutral/Edge", fileName = "Perk_Edge")]
public class EdgePerk : PerkSO
{
    public override void Apply(PlayerStats stats) => stats.AddCritChance(0.08f);
}
