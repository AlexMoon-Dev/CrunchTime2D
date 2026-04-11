using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Ranger/ClassPerk_Arsenal", fileName = "ClassPerk_Arsenal")]
public class ArsenalPerk : PerkSO
{
    private void OnEnable() => isClassPerk = true;

    public override void Equip(PlayerLeveling owner)
    {
        var combat = GetCombat(owner);
        if (combat != null) combat.perkArsenal = true;
    }
}
