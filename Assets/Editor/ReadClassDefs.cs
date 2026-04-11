using UnityEngine;
using UnityEditor;

public class ReadClassDefs
{
    public static void Execute()
    {
        string[] paths = {
            "Assets/Data/Classes/Tank.asset",
            "Assets/Data/Classes/Fighter.asset",
            "Assets/Data/Classes/Ranger.asset"
        };
        foreach (var path in paths)
        {
            var def = AssetDatabase.LoadAssetAtPath<ClassDefinitionSO>(path);
            if (def == null) { Debug.Log($"{path}: NOT FOUND"); continue; }
            Debug.Log($"{def.className}: maxHealth+{def.bonusMaxHealth}, armor+{def.bonusArmor}, " +
                      $"atk+{def.bonusAttackDamage}, move+{def.bonusMoveSpeed}, " +
                      $"atkSpd+{def.bonusAttackSpeed}, crit+{def.bonusCritChance}");
        }
    }
}
