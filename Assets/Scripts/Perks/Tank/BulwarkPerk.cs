using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Tank/ClassPerk_Bulwark", fileName = "ClassPerk_Bulwark")]
public class BulwarkPerk : PerkSO
{
    private void OnEnable() => isClassPerk = true;

    public override void Equip(PlayerLeveling owner)
    {
        var combat = GetCombat(owner);
        if (combat != null) combat.perkBulwark = true;
    }
}
