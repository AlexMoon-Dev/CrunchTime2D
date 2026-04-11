using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Tank/Colossus", fileName = "Perk_Colossus")]
public class ColossusPerk : PerkSO
{
    public override void Apply(PlayerStats stats)
    {
        stats.AddMaxHealth(30f);
        stats.AddArmor(5f);
    }
}
