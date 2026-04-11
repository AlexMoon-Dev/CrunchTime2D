using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CrunchTime/Class Definition", fileName = "NewClassDef")]
public class ClassDefinitionSO : ScriptableObject
{
    [Header("Identity")]
    public ClassType classType;
    public string    className;
    [TextArea] public string description;
    public Sprite    classIcon;   // TODO: assign art

    [Header("Base Stat Modifiers (additive on top of prefab defaults)")]
    public float bonusMaxHealth    = 0f;
    public float bonusArmor        = 0f;
    public float bonusAttackDamage = 0f;
    public float bonusMoveSpeed    = 0f;
    public float bonusAttackSpeed  = 0f;
    public float bonusCritChance   = 0f;

    [Header("Class Perks (offered every 10 levels)")]
    public List<PerkSO> classPerks = new List<PerkSO>();

    /// <summary>Apply base stat bonuses to a player on class selection.</summary>
    public void ApplyBaseStats(PlayerStats stats)
    {
        stats.AddMaxHealth(bonusMaxHealth);
        stats.AddArmor(bonusArmor);
        stats.AddAttackDamage(bonusAttackDamage);
        stats.AddMoveSpeed(bonusMoveSpeed);
        stats.AddAttackSpeed(bonusAttackSpeed);
        stats.AddCritChance(bonusCritChance);
        stats.EquippedClass = classType;
    }
}
