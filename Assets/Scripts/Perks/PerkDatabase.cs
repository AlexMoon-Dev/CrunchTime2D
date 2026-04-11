using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all PerkSO assets. Assign perks in the Inspector or place them all
/// in Resources/Perks/ and call LoadFromResources() at startup.
/// </summary>
[CreateAssetMenu(menuName = "CrunchTime/Perk Database", fileName = "PerkDatabase")]
public class PerkDatabase : ScriptableObject
{
    [Tooltip("All non-class perks. Drag every PerkSO here.")]
    public List<PerkSO> allPerks = new List<PerkSO>();

    /// <summary>
    /// Auto-load all PerkSO assets from Resources/Perks/.
    /// Call this at game start if you don't want to assign them manually.
    /// </summary>
    public void LoadFromResources()
    {
        allPerks.Clear();
        allPerks.AddRange(Resources.LoadAll<PerkSO>("Perks"));
    }

    /// <summary>
    /// Returns <paramref name="count"/> unique perks weighted by the player's class.
    /// Excludes perks the player already owns and class perks (those have a separate path).
    /// </summary>
    public List<PerkSO> GetWeightedSelection(ClassType playerClass, List<string> ownedPerkNames,
        int count = 3, bool classPerksOnly = false)
    {
        // Build weighted pool
        var pool = new List<(PerkSO perk, int weight)>();
        foreach (var perk in allPerks)
        {
            if (perk.isClassPerk != classPerksOnly) continue;
            if (ownedPerkNames.Contains(perk.perkName)) continue;

            int w = GetWeight(perk, playerClass);
            if (w > 0) pool.Add((perk, w));
        }

        var result = new List<PerkSO>();
        var used   = new HashSet<int>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int totalWeight = 0;
            foreach (var (_, w) in pool) totalWeight += w;

            int roll = Random.Range(0, totalWeight);
            int cum  = 0;
            for (int j = 0; j < pool.Count; j++)
            {
                cum += pool[j].weight;
                if (roll < cum && !used.Contains(j))
                {
                    result.Add(pool[j].perk);
                    used.Add(j);
                    pool.RemoveAt(j);
                    break;
                }
            }
        }

        return result;
    }

    private int GetWeight(PerkSO perk, ClassType cls) => cls switch
    {
        ClassType.Tank    => perk.tankWeight,
        ClassType.Fighter => perk.fighterWeight,
        ClassType.Ranger  => perk.rangerWeight,
        _                 => (perk.tankWeight + perk.fighterWeight + perk.rangerWeight) / 3
    };
}
