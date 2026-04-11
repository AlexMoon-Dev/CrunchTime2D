#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates all PerkSO asset instances and populates the PerkDatabase.
/// Run once via MCP, then delete.
/// </summary>
public static class CreatePerkAssets
{
    public static void Execute()
    {
        Directory.CreateDirectory("Assets/Data/Perks/Neutral");
        Directory.CreateDirectory("Assets/Data/Perks/Tank");
        Directory.CreateDirectory("Assets/Data/Perks/Fighter");
        Directory.CreateDirectory("Assets/Data/Perks/Ranger");
        Directory.CreateDirectory("Assets/Data/Perks/ClassPerks");

        // ── Neutral ───────────────────────────────────────────────────────────
        MakePerk<VitalityPerk>  ("Neutral/Vitality",    "Vitality",     "+20 Max Health",          5, 5, 5);
        MakePerk<PlatingPerk>   ("Neutral/Plating",     "Plating",      "+5 Armor",                5, 5, 5);
        MakePerk<BruteForcePerk>("Neutral/BruteForce",  "Brute Force",  "+10% Attack Damage",      5, 5, 5);
        MakePerk<SwiftnessPerk> ("Neutral/Swiftness",   "Swiftness",    "+10% Attack Speed",       5, 5, 5);
        MakePerk<EdgePerk>      ("Neutral/Edge",        "Edge",         "+8% Crit Chance",         5, 5, 5);
        MakePerk<QuickstepPerk> ("Neutral/Quickstep",   "Quickstep",    "+10% Move Speed",         5, 5, 5);

        // ── Tank ──────────────────────────────────────────────────────────────
        MakePerk<IronHidePerk>      ("Tank/IronHide",      "Iron Hide",      "+15 Armor, 10% less damage taken",        9, 3, 1);
        MakePerk<ColossusPerk>      ("Tank/Colossus",      "Colossus",       "+30 Max Health, +5 Armor",                8, 3, 1);
        MakePerk<RetributionPerk>   ("Tank/Retribution",   "Retribution",    "On taking damage, deal 15% back as AoE",  8, 4, 2);
        MakePerk<ImmovablePerk>     ("Tank/Immovable",     "Immovable",      "Immune to knockback, +10 Armor",          9, 2, 1);
        MakePerk<GroundShakerPerk>  ("Tank/GroundShaker",  "Ground Shaker",  "Heavy attack adds ground shockwave",      9, 3, 1);
        MakePerk<GuardiansOathPerk> ("Tank/GuardiansOath", "Guardian's Oath","When ally is below 25% HP, gain 30% DR",  9, 4, 2);
        MakePerk<AegisPerk>         ("Tank/Aegis",         "Aegis",          "Gain shield = 20% of armor every 10s",    9, 3, 1);
        MakePerk<ThunderclapPerk>   ("Tank/Thunderclap",   "Thunderclap",    "Basic attack 15% chance to stun 0.5s",    7, 5, 3);

        // ── Fighter ───────────────────────────────────────────────────────────
        MakePerk<FlurryPerk>       ("Fighter/Flurry",       "Flurry",       "+15% Atk Spd, 3rd combo hit",         4, 9, 4);
        MakePerk<BloodrushPerk>    ("Fighter/Bloodrush",    "Bloodrush",    "On kill, heal 5% Max HP",             4, 9, 5);
        MakePerk<RelentlessPerk>   ("Fighter/Relentless",   "Relentless",   "Dash cooldown -30%",                  4, 8, 6);
        MakePerk<CriticalEyePerk>  ("Fighter/CriticalEye",  "Critical Eye", "+12% Crit, crits deal 175%",          3, 8, 6);
        MakePerk<WarCryPerk>       ("Fighter/WarCry",       "War Cry",      "Every 20s, +25% damage for 5s",       4, 9, 4);
        MakePerk<StreetBrawlerPerk>("Fighter/StreetBrawler","Street Brawler","5 hits → next hit +50% damage",      4, 9, 4);
        MakePerk<TenacityPerk>     ("Fighter/Tenacity",     "Tenacity",     "+20 HP, +10% Move Speed",             5, 8, 5);
        MakePerk<IronWillPerk>     ("Fighter/IronWill",     "Iron Will",    "Below 25% HP: 50% DR for 5s (60s cd)",5, 9, 3);

        // ── Ranger ────────────────────────────────────────────────────────────
        MakePerk<RicochetPerk>      ("Ranger/Ricochet",       "Ricochet",       "Basic projectile bounces to 1 enemy",    1, 3, 9);
        MakePerk<MultiShotPerk>     ("Ranger/MultiShot",      "Multi-Shot",     "Every 4th basic attack fires 2 shots",   1, 3, 9);
        MakePerk<PoisonTipPerk>     ("Ranger/PoisonTip",      "Poison Tip",     "3 hits apply poison DoT",                1, 3, 9);
        MakePerk<ArmorShredPerk>    ("Ranger/ArmorShred",     "Armor Shred",    "Heavy attack -20% enemy armor for 5s",   1, 4, 9);
        MakePerk<RapidFirePerk>     ("Ranger/RapidFire",      "Rapid Fire",     "+20% Atk Spd, smaller projectile",       1, 3, 9);
        MakePerk<MarkedPerk>        ("Ranger/Marked",         "Marked",         "First hit marks; second hit +25% dmg",   1, 4, 8);
        MakePerk<ExplosiveRoundPerk>("Ranger/ExplosiveRound", "Explosive Round","Heavy attack explodes on impact (AoE)",  1, 3, 9);
        MakePerk<GhostStepPerk>     ("Ranger/GhostStep",      "Ghost Step",     "Dash leaves damaging trail for 1s",      2, 5, 9);

        // ── Class Perks ───────────────────────────────────────────────────────
        var bulwark  = MakePerk<BulwarkPerk> ("ClassPerks/Bulwark",   "Bulwark",   "Heavy → shield bash, scales with armor", 10, 0, 0, isClass: true);
        var fortress = MakePerk<FortressPerk>("ClassPerks/Fortress",  "Fortress",  "Nearby allies take 20% less damage",     10, 0, 0, isClass: true);
        var momentum = MakePerk<MomentumPerk>("ClassPerks/Momentum",  "Momentum",  "Consecutive hits on same enemy +5% dmg", 0, 10, 0, isClass: true);
        var adaptable= MakePerk<AdaptablePerk>("ClassPerks/Adaptable","Adaptable", "Every 5 levels gain +1 to all stats",    0, 10, 0, isClass: true);
        var predator = MakePerk<PredatorPerk>("ClassPerks/Predator",  "Predator",  "Enemies below 30% HP take 2x damage",    0, 0, 10, isClass: true);
        var arsenal  = MakePerk<ArsenalPerk> ("ClassPerks/Arsenal",   "Arsenal",   "Heavy → 3-shot spread instead of charge",0, 0, 10, isClass: true);

        // Wire class perks into ClassDefinitionSOs
        WireClassPerk("Assets/Data/Classes/Tank.asset",    bulwark, fortress);
        WireClassPerk("Assets/Data/Classes/Fighter.asset", momentum, adaptable);
        WireClassPerk("Assets/Data/Classes/Ranger.asset",  predator, arsenal);

        // Populate PerkDatabase with all non-class perks
        PopulateDatabase();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreatePerkAssets] All perk assets created and PerkDatabase populated.");
    }

    static PerkSO MakePerk<T>(string subpath, string perkName, string desc,
        int tank, int fighter, int ranger, bool isClass = false)
        where T : PerkSO
    {
        string path = $"Assets/Data/Perks/{subpath}.asset";
        if (File.Exists(path))
            return AssetDatabase.LoadAssetAtPath<PerkSO>(path);

        var perk           = ScriptableObject.CreateInstance<T>();
        perk.perkName      = perkName;
        perk.description   = desc;
        perk.tankWeight    = tank;
        perk.fighterWeight = fighter;
        perk.rangerWeight  = ranger;
        perk.isClassPerk   = isClass;
        AssetDatabase.CreateAsset(perk, path);
        return perk;
    }

    static void WireClassPerk(string classAssetPath, params PerkSO[] perks)
    {
        var def = AssetDatabase.LoadAssetAtPath<ClassDefinitionSO>(classAssetPath);
        if (def == null) return;
        var so  = new SerializedObject(def);
        var list = so.FindProperty("classPerks");
        list.arraySize = perks.Length;
        for (int i = 0; i < perks.Length; i++)
            list.GetArrayElementAtIndex(i).objectReferenceValue = perks[i];
        so.ApplyModifiedProperties();
    }

    static void PopulateDatabase()
    {
        var db = AssetDatabase.LoadAssetAtPath<PerkDatabase>("Assets/Data/PerkDatabase.asset");
        if (db == null) return;

        var allPerks = AssetDatabase.FindAssets("t:PerkSO", new[] { "Assets/Data/Perks" });
        var so = new SerializedObject(db);
        var list = so.FindProperty("allPerks");
        list.arraySize = 0;

        int idx = 0;
        foreach (var guid in allPerks)
        {
            var p = AssetDatabase.LoadAssetAtPath<PerkSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (p == null) continue;
            list.arraySize++;
            list.GetArrayElementAtIndex(idx++).objectReferenceValue = p;
        }
        so.ApplyModifiedProperties();
        Debug.Log($"[CreatePerkAssets] PerkDatabase populated with {idx} perks.");
    }
}
#endif
